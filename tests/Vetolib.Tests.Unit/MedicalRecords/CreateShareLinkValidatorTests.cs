using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.MedicalRecords.Application.Commands.CreateShareLink;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class CreateShareLinkValidatorTests
{
    private readonly CreateShareLinkValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var cmd = new CreateShareLinkCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyClinicId_FailsValidation()
    {
        var cmd = new CreateShareLinkCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.ClinicId);
    }

    [Fact]
    public void Validate_WithEmptyPatientId_FailsValidation()
    {
        var cmd = new CreateShareLinkCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void Validate_WithEmptyOwnerAccountId_FailsValidation()
    {
        var cmd = new CreateShareLinkCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.OwnerAccountId);
    }
}
