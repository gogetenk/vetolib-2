using FluentAssertions;
using Vetolib.Breeding.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class LitterDomainTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid MotherId = Guid.NewGuid();
    private static readonly Guid FatherId = Guid.NewGuid();
    private static readonly DateOnly ValidDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var result = Litter.Create(ClinicId, MotherId, FatherId, null, ValidDate, 3, 3, "Healthy litter");

        result.IsSuccess.Should().BeTrue();
        result.Value.MotherPatientId.Should().Be(MotherId);
        result.Value.FatherPatientId.Should().Be(FatherId);
        result.Value.BornCount.Should().Be(3);
        result.Value.AliveCount.Should().Be(3);
        result.Value.Notes.Should().Be("Healthy litter");
    }

    [Fact]
    public void Create_WithExternalFather_ReturnsSuccess()
    {
        var result = Litter.Create(ClinicId, MotherId, null, "Desert Wind", ValidDate, 1, 1, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExternalFatherName.Should().Be("Desert Wind");
        result.Value.FatherPatientId.Should().BeNull();
    }

    [Fact]
    public void Create_AliveCountExceedsBornCount_ReturnsInvalid()
    {
        var result = Litter.Create(ClinicId, MotherId, null, null, ValidDate, 3, 5, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Alive count cannot exceed born count"));
    }

    [Fact]
    public void Create_FutureBirthDate_ReturnsInvalid()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var result = Litter.Create(ClinicId, MotherId, null, null, futureDate, 1, 1, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("future"));
    }

    [Fact]
    public void Create_BothFatherFieldsSet_ReturnsInvalid()
    {
        var result = Litter.Create(ClinicId, MotherId, FatherId, "External Name", ValidDate, 1, 1, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Cannot specify both"));
    }

    [Fact]
    public void Create_EmptyClinicId_ReturnsInvalid()
    {
        var result = Litter.Create(Guid.Empty, MotherId, null, null, ValidDate, 1, 1, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_EmptyMotherId_ReturnsInvalid()
    {
        var result = Litter.Create(ClinicId, Guid.Empty, null, null, ValidDate, 1, 1, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "motherPatientId");
    }

    [Fact]
    public void Create_NegativeBornCount_ReturnsInvalid()
    {
        var result = Litter.Create(ClinicId, MotherId, null, null, ValidDate, -1, 0, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "bornCount");
    }

    [Fact]
    public void AddOffspring_ValidPatient_ReturnsSuccess()
    {
        var litter = Litter.Create(ClinicId, MotherId, null, null, ValidDate, 1, 1, null).Value;
        var offspringId = Guid.NewGuid();

        var result = litter.AddOffspring(offspringId, 1);

        result.IsSuccess.Should().BeTrue();
        litter.Offspring.Should().HaveCount(1);
        litter.Offspring[0].PatientId.Should().Be(offspringId);
        litter.Offspring[0].BirthOrder.Should().Be(1);
    }

    [Fact]
    public void AddOffspring_DuplicatePatient_ReturnsError()
    {
        var litter = Litter.Create(ClinicId, MotherId, null, null, ValidDate, 2, 2, null).Value;
        var offspringId = Guid.NewGuid();

        litter.AddOffspring(offspringId, 1);
        var result = litter.AddOffspring(offspringId, 2);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already registered"));
    }

    [Fact]
    public void AddOffspring_EmptyPatientId_ReturnsError()
    {
        var litter = Litter.Create(ClinicId, MotherId, null, null, ValidDate, 1, 1, null).Value;

        var result = litter.AddOffspring(Guid.Empty, null);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var litter = Litter.Create(ClinicId, MotherId, FatherId, null, ValidDate, 2, 2, "Notes").Value;
        var offspringId = Guid.NewGuid();
        litter.AddOffspring(offspringId, 1);

        var dto = litter.ToDto();

        dto.MotherPatientId.Should().Be(MotherId);
        dto.FatherPatientId.Should().Be(FatherId);
        dto.BirthDate.Should().Be(ValidDate);
        dto.BornCount.Should().Be(2);
        dto.AliveCount.Should().Be(2);
        dto.Notes.Should().Be("Notes");
        dto.Offspring.Should().HaveCount(1);
        dto.Offspring[0].PatientId.Should().Be(offspringId);
    }
}
