using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Application.Rules;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.AI.Application.Commands.GenerateHealthAlerts;

internal class GenerateHealthAlertsHandler : IRequestHandler<GenerateHealthAlertsCommand, Result<int>>
{
    private readonly IPatientAlertDataReader _patientDataReader;
    private readonly AIDbContext _dbContext;
    private readonly IEnumerable<IHealthAlertRule> _rules;
    private readonly IClinicContext _clinicContext;
    private readonly ILogger<GenerateHealthAlertsHandler> _logger;

    public GenerateHealthAlertsHandler(
        IPatientAlertDataReader patientDataReader,
        AIDbContext dbContext,
        IEnumerable<IHealthAlertRule> rules,
        IClinicContext clinicContext,
        ILogger<GenerateHealthAlertsHandler> logger)
    {
        _patientDataReader = patientDataReader;
        _dbContext = dbContext;
        _rules = rules;
        _clinicContext = clinicContext;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(GenerateHealthAlertsCommand request, CancellationToken cancellationToken)
    {
        // 1. Get all active patients with their medical records
        var patientsResult = await _patientDataReader.GetAllActivePatientsWithRecordsAsync(cancellationToken);
        if (!patientsResult.IsSuccess)
        {
            _logger.LogWarning("Failed to load patient data: {Errors}", string.Join(", ", patientsResult.Errors));
            return Result<int>.Error("Failed to load patient data.");
        }

        var patients = patientsResult.Value;
        _logger.LogInformation("Evaluating health alert rules for {PatientCount} patients", patients.Count);

        // 2. Load existing non-dismissed alerts to deduplicate
        var existingAlerts = await _dbContext.HealthAlerts
            .AsNoTracking()
            .Where(a => a.Status != HealthAlertStatus.Dismissed)
            .ToListAsync(cancellationToken);

        var newAlertCount = 0;

        // 3. For each patient, build context and run all rules
        foreach (var patientDto in patients)
        {
            var context = new PatientAlertContext(
                _clinicContext.ClinicId,
                patientDto.PatientId,
                patientDto.Name,
                patientDto.Species,
                patientDto.Breed,
                patientDto.BirthDate,
                patientDto.WeightKg,
                patientDto.RecentRecords,
                patientDto.WeightHistory);

            // Get existing alerts for this patient for deduplication within rules
            var patientExistingAlerts = existingAlerts
                .Where(a => a.PatientId == patientDto.PatientId)
                .ToList();

            foreach (var rule in _rules)
            {
                var newAlerts = rule.Evaluate(context, patientExistingAlerts);

                foreach (var alert in newAlerts)
                {
                    // Deduplicate: same RuleId + PatientId = skip
                    var isDuplicate = existingAlerts.Any(a =>
                        a.RuleId == alert.RuleId &&
                        a.PatientId == alert.PatientId);

                    if (isDuplicate)
                        continue;

                    _dbContext.HealthAlerts.Add(alert);
                    existingAlerts.Add(alert); // Track for subsequent dedup within this run
                    newAlertCount++;
                }
            }
        }

        if (newAlertCount > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated {NewAlertCount} new health alerts", newAlertCount);
        return Result<int>.Success(newAlertCount);
    }
}
