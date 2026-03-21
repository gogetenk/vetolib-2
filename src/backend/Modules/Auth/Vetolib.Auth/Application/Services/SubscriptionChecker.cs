using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Services;

internal class SubscriptionChecker : ISubscriptionChecker
{
    private readonly AuthDbContext _dbContext;
    private readonly ISender _sender;
    private readonly ILogger<SubscriptionChecker> _logger;

    public SubscriptionChecker(AuthDbContext dbContext, ISender sender, ILogger<SubscriptionChecker> logger)
    {
        _dbContext = dbContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result> CheckLimitAsync(Guid clinicId, LimitType limitType, CancellationToken ct = default)
    {
        var clinic = await _dbContext.Clinics
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct);

        if (clinic is null)
            return Result.NotFound($"Clinic {clinicId} not found.");

        var limits = PlanDefinitions.GetLimits(clinic.SubscriptionPlan);

        return limitType switch
        {
            LimitType.Vets => await CheckCountLimit(
                clinicId, limits.MaxVets,
                async () => await _dbContext.Users.CountAsync(u => u.ClinicId == clinicId && u.Role == UserRole.Vet && u.IsActive, ct),
                "veterinarians", clinic.SubscriptionPlan, ct),

            LimitType.Patients => await CheckCountLimit(
                clinicId, limits.MaxPatients,
                async () => await CountPatientsAsync(clinicId, ct),
                "patients", clinic.SubscriptionPlan, ct),

            LimitType.WhatsAppMessages => await CheckCountLimit(
                clinicId, limits.MaxWhatsAppMessagesPerMonth,
                async () => await CountWhatsAppMessagesThisMonthAsync(clinicId, ct),
                "WhatsApp messages this month", clinic.SubscriptionPlan, ct),

            LimitType.StorageGB => await CheckCountLimit(
                clinicId, limits.MaxStorageGB,
                async () => await GetStorageUsedGBAsync(clinicId, ct),
                "GB of storage", clinic.SubscriptionPlan, ct),

            LimitType.AiTriage => limits.HasAiTriage
                ? Result.Success()
                : Result.Error($"AI Triage is not available on {clinic.SubscriptionPlan} plan. Please upgrade to access this feature."),

            LimitType.WhatsApp => limits.HasWhatsApp
                ? Result.Success()
                : Result.Error($"WhatsApp integration is not available on {clinic.SubscriptionPlan} plan. Please upgrade to access this feature."),

            LimitType.MultiClinic => limits.HasMultiClinic
                ? Result.Success()
                : Result.Error($"Multi-clinic management is not available on {clinic.SubscriptionPlan} plan. Please upgrade to Enterprise."),

            LimitType.Api => limits.HasApi
                ? Result.Success()
                : Result.Error($"API access is not available on {clinic.SubscriptionPlan} plan. Please upgrade to Enterprise."),

            _ => Result.Error($"Unknown limit type: {limitType}")
        };
    }

    public async Task<Result<UsageDto>> GetCurrentUsageAsync(Guid clinicId, CancellationToken ct = default)
    {
        var clinic = await _dbContext.Clinics
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct);

        if (clinic is null)
            return Result<UsageDto>.NotFound($"Clinic {clinicId} not found.");

        var limits = PlanDefinitions.GetLimits(clinic.SubscriptionPlan);

        var currentVets = await _dbContext.Users
            .CountAsync(u => u.ClinicId == clinicId && u.Role == UserRole.Vet && u.IsActive, ct);

        var currentPatients = await CountPatientsAsync(clinicId, ct);
        var currentWhatsApp = await CountWhatsAppMessagesThisMonthAsync(clinicId, ct);
        var currentStorage = await GetStorageUsedGBAsync(clinicId, ct);

        var dto = new UsageDto(
            Plan: clinic.SubscriptionPlan,
            CurrentVets: currentVets,
            MaxVets: limits.MaxVets,
            CurrentPatients: currentPatients,
            MaxPatients: limits.MaxPatients,
            CurrentWhatsAppMessages: currentWhatsApp,
            MaxWhatsAppMessages: limits.MaxWhatsAppMessagesPerMonth,
            CurrentStorageGB: currentStorage,
            MaxStorageGB: limits.MaxStorageGB,
            HasAiTriage: limits.HasAiTriage,
            HasWhatsApp: limits.HasWhatsApp,
            HasMultiClinic: limits.HasMultiClinic,
            HasApi: limits.HasApi);

        return Result<UsageDto>.Success(dto);
    }

    private static async Task<Result> CheckCountLimit(
        Guid clinicId,
        int maxAllowed,
        Func<Task<int>> countFunc,
        string resourceName,
        Contracts.SubscriptionPlan plan,
        CancellationToken ct)
    {
        if (maxAllowed == int.MaxValue)
            return Result.Success();

        var current = await countFunc();
        if (current < maxAllowed)
            return Result.Success();

        return Result.Error(
            $"You have reached the maximum of {maxAllowed} {resourceName} on the {plan} plan. Please upgrade to add more.");
    }

    private async Task<int> CountPatientsAsync(Guid clinicId, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(new GetPatientCountQuery(), ct);
            return result.IsSuccess ? result.Value : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to count patients for clinic {ClinicId}, returning 0", clinicId);
            return 0;
        }
    }

    // TODO: Wire to Messaging module once GetWhatsAppMessageCountQuery is added to Vetolib.Messaging.Contracts
    private Task<int> CountWhatsAppMessagesThisMonthAsync(Guid clinicId, CancellationToken ct)
    {
        _logger.LogDebug("WhatsApp message count not yet wired for clinic {ClinicId}, returning 0", clinicId);
        return Task.FromResult(0);
    }

    // TODO: Wire to storage tracking once GetStorageUsedQuery is available
    private Task<int> GetStorageUsedGBAsync(Guid clinicId, CancellationToken ct)
    {
        _logger.LogDebug("Storage usage tracking not yet wired for clinic {ClinicId}, returning 0", clinicId);
        return Task.FromResult(0);
    }
}
