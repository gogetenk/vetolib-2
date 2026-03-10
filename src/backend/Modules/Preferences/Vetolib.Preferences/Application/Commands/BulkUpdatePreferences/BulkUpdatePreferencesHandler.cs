using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Commands.BulkUpdatePreferences;

internal class BulkUpdatePreferencesHandler : IRequestHandler<BulkUpdatePreferencesCommand, Result>
{
    private readonly PreferencesDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public BulkUpdatePreferencesHandler(PreferencesDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(BulkUpdatePreferencesCommand cmd, CancellationToken ct)
    {
        var userPreferences = await _context.UserPreferences
            .Where(p => p.UserId == cmd.UserId)
            .ToListAsync(ct);

        var errors = new List<ValidationError>();
        var toPublish = new List<PreferenceChangedIntegrationEvent>();

        foreach (var item in cmd.Preferences)
        {
            var existing = userPreferences.FirstOrDefault(p => p.Key == item.Key);
            string? previousValue = existing?.Value;

            if (existing is null)
            {
                var createResult = UserPreference.Create(cmd.ClinicId, cmd.UserId, item.Key, item.Value);
                if (!createResult.IsSuccess)
                {
                    errors.AddRange(createResult.ValidationErrors);
                    continue;
                }
                _context.UserPreferences.Add(createResult.Value);
            }
            else
            {
                var updateResult = existing.Update(item.Value);
                if (!updateResult.IsSuccess)
                {
                    errors.AddRange(updateResult.ValidationErrors);
                    continue;
                }
            }

            var category = SystemDefaults.GetCategory(item.Key);
            var auditResult = ConsentAuditEntry.Create(
                cmd.ClinicId,
                cmd.UserId,
                category,
                item.Key,
                previousValue,
                item.Value,
                PreferenceSource.User);

            if (!auditResult.IsSuccess)
            {
                errors.AddRange(auditResult.ValidationErrors);
                continue;
            }

            _context.ConsentAuditEntries.Add(auditResult.Value);

            toPublish.Add(new PreferenceChangedIntegrationEvent
            {
                ClinicId = cmd.ClinicId,
                UserId = cmd.UserId,
                Key = item.Key,
                Category = category,
                PreviousValue = previousValue,
                NewValue = item.Value,
                Source = PreferenceSource.User
            });
        }

        if (errors.Count > 0)
            return Result.Invalid(errors);

        await _context.SaveChangesAsync(ct);

        foreach (var evt in toPublish)
            await _publishEndpoint.Publish(evt, ct);

        return Result.Success();
    }
}
