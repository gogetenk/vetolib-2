using FluentAssertions;
using Vetolib.Messaging.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class PendingUploadTests
{
    private static readonly Guid TestClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var result = PendingUpload.Create(
            TestClinicId,
            "report.pdf",
            "application/pdf",
            1024,
            "uploads/2026/03/report.pdf");

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(TestClinicId);
        result.Value.FileName.Should().Be("report.pdf");
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.FileSizeBytes.Should().Be(1024);
        result.Value.StoragePath.Should().Be("uploads/2026/03/report.pdf");
        result.Value.ExpiresAt.Should().BeAfter(result.Value.UploadedAt);
        (result.Value.ExpiresAt - result.Value.UploadedAt).Should().BeCloseTo(TimeSpan.FromHours(24), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_EmptyClinicId_ReturnsInvalid()
    {
        var result = PendingUpload.Create(
            Guid.Empty,
            "photo.jpg",
            "image/jpeg",
            1024,
            "uploads/photo.jpg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_EmptyFileName_ReturnsInvalid()
    {
        var result = PendingUpload.Create(
            TestClinicId,
            "",
            "image/jpeg",
            1024,
            "uploads/photo.jpg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "fileName");
    }

    [Fact]
    public void Create_ZeroFileSize_ReturnsInvalid()
    {
        var result = PendingUpload.Create(
            TestClinicId,
            "photo.jpg",
            "image/jpeg",
            0,
            "uploads/photo.jpg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "fileSizeBytes");
    }

    [Fact]
    public void Create_EmptyStoragePath_ReturnsInvalid()
    {
        var result = PendingUpload.Create(
            TestClinicId,
            "photo.jpg",
            "image/jpeg",
            1024,
            "");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "storagePath");
    }
}
