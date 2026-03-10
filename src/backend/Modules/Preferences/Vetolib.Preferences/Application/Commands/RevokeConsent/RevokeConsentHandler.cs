using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Commands.RevokeConsent;

internal class RevokeConsentHandler : IRequestHandler<RevokeConsentCommand, Result>
{
    private readonly PreferencesDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RevokeConsentHandler(PreferencesDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(RevokeConsentCommand cmd, CancellationToken ct)
    {
        // Find all boolean-type keys in this category, excluding AIDrugInteractions
        var keysInCategory = Enum.GetValues<PreferenceKey>()
            .Where(k => SystemDefaults.GetCategory(k) == cmd.Category)
            .Where(k => k != PreferenceKey.AIDrugInteractions)
            .Where(k => IsBooleanPreference(k))
            .ToList();

        var userPreferences = await _context.UserPreferences
            .Where(p => p.UserId == cmd.UserId && p.Category == cmd.Category)
            .ToListAsync(ct);

        var toPublish = new List<PreferenceChangedIntegrationEvent>();

        foreach (var key in keysInCategory)
        {
            var existing = userPreferences.FirstOrDefault(p => p.Key == key);
            string? previousValue = existing?.Value;
            const string newValue = "false";

            if (existing is null)
            {
                var createResult = UserPreference.Create(cmd.ClinicId, cmd.UserId, key, newValue);
                if (!createResult.IsSuccess)
                    continue;
                _context.UserPreferences.Add(createResult.Value);
            }
            else
            {
                existing.Update(newValue);
            }

            var auditResult = ConsentAuditEntry.Create(
                cmd.ClinicId,
                cmd.UserId,
                cmd.Category,
                key,
                previousValue,
                newValue,
                PreferenceSource.User);

            if (!auditResult.IsSuccess)
                continue;

            _context.ConsentAuditEntries.Add(auditResult.Value);

            toPublish.Add(new PreferenceChangedIntegrationEvent
            {
                ClinicId = cmd.ClinicId,
                UserId = cmd.UserId,
                Key = key,
                Category = cmd.Category,
                PreviousValue = previousValue,
                NewValue = newValue,
                Source = PreferenceSource.User
            });
        }

        await _context.SaveChangesAsync(ct);

        foreach (var evt in toPublish)
            await _publishEndpoint.Publish(evt, ct);

        return Result.Success();
    }

    private static bool IsBooleanPreference(PreferenceKey key)
    {
        var defaultValue = SystemDefaults.GetDefault(key);
        return defaultValue.Equals("true", StringComparison.OrdinalIgnoreCase)
            || defaultValue.Equals("false", StringComparison.OrdinalIgnoreCase);
    }
}
