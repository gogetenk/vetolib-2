using FluentAssertions;
using FluentValidation;
using Vetolib.MedicalRecords.Application.Commands.CreatePatient;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class CreatePatientValidatorTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private readonly CreatePatientValidator _validator = new();

    private CreatePatientCommand BuildValidCommand(
        string name = "Bella",
        string breed = "Labrador",
        string ownerName = "Faisal Al-Kuwari",
        string ownerPhone = "+971501234567",
        DateOnly? birthDate = null)
        => new(
            ClinicId: ClinicId,
            Name: name,
            Species: Species.Dog,
            Breed: breed,
            BirthDate: birthDate ?? new DateOnly(2020, 5, 10),
            OwnerName: ownerName,
            OwnerPhone: ownerPhone);

    [Fact]
    public void Validate_WhenCommandIsValid_ReturnsNoErrors()
    {
        var cmd = BuildValidCommand();

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_FailsWithExpectedMessage()
    {
        var cmd = BuildValidCommand(name: "");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreatePatientCommand.Name) &&
            e.ErrorMessage == "Animal name is required");
    }

    [Fact]
    public void Validate_WhenBreedIsEmpty_FailsWithExpectedMessage()
    {
        var cmd = BuildValidCommand(breed: "");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreatePatientCommand.Breed) &&
            e.ErrorMessage == "Breed is required");
    }

    [Fact]
    public void Validate_WhenOwnerNameIsEmpty_FailsWithExpectedMessage()
    {
        var cmd = BuildValidCommand(ownerName: "");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreatePatientCommand.OwnerName) &&
            e.ErrorMessage == "Owner name is required");
    }

    [Fact]
    public void Validate_WhenOwnerPhoneIsEmpty_FailsWithExpectedMessage()
    {
        var cmd = BuildValidCommand(ownerPhone: "");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreatePatientCommand.OwnerPhone) &&
            e.ErrorMessage == "Owner phone number is required");
    }

    [Fact]
    public void Validate_WhenBirthDateIsDefault_FailsWithExpectedMessage()
    {
        // Must construct directly — the helper's ?? operator replaces null with a valid date
        var cmd = new CreatePatientCommand(
            ClinicId: ClinicId,
            Name: "Bella",
            Species: Species.Dog,
            Breed: "Labrador",
            BirthDate: default,  // DateOnly.MinValue == default
            OwnerName: "Faisal Al-Kuwari",
            OwnerPhone: "+971501234567");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreatePatientCommand.BirthDate) &&
            e.ErrorMessage == "Birth date is required");
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreEmpty_ReturnsMultipleErrors()
    {
        var cmd = new CreatePatientCommand(
            ClinicId: ClinicId,
            Name: "",
            Species: Species.Dog,
            Breed: "",
            BirthDate: default,
            OwnerName: "",
            OwnerPhone: "");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePatientCommand.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePatientCommand.Breed));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePatientCommand.OwnerName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePatientCommand.OwnerPhone));
    }
}
