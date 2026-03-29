using Ardalis.Result;
using FluentAssertions;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class PregnancyDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid FatherId = new("33333333-3333-3333-3333-333333333333");
    private static readonly DateOnly MatingDate = new(2026, 2, 15);

    private static Result<Pregnancy> CreatePregnancy(
        string species = "Dog",
        MatingMethod method = MatingMethod.Natural)
        => Pregnancy.Create(ClinicId, PatientId, FatherId, MatingDate, method, species, "Test notes");

    // ── Create ────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = CreatePregnancy();

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.FatherPatientId.Should().Be(FatherId);
        result.Value.MatingDate.Should().Be(MatingDate);
        result.Value.MatingMethod.Should().Be(MatingMethod.Natural);
        result.Value.Status.Should().Be(PregnancyStatus.Active);
        result.Value.Notes.Should().Be("Test notes");
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = Pregnancy.Create(Guid.Empty, PatientId, FatherId, MatingDate, MatingMethod.Natural, "Dog", null);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Create_WithEmptyPatientId_ReturnsInvalid()
    {
        var result = Pregnancy.Create(ClinicId, Guid.Empty, FatherId, MatingDate, MatingMethod.Natural, "Dog", null);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Create_WithNullFatherId_IsAllowed()
    {
        var result = Pregnancy.Create(ClinicId, PatientId, null, MatingDate, MatingMethod.Natural, "Dog", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.FatherPatientId.Should().BeNull();
    }

    // ── Gestation periods ─────────────────────────────────────────────

    [Theory]
    [InlineData("Dog", 63)]
    [InlineData("Cat", 65)]
    [InlineData("Horse", 340)]
    [InlineData("Camel", 390)]
    [InlineData("Falcon", 32)]
    [InlineData("Rabbit", 31)]
    [InlineData("Lizard", 60)] // unknown species defaults to 60
    public void Create_CalculatesExpectedDueDate_BasedOnSpecies(string species, int expectedDays)
    {
        var result = CreatePregnancy(species: species);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpectedDueDate.Should().Be(MatingDate.AddDays(expectedDays));
    }

    // ── Mating methods ────────────────────────────────────────────────

    [Theory]
    [InlineData(MatingMethod.Natural)]
    [InlineData(MatingMethod.ArtificialInsemination)]
    [InlineData(MatingMethod.EmbryoTransfer)]
    public void Create_WithDifferentMatingMethods_SetsCorrectMethod(MatingMethod method)
    {
        var result = CreatePregnancy(method: method);

        result.IsSuccess.Should().BeTrue();
        result.Value.MatingMethod.Should().Be(method);
    }

    // ── RecordDelivery ────────────────────────────────────────────────

    [Fact]
    public void RecordDelivery_WhenActive_CompletesPregnancy()
    {
        var pregnancy = CreatePregnancy().Value;
        var deliveryDate = new DateOnly(2026, 4, 20);

        var result = pregnancy.RecordDelivery(deliveryDate, PregnancyOutcome.LiveBirth, 3, "Healthy litter");

        result.IsSuccess.Should().BeTrue();
        pregnancy.Status.Should().Be(PregnancyStatus.Completed);
        pregnancy.ActualDeliveryDate.Should().Be(deliveryDate);
        pregnancy.Outcome.Should().Be(PregnancyOutcome.LiveBirth);
        pregnancy.OffspringCount.Should().Be(3);
    }

    [Fact]
    public void RecordDelivery_WithStillbirth_CompletesPregnancy()
    {
        var pregnancy = CreatePregnancy().Value;
        var deliveryDate = new DateOnly(2026, 4, 20);

        var result = pregnancy.RecordDelivery(deliveryDate, PregnancyOutcome.Stillbirth, 0, null);

        result.IsSuccess.Should().BeTrue();
        pregnancy.Status.Should().Be(PregnancyStatus.Completed);
        pregnancy.Outcome.Should().Be(PregnancyOutcome.Stillbirth);
        pregnancy.OffspringCount.Should().Be(0);
    }

    [Fact]
    public void RecordDelivery_WhenNotActive_ReturnsError()
    {
        var pregnancy = CreatePregnancy().Value;
        pregnancy.RecordDelivery(new DateOnly(2026, 4, 20), PregnancyOutcome.LiveBirth, 1, null);

        var result = pregnancy.RecordDelivery(new DateOnly(2026, 5, 1), PregnancyOutcome.LiveBirth, 1, null);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void RecordDelivery_WithNegativeOffspringCount_ReturnsInvalid()
    {
        var pregnancy = CreatePregnancy().Value;

        var result = pregnancy.RecordDelivery(new DateOnly(2026, 4, 20), PregnancyOutcome.LiveBirth, -1, null);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    // ── RecordLoss ────────────────────────────────────────────────────

    [Fact]
    public void RecordLoss_WhenActive_SetsStatusToLost()
    {
        var pregnancy = CreatePregnancy().Value;
        var lossDate = new DateOnly(2026, 3, 10);

        var result = pregnancy.RecordLoss(lossDate, PregnancyOutcome.Miscarriage, "Early miscarriage");

        result.IsSuccess.Should().BeTrue();
        pregnancy.Status.Should().Be(PregnancyStatus.Lost);
        pregnancy.ActualDeliveryDate.Should().Be(lossDate);
        pregnancy.Outcome.Should().Be(PregnancyOutcome.Miscarriage);
        pregnancy.OffspringCount.Should().Be(0);
    }

    [Fact]
    public void RecordLoss_WhenNotActive_ReturnsError()
    {
        var pregnancy = CreatePregnancy().Value;
        pregnancy.RecordLoss(new DateOnly(2026, 3, 10), PregnancyOutcome.Miscarriage, null);

        var result = pregnancy.RecordLoss(new DateOnly(2026, 4, 1), PregnancyOutcome.Miscarriage, null);

        result.IsSuccess.Should().BeFalse();
    }

    // ── ScheduleCheck ─────────────────────────────────────────────────

    [Fact]
    public void ScheduleCheck_WhenActive_AddsCheck()
    {
        var pregnancy = CreatePregnancy().Value;

        var result = pregnancy.ScheduleCheck(
            new DateOnly(2026, 3, 15), PregnancyCheckType.Ultrasound, "28-day scan");

        result.IsSuccess.Should().BeTrue();
        pregnancy.ScheduledChecks.Should().HaveCount(1);
        result.Value.CheckType.Should().Be(PregnancyCheckType.Ultrasound);
        result.Value.Note.Should().Be("28-day scan");
    }

    [Fact]
    public void ScheduleCheck_WhenNotActive_ReturnsError()
    {
        var pregnancy = CreatePregnancy().Value;
        pregnancy.RecordDelivery(new DateOnly(2026, 4, 20), PregnancyOutcome.LiveBirth, 1, null);

        var result = pregnancy.ScheduleCheck(
            new DateOnly(2026, 5, 1), PregnancyCheckType.BloodTest, null);

        result.IsSuccess.Should().BeFalse();
    }

    // ── ToDto ─────────────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var pregnancy = CreatePregnancy().Value;
        pregnancy.ScheduleCheck(new DateOnly(2026, 3, 15), PregnancyCheckType.Ultrasound, null);

        var dto = pregnancy.ToDto();

        dto.Id.Should().Be(pregnancy.Id);
        dto.PatientId.Should().Be(PatientId);
        dto.MatingMethod.Should().Be(MatingMethod.Natural);
        dto.Status.Should().Be(PregnancyStatus.Active);
        dto.ScheduledChecks.Should().HaveCount(1);
    }
}
