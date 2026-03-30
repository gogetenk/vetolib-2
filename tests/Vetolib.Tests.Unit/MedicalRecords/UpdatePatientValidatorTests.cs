using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.UpdatePatient;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class UpdatePatientValidatorTests
{
    private readonly UpdatePatientValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), "Buddy", null, "Golden Retriever", null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = new UpdatePatientCommand(Guid.Empty, null, null, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "PatientId is required");
    }

    [Fact]
    public void Empty_name_when_provided_fails()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), "", null, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name cannot be empty");
    }

    [Fact]
    public void Null_name_passes()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), null, null, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_breed_when_provided_fails()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), null, null, "", null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Breed cannot be empty");
    }

    [Fact]
    public void Empty_phone_when_provided_fails()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), null, null, null, null, null, "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Phone number cannot be empty");
    }

    [Fact]
    public void Invalid_microchip_number_fails()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), null, null, null, null, null, null, null, "12345");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Microchip number must be 15 digits (ISO 11784/11785)");
    }

    [Fact]
    public void Valid_microchip_number_passes()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), null, null, null, null, null, null, null, "123456789012345");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Null_microchip_number_passes()
    {
        var cmd = new UpdatePatientCommand(Guid.NewGuid(), null, null, null, null, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
