using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.AI.Application.Commands.GenerateHealthAlerts;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Application.Rules;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class GenerateHealthAlertsHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Patient1Id = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Patient2Id = new("33333333-3333-3333-3333-333333333333");

    private readonly IPatientAlertDataReader _patientDataReader;
    private readonly AIDbContext _dbContext;
    private readonly IClinicContext _clinicContext;
    private readonly ILogger<GenerateHealthAlertsHandler> _logger;

    public GenerateHealthAlertsHandlerTests()
    {
        _patientDataReader = Substitute.For<IPatientAlertDataReader>();
        _clinicContext = Substitute.For<IClinicContext>();
        _clinicContext.ClinicId.Returns(ClinicId);
        _logger = NullLogger<GenerateHealthAlertsHandler>.Instance;

        var options = new DbContextOptionsBuilder<AIDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AIDbContext(options, _clinicContext, Substitute.For<IPublisher>());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private GenerateHealthAlertsHandler CreateHandler(IEnumerable<IHealthAlertRule>? rules = null)
    {
        return new GenerateHealthAlertsHandler(
            _patientDataReader,
            _dbContext,
            rules ?? [],
            _clinicContext,
            _logger);
    }

    private static PatientAlertDataDto CreatePatientDto(
        Guid? patientId = null,
        Species species = Species.Cat,
        string breed = "Persian",
        int ageYears = 10,
        decimal? weightKg = 4.5m)
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-ageYears));
        return new PatientAlertDataDto(
            patientId ?? Patient1Id,
            "TestPet",
            species,
            breed,
            birthDate,
            weightKg,
            Array.Empty<MedicalRecordSummaryDto>(),
            Array.Empty<WeightEntryDto>());
    }

    [Fact]
    public async Task Handle_NoPatientsReturned_ReturnsZeroAlerts()
    {
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(
                Array.Empty<PatientAlertDataDto>()));

        var handler = CreateHandler(new[] { new CatRenalScreeningRule() });
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PatientDataReaderFails_ReturnsError()
    {
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Error("Database unavailable"));

        var handler = CreateHandler();
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to load patient data.");
    }

    [Fact]
    public async Task Handle_RuleGeneratesAlert_PersistsAndReturnsCount()
    {
        var patients = new List<PatientAlertDataDto> { CreatePatientDto() };
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(patients));

        // Use the real CatRenalScreeningRule — a 10-year-old Persian cat triggers it
        var handler = CreateHandler(new IHealthAlertRule[] { new CatRenalScreeningRule() });
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);

        var savedAlerts = await _dbContext.HealthAlerts.IgnoreQueryFilters().ToListAsync();
        savedAlerts.Should().HaveCount(1);
        savedAlerts[0].RuleId.Should().Be("CAT_RENAL_SCREENING");
        savedAlerts[0].PatientId.Should().Be(Patient1Id);
    }

    [Fact]
    public async Task Handle_DuplicateRuleIdAndPatientId_SkipsAlert()
    {
        // Pre-seed an existing alert for the same rule and patient
        var existingAlert = HealthAlert.Create(
            ClinicId, Patient1Id, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium, "Existing", "Existing alert", null,
            "CAT_RENAL_SCREENING", 50);
        _dbContext.HealthAlerts.Add(existingAlert.Value);
        await _dbContext.SaveChangesAsync();

        var patients = new List<PatientAlertDataDto> { CreatePatientDto() };
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(patients));

        var handler = CreateHandler(new IHealthAlertRule[] { new CatRenalScreeningRule() });
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0); // No new alerts — deduplicated

        var allAlerts = await _dbContext.HealthAlerts.IgnoreQueryFilters().ToListAsync();
        allAlerts.Should().HaveCount(1); // Only the pre-existing one
    }

    [Fact]
    public async Task Handle_DismissedAlertSameRule_GeneratesNewAlert()
    {
        // Pre-seed a DISMISSED alert for the same rule and patient
        var existingAlert = HealthAlert.Create(
            ClinicId, Patient1Id, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium, "Old", "Old alert", null,
            "CAT_RENAL_SCREENING", 50);
        existingAlert.Value.Dismiss("No longer relevant", "Dr. Test");
        _dbContext.HealthAlerts.Add(existingAlert.Value);
        await _dbContext.SaveChangesAsync();

        var patients = new List<PatientAlertDataDto> { CreatePatientDto() };
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(patients));

        var handler = CreateHandler(new IHealthAlertRule[] { new CatRenalScreeningRule() });
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1); // New alert generated since old one was dismissed
    }

    [Fact]
    public async Task Handle_MultiplePatients_GeneratesAlertsForEach()
    {
        var patients = new List<PatientAlertDataDto>
        {
            CreatePatientDto(Patient1Id),
            CreatePatientDto(Patient2Id)
        };
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(patients));

        var handler = CreateHandler(new IHealthAlertRule[] { new CatRenalScreeningRule() });
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);

        var savedAlerts = await _dbContext.HealthAlerts.IgnoreQueryFilters().ToListAsync();
        savedAlerts.Should().HaveCount(2);
        savedAlerts.Select(a => a.PatientId).Should().Contain(new[] { Patient1Id, Patient2Id });
    }

    [Fact]
    public async Task Handle_NoRulesRegistered_ReturnsZero()
    {
        var patients = new List<PatientAlertDataDto> { CreatePatientDto() };
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(patients));

        var handler = CreateHandler(Enumerable.Empty<IHealthAlertRule>());
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultipleRulesSamePatient_DeduplicatesWithinRun()
    {
        var stubRule1 = new StubHealthAlertRule("STUB_RULE", "Stub1");
        var stubRule2 = new StubHealthAlertRule("STUB_RULE", "Stub2");

        var patients = new List<PatientAlertDataDto> { CreatePatientDto() };
        _patientDataReader.GetAllActivePatientsWithRecordsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientAlertDataDto>>.Success(patients));

        var handler = CreateHandler(new IHealthAlertRule[] { stubRule1, stubRule2 });
        var result = await handler.Handle(new GenerateHealthAlertsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1); // Deduplicated within the run
    }

    /// <summary>
    /// Concrete stub for IHealthAlertRule since NSubstitute cannot proxy internal interfaces
    /// from strong-named assemblies.
    /// </summary>
    private class StubHealthAlertRule : IHealthAlertRule
    {
        private readonly string _title;

        public StubHealthAlertRule(string ruleId, string title)
        {
            RuleId = ruleId;
            _title = title;
        }

        public string RuleId { get; }

        public IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts)
        {
            var alert = HealthAlert.Create(
                patient.ClinicId, patient.PatientId, HealthAlertType.AgeRelatedScreening,
                HealthAlertSeverity.Low, _title, $"{_title} alert", null, RuleId, 30);
            return alert.IsSuccess ? [alert.Value] : [];
        }
    }
}
