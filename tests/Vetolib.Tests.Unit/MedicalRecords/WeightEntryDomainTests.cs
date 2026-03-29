using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class WeightEntryDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();
    private static readonly Guid ValidPatientId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 25.5m, "Dr. Ahmed");

        result.IsSuccess.Should().BeTrue();
        result.Value.WeightKg.Should().Be(25.5m);
        result.Value.RecordedBy.Should().Be("Dr. Ahmed");
        result.Value.ClinicId.Should().Be(ValidClinicId);
        result.Value.PatientId.Should().Be(ValidPatientId);
    }

    [Fact]
    public void Create_WithNote_ReturnsSuccessWithNote()
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 30.0m, "Dr. Ahmed", "Post-surgery check");

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().Be("Post-surgery check");
    }

    [Fact]
    public void Create_WithNullNote_SetsNoteToNull()
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 30.0m, "Dr. Ahmed");

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().BeNull();
    }

    [Fact]
    public void Create_WithCustomDate_UsesProvidedDate()
    {
        var date = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 30.0m, "Dr. Ahmed", recordedAt: date);

        result.IsSuccess.Should().BeTrue();
        result.Value.RecordedAt.Should().Be(date);
    }

    [Fact]
    public void Create_WithoutDate_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 30.0m, "Dr. Ahmed");
        var after = DateTime.UtcNow;

        result.IsSuccess.Should().BeTrue();
        result.Value.RecordedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeWeight_ReturnsInvalid(decimal weight)
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, weight, "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "weightKg");
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Weight must be greater than zero");
    }

    [Fact]
    public void Create_WithWeightExceedingMaximum_ReturnsInvalid()
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 50000m, "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "Weight exceeds maximum allowed value");
    }

    [Fact]
    public void Create_WithMaxWeight_ReturnsSuccess()
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 10000m, "Dr. Ahmed");

        result.IsSuccess.Should().BeTrue();
        result.Value.WeightKg.Should().Be(10000m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithMissingRecordedBy_ReturnsInvalid(string? recordedBy)
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 25.5m, recordedBy!);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "recordedBy");
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = WeightEntry.Create(Guid.Empty, ValidPatientId, 25.5m, "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyPatientId_ReturnsInvalid()
    {
        var result = WeightEntry.Create(ValidClinicId, Guid.Empty, 25.5m, "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "patientId");
    }

    [Fact]
    public void Create_WithLongNote_TruncatesTo500Chars()
    {
        var longNote = new string('A', 600);
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 25.5m, "Dr. Ahmed", longNote);

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().HaveLength(500);
    }

    [Fact]
    public void Create_WithWhitespaceOnlyNote_SetsNoteToNull()
    {
        var result = WeightEntry.Create(ValidClinicId, ValidPatientId, 25.5m, "Dr. Ahmed", "   ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().BeNull();
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var date = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var entry = WeightEntry.Create(ValidClinicId, ValidPatientId, 25.5m, "Dr. Ahmed", "Check-up", date).Value;

        var dto = entry.ToDto();

        dto.Id.Should().Be(entry.Id);
        dto.PatientId.Should().Be(ValidPatientId);
        dto.WeightKg.Should().Be(25.5m);
        dto.RecordedAt.Should().Be(date);
        dto.RecordedBy.Should().Be("Dr. Ahmed");
        dto.Note.Should().Be("Check-up");
    }

    [Fact]
    public void ToCurvePointDto_MapsDateAndWeight()
    {
        var date = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var entry = WeightEntry.Create(ValidClinicId, ValidPatientId, 25.5m, "Dr. Ahmed", recordedAt: date).Value;

        var point = entry.ToCurvePointDto();

        point.RecordedAt.Should().Be(date);
        point.WeightKg.Should().Be(25.5m);
    }

    // ── Patient.SetWeight tests ─────────────────────────────────────

    [Fact]
    public void Patient_SetWeight_UpdatesWeightKg()
    {
        var patient = Patient.Create(ValidClinicId, "Layla", Species.Horse, "Arabian", new DateOnly(2020, 1, 1)).Value;

        var result = patient.SetWeight(450.5m);

        result.IsSuccess.Should().BeTrue();
        patient.WeightKg.Should().Be(450.5m);
    }

    [Fact]
    public void Patient_SetWeight_WithNewWeight_OverridesPrevious()
    {
        var patient = Patient.Create(ValidClinicId, "Layla", Species.Horse, "Arabian", new DateOnly(2020, 1, 1)).Value;
        patient.SetWeight(420.0m);

        var result = patient.SetWeight(450.5m);

        result.IsSuccess.Should().BeTrue();
        patient.WeightKg.Should().Be(450.5m);
    }
}
