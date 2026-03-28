using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class PatientDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Rocky");
        result.Value.Species.Should().Be(Species.Dog);
        result.Value.Breed.Should().Be("Labrador");
        result.Value.BirthDate.Should().Be(new DateOnly(2021, 5, 10));
        result.Value.ClinicId.Should().Be(ValidClinicId);
    }

    [Fact]
    public void Create_WithCamelSpecies_ReturnsSuccess()
    {
        var result = Patient.Create(ValidClinicId, "Layla", Species.Camel, "Dromedary", new DateOnly(2018, 3, 15));

        result.IsSuccess.Should().BeTrue();
        result.Value.Species.Should().Be(Species.Camel);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsInvalid()
    {
        var result = Patient.Create(ValidClinicId, "", Species.Dog, "Labrador", new DateOnly(2021, 5, 10));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "name");
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = Patient.Create(Guid.Empty, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithDefaultBirthDate_ReturnsInvalid()
    {
        var result = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", default);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "birthDate");
    }

    [Fact]
    public void UpdateInfo_WithNewName_UpdatesName()
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.UpdateInfo("Max", null, null, null);

        result.IsSuccess.Should().BeTrue();
        patient.Name.Should().Be("Max");
    }

    [Fact]
    public void UpdateInfo_WithNewSpecies_UpdatesSpecies()
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.UpdateInfo(null, Species.Cat, null, null);

        result.IsSuccess.Should().BeTrue();
        patient.Species.Should().Be(Species.Cat);
    }

    [Fact]
    public void SetWeight_WithPositiveWeight_ReturnsSuccess()
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.SetWeight(25.5m);

        result.IsSuccess.Should().BeTrue();
        patient.WeightKg.Should().Be(25.5m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetWeight_WithZeroOrNegativeWeight_ReturnsInvalid(decimal weight)
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.SetWeight(weight);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "weightKg");
    }

    [Fact]
    public void AddOwner_WithValidOwner_ReturnsSuccess()
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;
        var owner = Owner.Create(ValidClinicId, "Faisal", "Al-Kuwari", "faisal@test.com", "+971501234567").Value;
        var patientOwner = PatientOwner.Create(ValidClinicId, patient.Id, owner.Id);

        var result = patient.AddOwner(patientOwner);

        result.IsSuccess.Should().BeTrue();
        patient.PatientOwners.Should().HaveCount(1);
    }

    [Fact]
    public void AddOwner_WithNull_ReturnsError()
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.AddOwner(null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AddOwner_WithDuplicateOwner_ReturnsError()
    {
        var patient = Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;
        var owner = Owner.Create(ValidClinicId, "Faisal", "Al-Kuwari", "faisal@test.com", "+971501234567").Value;
        var patientOwner1 = PatientOwner.Create(ValidClinicId, patient.Id, owner.Id);
        var patientOwner2 = PatientOwner.Create(ValidClinicId, patient.Id, owner.Id);
        patient.AddOwner(patientOwner1);

        var result = patient.AddOwner(patientOwner2);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AllUaeSpeciesAreAvailable()
    {
        var expectedSpecies = new[]
        {
            Species.Dog, Species.Cat, Species.Bird, Species.Rabbit,
            Species.Horse, Species.Exotic, Species.Camel
        };

        var allSpecies = Enum.GetValues<Species>();

        foreach (var species in expectedSpecies)
        {
            allSpecies.Should().Contain(species, $"{species} should be a valid UAE species");
        }
    }

    // ── Sex field tests ───────────────────────────────────────────

    [Fact]
    public void Create_WithExplicitSex_SetsSex()
    {
        var result = Patient.Create(ValidClinicId, "Simba", Species.Cat, "Arabian Mau", new DateOnly(2022, 1, 15), Sex.Male);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sex.Should().Be(Sex.Male);
    }

    [Fact]
    public void Create_WithoutSex_DefaultsToUnknown()
    {
        var result = Patient.Create(ValidClinicId, "Shadow", Species.Cat, "Persian", new DateOnly(2021, 6, 1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Sex.Should().Be(Sex.Unknown);
    }

    [Fact]
    public void UpdateInfo_WithSexChange_UpdatesSex()
    {
        var patient = Patient.Create(ValidClinicId, "Rex", Species.Dog, "German Shepherd", new DateOnly(2020, 3, 10), Sex.Male).Value;

        var result = patient.UpdateInfo(null, null, null, null, Sex.NeuteredMale);

        result.IsSuccess.Should().BeTrue();
        patient.Sex.Should().Be(Sex.NeuteredMale);
    }

    [Fact]
    public void UpdateInfo_WithNullSex_KeepsExistingSex()
    {
        var patient = Patient.Create(ValidClinicId, "Bella", Species.Dog, "Saluki", new DateOnly(2019, 8, 20), Sex.Female).Value;

        var result = patient.UpdateInfo("Bella Updated", null, null, null, null);

        result.IsSuccess.Should().BeTrue();
        patient.Sex.Should().Be(Sex.Female);
    }

    [Fact]
    public void SexEnum_HasAllExpectedValues()
    {
        var allValues = Enum.GetValues<Sex>();
        allValues.Should().Contain(Sex.Male);
        allValues.Should().Contain(Sex.Female);
        allValues.Should().Contain(Sex.NeuteredMale);
        allValues.Should().Contain(Sex.SpayedFemale);
        allValues.Should().Contain(Sex.Unknown);
    }

    [Fact]
    public void SexEnum_SerializesToString()
    {
        var options = new System.Text.Json.JsonSerializerOptions();
        var json = System.Text.Json.JsonSerializer.Serialize(Sex.NeuteredMale, options);
        json.Should().Be("\"NeuteredMale\"");
    }
}
