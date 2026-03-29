using FluentAssertions;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Breeding.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class PatientLineageDomainTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid MotherId = Guid.NewGuid();
    private static readonly Guid FatherId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var result = PatientLineage.Create(ClinicId, PatientId, MotherId, FatherId, "LOF-123", RegistryType.LOF);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.MotherPatientId.Should().Be(MotherId);
        result.Value.FatherPatientId.Should().Be(FatherId);
        result.Value.RegistryNumber.Should().Be("LOF-123");
        result.Value.RegistryType.Should().Be(RegistryType.LOF);
    }

    [Fact]
    public void Create_WithoutParents_ReturnsSuccess()
    {
        var result = PatientLineage.Create(ClinicId, PatientId, null, null, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.MotherPatientId.Should().BeNull();
        result.Value.FatherPatientId.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyClinicId_ReturnsInvalid()
    {
        var result = PatientLineage.Create(Guid.Empty, PatientId, null, null, null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_EmptyPatientId_ReturnsInvalid()
    {
        var result = PatientLineage.Create(ClinicId, Guid.Empty, null, null, null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "patientId");
    }

    [Fact]
    public void Create_SelfAsMother_ReturnsInvalid()
    {
        var result = PatientLineage.Create(ClinicId, PatientId, PatientId, null, null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("cannot be its own mother"));
    }

    [Fact]
    public void Create_SelfAsFather_ReturnsInvalid()
    {
        var result = PatientLineage.Create(ClinicId, PatientId, null, PatientId, null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("cannot be its own father"));
    }

    [Fact]
    public void SetParents_ValidData_ReturnsSuccess()
    {
        var lineage = PatientLineage.Create(ClinicId, PatientId, null, null, null, null).Value;
        var newMother = Guid.NewGuid();
        var newFather = Guid.NewGuid();

        var result = lineage.SetParents(newMother, newFather);

        result.IsSuccess.Should().BeTrue();
        lineage.MotherPatientId.Should().Be(newMother);
        lineage.FatherPatientId.Should().Be(newFather);
    }

    [Fact]
    public void SetParents_SelfAsMother_ReturnsError()
    {
        var lineage = PatientLineage.Create(ClinicId, PatientId, null, null, null, null).Value;

        var result = lineage.SetParents(PatientId, null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be its own mother"));
    }

    [Fact]
    public void SetParents_SelfAsFather_ReturnsError()
    {
        var lineage = PatientLineage.Create(ClinicId, PatientId, null, null, null, null).Value;

        var result = lineage.SetParents(null, PatientId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be its own father"));
    }

    [Fact]
    public void SetRegistry_UpdatesValues()
    {
        var lineage = PatientLineage.Create(ClinicId, PatientId, null, null, null, null).Value;

        var result = lineage.SetRegistry("LOOF-456", RegistryType.LOOF);

        result.IsSuccess.Should().BeTrue();
        lineage.RegistryNumber.Should().Be("LOOF-456");
        lineage.RegistryType.Should().Be(RegistryType.LOOF);
    }

    [Fact]
    public void ValidateParentCompatibility_MotherIsMale_ReturnsError()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Etoile", Species.Horse, Sex.Female);
        var mother = new PatientBasicInfoDto(MotherId, "Tonnerre", Species.Horse, Sex.Male);

        var result = PatientLineage.ValidateParentCompatibility(offspring, mother, null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Mother must be female"));
    }

    [Fact]
    public void ValidateParentCompatibility_FatherIsFemale_ReturnsError()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Etoile", Species.Horse, Sex.Female);
        var father = new PatientBasicInfoDto(FatherId, "Aurore", Species.Horse, Sex.Female);

        var result = PatientLineage.ValidateParentCompatibility(offspring, null, father);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Father must be male"));
    }

    [Fact]
    public void ValidateParentCompatibility_DifferentSpecies_ReturnsError()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Cleo", Species.Cat, Sex.Female);
        var father = new PatientBasicInfoDto(FatherId, "Sultan", Species.Horse, Sex.Male);

        var result = PatientLineage.ValidateParentCompatibility(offspring, null, father);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("same species"));
    }

    [Fact]
    public void ValidateParentCompatibility_SpayedFemaleAsMother_ReturnsSuccess()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Kitten", Species.Cat, Sex.Female);
        var mother = new PatientBasicInfoDto(MotherId, "Luna", Species.Cat, Sex.SpayedFemale);

        var result = PatientLineage.ValidateParentCompatibility(offspring, mother, null);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateParentCompatibility_NeuteredMaleAsFather_ReturnsSuccess()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Kitten", Species.Cat, Sex.Male);
        var father = new PatientBasicInfoDto(FatherId, "Milo", Species.Cat, Sex.NeuteredMale);

        var result = PatientLineage.ValidateParentCompatibility(offspring, null, father);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateParentCompatibility_ValidParents_ReturnsSuccess()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Etoile", Species.Horse, Sex.Female);
        var mother = new PatientBasicInfoDto(MotherId, "Aurore", Species.Horse, Sex.Female);
        var father = new PatientBasicInfoDto(FatherId, "Tonnerre", Species.Horse, Sex.Male);

        var result = PatientLineage.ValidateParentCompatibility(offspring, mother, father);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateParentCompatibility_NullParents_ReturnsSuccess()
    {
        var offspring = new PatientBasicInfoDto(PatientId, "Etoile", Species.Horse, Sex.Female);

        var result = PatientLineage.ValidateParentCompatibility(offspring, null, null);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_TrimsRegistryNumber()
    {
        var result = PatientLineage.Create(ClinicId, PatientId, null, null, "  LOF-123  ", RegistryType.LOF);

        result.IsSuccess.Should().BeTrue();
        result.Value.RegistryNumber.Should().Be("LOF-123");
    }
}
