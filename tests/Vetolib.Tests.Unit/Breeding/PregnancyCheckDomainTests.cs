using Ardalis.Result;
using FluentAssertions;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class PregnancyCheckDomainTests
{
    private static readonly Guid PregnancyId = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = PregnancyCheck.Create(
            PregnancyId, new DateOnly(2026, 3, 15), PregnancyCheckType.Ultrasound, "Scan note");

        result.IsSuccess.Should().BeTrue();
        result.Value.PregnancyId.Should().Be(PregnancyId);
        result.Value.CheckType.Should().Be(PregnancyCheckType.Ultrasound);
        result.Value.Note.Should().Be("Scan note");
        result.Value.CompletedAt.Should().BeNull();
        result.Value.Result.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyPregnancyId_ReturnsInvalid()
    {
        var result = PregnancyCheck.Create(
            Guid.Empty, new DateOnly(2026, 3, 15), PregnancyCheckType.BloodTest, null);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Complete_WhenNotCompleted_SetsCompletedAtAndResult()
    {
        var check = PregnancyCheck.Create(
            PregnancyId, new DateOnly(2026, 3, 15), PregnancyCheckType.Ultrasound, null).Value;

        var result = check.Complete("Pregnancy confirmed, single foal");

        result.IsSuccess.Should().BeTrue();
        check.CompletedAt.Should().NotBeNull();
        check.Result.Should().Be("Pregnancy confirmed, single foal");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ReturnsError()
    {
        var check = PregnancyCheck.Create(
            PregnancyId, new DateOnly(2026, 3, 15), PregnancyCheckType.Ultrasound, null).Value;
        check.Complete("First result");

        var result = check.Complete("Second result");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var check = PregnancyCheck.Create(
            PregnancyId, new DateOnly(2026, 3, 15), PregnancyCheckType.PhysicalExam, "Note").Value;

        var dto = check.ToDto();

        dto.PregnancyId.Should().Be(PregnancyId);
        dto.ScheduledDate.Should().Be(new DateOnly(2026, 3, 15));
        dto.CheckType.Should().Be(PregnancyCheckType.PhysicalExam);
        dto.Note.Should().Be("Note");
    }
}
