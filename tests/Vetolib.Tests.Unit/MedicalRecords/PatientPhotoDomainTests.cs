using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class PatientPhotoDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();

    private static Patient CreateValidPatient()
        => Patient.Create(ValidClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

    private static byte[] CreateValidPhotoData(int sizeBytes = 1024)
        => new byte[sizeBytes];

    // ── SetPhoto ───────────────────────────────────────────

    [Fact]
    public void SetPhoto_WithValidJpeg_ReturnsSuccess()
    {
        var patient = CreateValidPatient();
        var data = CreateValidPhotoData();

        var result = patient.SetPhoto(data, "image/jpeg");

        result.IsSuccess.Should().BeTrue();
        patient.HasPhoto.Should().BeTrue();
        patient.PhotoBase64.Should().NotBeNullOrEmpty();
        patient.PhotoContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public void SetPhoto_WithValidPng_ReturnsSuccess()
    {
        var patient = CreateValidPatient();

        var result = patient.SetPhoto(CreateValidPhotoData(), "image/png");

        result.IsSuccess.Should().BeTrue();
        patient.PhotoContentType.Should().Be("image/png");
    }

    [Fact]
    public void SetPhoto_WithValidWebp_ReturnsSuccess()
    {
        var patient = CreateValidPatient();

        var result = patient.SetPhoto(CreateValidPhotoData(), "image/webp");

        result.IsSuccess.Should().BeTrue();
        patient.PhotoContentType.Should().Be("image/webp");
    }

    [Fact]
    public void SetPhoto_WithEmptyData_ReturnsInvalid()
    {
        var patient = CreateValidPatient();

        var result = patient.SetPhoto(Array.Empty<byte>(), "image/jpeg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "photoData");
    }

    [Fact]
    public void SetPhoto_WithNullData_ReturnsInvalid()
    {
        var patient = CreateValidPatient();

        var result = patient.SetPhoto(null!, "image/jpeg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "photoData");
    }

    [Fact]
    public void SetPhoto_ExceedingMaxSize_ReturnsInvalid()
    {
        var patient = CreateValidPatient();
        var oversizedData = new byte[5 * 1024 * 1024 + 1]; // 5 MB + 1 byte

        var result = patient.SetPhoto(oversizedData, "image/jpeg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "photoData");
    }

    [Fact]
    public void SetPhoto_AtExactMaxSize_ReturnsSuccess()
    {
        var patient = CreateValidPatient();
        var exactData = new byte[5 * 1024 * 1024]; // exactly 5 MB

        var result = patient.SetPhoto(exactData, "image/jpeg");

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    [InlineData("text/plain")]
    [InlineData("")]
    public void SetPhoto_WithDisallowedContentType_ReturnsInvalid(string contentType)
    {
        var patient = CreateValidPatient();

        var result = patient.SetPhoto(CreateValidPhotoData(), contentType);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "contentType");
    }

    [Fact]
    public void SetPhoto_WithNullContentType_ReturnsInvalid()
    {
        var patient = CreateValidPatient();

        var result = patient.SetPhoto(CreateValidPhotoData(), null!);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "contentType");
    }

    [Fact]
    public void SetPhoto_ReplacesExistingPhoto()
    {
        var patient = CreateValidPatient();
        patient.SetPhoto(CreateValidPhotoData(100), "image/jpeg");
        var originalBase64 = patient.PhotoBase64;

        var newData = CreateValidPhotoData(200);
        var result = patient.SetPhoto(newData, "image/png");

        result.IsSuccess.Should().BeTrue();
        patient.PhotoBase64.Should().NotBe(originalBase64);
        patient.PhotoContentType.Should().Be("image/png");
    }

    // ── DeletePhoto ───────────────────────────────────────

    [Fact]
    public void DeletePhoto_WithExistingPhoto_ReturnsSuccess()
    {
        var patient = CreateValidPatient();
        patient.SetPhoto(CreateValidPhotoData(), "image/jpeg");

        var result = patient.DeletePhoto();

        result.IsSuccess.Should().BeTrue();
        patient.HasPhoto.Should().BeFalse();
        patient.PhotoBase64.Should().BeNull();
        patient.PhotoContentType.Should().BeNull();
    }

    [Fact]
    public void DeletePhoto_WithNoPhoto_ReturnsNotFound()
    {
        var patient = CreateValidPatient();

        var result = patient.DeletePhoto();

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    // ── GetPhoto ──────────────────────────────────────────

    [Fact]
    public void GetPhoto_WithExistingPhoto_ReturnsData()
    {
        var patient = CreateValidPatient();
        var originalData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG magic bytes
        patient.SetPhoto(originalData, "image/jpeg");

        var result = patient.GetPhoto();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().BeEquivalentTo(originalData);
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public void GetPhoto_WithNoPhoto_ReturnsNotFound()
    {
        var patient = CreateValidPatient();

        var result = patient.GetPhoto();

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    // ── HasPhoto ──────────────────────────────────────────

    [Fact]
    public void HasPhoto_WithNoPhoto_ReturnsFalse()
    {
        var patient = CreateValidPatient();

        patient.HasPhoto.Should().BeFalse();
    }

    [Fact]
    public void HasPhoto_AfterSetPhoto_ReturnsTrue()
    {
        var patient = CreateValidPatient();
        patient.SetPhoto(CreateValidPhotoData(), "image/jpeg");

        patient.HasPhoto.Should().BeTrue();
    }

    [Fact]
    public void HasPhoto_AfterDeletePhoto_ReturnsFalse()
    {
        var patient = CreateValidPatient();
        patient.SetPhoto(CreateValidPhotoData(), "image/jpeg");
        patient.DeletePhoto();

        patient.HasPhoto.Should().BeFalse();
    }

    // ── ToDto includes HasPhoto ───────────────────────────

    [Fact]
    public void ToDto_WithNoPhoto_HasPhotoIsFalse()
    {
        var patient = CreateValidPatient();

        var dto = patient.ToDto();

        dto.HasPhoto.Should().BeFalse();
    }

    [Fact]
    public void ToDto_WithPhoto_HasPhotoIsTrue()
    {
        var patient = CreateValidPatient();
        patient.SetPhoto(CreateValidPhotoData(), "image/jpeg");

        var dto = patient.ToDto();

        dto.HasPhoto.Should().BeTrue();
    }
}
