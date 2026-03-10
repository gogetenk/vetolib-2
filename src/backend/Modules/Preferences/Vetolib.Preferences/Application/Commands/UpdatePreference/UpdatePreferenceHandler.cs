using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Commands.UpdatePreference;

internal class UpdatePreferenceHandler : IRequestHandler<UpdatePreferenceCommand, Result>
{
    private readonly PreferencesDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdatePreferenceHandler(PreferencesDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(UpdatePreferenceCommand cmd, CancellationToken ct)
    {
        var existing = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == cmd.UserId && p.Key == cmd.Key, ct);

        string? previousValue = existing?.Value;
        string newValue = cmd.Value;

        if (existing is null)
        {
            var createResult = UserPreference.Create(cmd.ClinicId, cmd.UserId, cmd.Key, cmd.Value);
            if (!createResult.IsSuccess)
                return Result.Invalid(createResult.ValidationErrors);

            _context.UserPreferences.Add(createResult.Value);
        }
        else
        {
            var updateResult = existing.Update(cmd.Value);
            if (!updateResult.IsSuccess)
                return updateResult;
        }

        var category = SystemDefaults.GetCategory(cmd.Key);
        var auditResult = ConsentAuditEntry.Create(
            cmd.ClinicId,
            cmd.UserId,
            category,
            cmd.Key,
            previousValue,
            newValue,
            PreferenceSource.User);

        if (!auditResult.IsSuccess)
            return Result.Invalid(auditResult.ValidationErrors);

        _context.ConsentAuditEntries.Add(auditResult.Value);
        await _context.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new PreferenceChangedIntegrationEvent
        {
            ClinicId = cmd.ClinicId,
            UserId = cmd.UserId,
            Key = cmd.Key,
            Category = category,
            PreviousValue = previousValue,
            NewValue = newValue,
            Source = PreferenceSource.User
        }, ct);

        return Result.Success();
    }
}
