using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class UploadPatientPhotoValidatorTests
{
    private readonly UploadPatientPhotoValidator _validator = new();

    private static UploadPatientPhotoCommand ValidCommand() => new(
        Guid.NewGuid(),
        new byte[1024],
        "image/jpeg");

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = ValidCommand() with { PatientId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Null_photo_data_fails()
    {
        var cmd = ValidCommand() with { PhotoData = null! };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoData");
    }

    [Fact]
    public void Empty_photo_data_fails()
    {
        var cmd = ValidCommand() with { PhotoData = Array.Empty<byte>() };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoData");
    }

    [Fact]
    public void Photo_data_over_5mb_fails()
    {
        var cmd = ValidCommand() with { PhotoData = new byte[5 * 1024 * 1024 + 1] };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoData");
    }

    [Fact]
    public void Photo_data_exactly_5mb_passes()
    {
        var cmd = ValidCommand() with { PhotoData = new byte[5 * 1024 * 1024] };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_content_type_fails()
    {
        var cmd = ValidCommand() with { ContentType = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContentType");
    }

    [Fact]
    public void Invalid_content_type_fails()
    {
        var cmd = ValidCommand() with { ContentType = "application/pdf" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContentType");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Allowed_content_types_pass(string contentType)
    {
        var cmd = ValidCommand() with { ContentType = contentType };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
