using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class UploadPatientPhotoValidatorTests
{
    private readonly UploadPatientPhotoValidator _validator = new();

    // Valid JPEG: FF D8 FF E0 + padding
    private static byte[] ValidJpegBytes() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    // Valid PNG: 89 50 4E 47 + padding
    private static byte[] ValidPngBytes() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A];

    // Valid WebP: RIFF + size + WEBP
    private static byte[] ValidWebPBytes() => [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];

    private static UploadPatientPhotoCommand ValidCommand() => new(
        Guid.NewGuid(),
        ValidJpegBytes(),
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
        // Create a large byte array with valid JPEG header
        var data = new byte[5 * 1024 * 1024 + 1];
        data[0] = 0xFF; data[1] = 0xD8; data[2] = 0xFF; data[3] = 0xE0;
        var cmd = ValidCommand() with { PhotoData = data };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoData");
    }

    [Fact]
    public void Photo_data_exactly_5mb_passes()
    {
        var data = new byte[5 * 1024 * 1024];
        data[0] = 0xFF; data[1] = 0xD8; data[2] = 0xFF; data[3] = 0xE0;
        var cmd = ValidCommand() with { PhotoData = data };
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

    [Fact]
    public void Jpeg_content_type_with_matching_magic_bytes_passes()
    {
        var cmd = new UploadPatientPhotoCommand(Guid.NewGuid(), ValidJpegBytes(), "image/jpeg");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Png_content_type_with_matching_magic_bytes_passes()
    {
        var cmd = new UploadPatientPhotoCommand(Guid.NewGuid(), ValidPngBytes(), "image/png");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Webp_content_type_with_matching_magic_bytes_passes()
    {
        var cmd = new UploadPatientPhotoCommand(Guid.NewGuid(), ValidWebPBytes(), "image/webp");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Declared_jpeg_but_actually_png_fails_magic_byte_check()
    {
        var cmd = new UploadPatientPhotoCommand(Guid.NewGuid(), ValidPngBytes(), "image/jpeg");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("does not match"));
    }

    [Fact]
    public void Declared_png_but_actually_jpeg_fails_magic_byte_check()
    {
        var cmd = new UploadPatientPhotoCommand(Guid.NewGuid(), ValidJpegBytes(), "image/png");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("does not match"));
    }

    [Fact]
    public void Declared_jpeg_but_exe_file_fails_magic_byte_check()
    {
        byte[] exeData = [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00];
        var cmd = new UploadPatientPhotoCommand(Guid.NewGuid(), exeData, "image/jpeg");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}
