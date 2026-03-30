using Ardalis.Result;
using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class MedicalRecordDomainTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    [Fact]
    public void Create_WithFutureExaminedAt_ReturnsInvalid()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        var result = MedicalRecord.Create(
            ClinicId,
            PatientId,
            "Dermatitis",
            "Topical cream",
            "Dr. Ahmed",
            futureDate);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "examinedAt");
    }

    [Fact]
    public void Create_WithCurrentExaminedAt_Succeeds()
    {
        var now = DateTime.UtcNow;

        var result = MedicalRecord.Create(
            ClinicId,
            PatientId,
            "Dermatitis",
            "Topical cream",
            "Dr. Ahmed",
            now);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExaminedAt.Should().Be(now);
    }

    [Fact]
    public void Create_WithPastExaminedAt_Succeeds()
    {
        var pastDate = DateTime.UtcNow.AddDays(-30);

        var result = MedicalRecord.Create(
            ClinicId,
            PatientId,
            "Annual checkup",
            "No issues found",
            "Dr. Fatima",
            pastDate);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExaminedAt.Should().Be(pastDate);
    }
}
