using FluentAssertions;
using Vetolib.Messaging.Application.Services;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class FileTypeValidatorTests
{
    // --- DetectContentType ---

    [Fact]
    public void DetectContentType_Pdf_ReturnsApplicationPdf()
    {
        // %PDF magic bytes
        byte[] header = [0x25, 0x50, 0x44, 0x46];
        var result = FileTypeValidator.DetectContentType(header);
        result.Should().Be("application/pdf");
    }

    [Fact]
    public void DetectContentType_Jpg_ReturnsImageJpeg()
    {
        byte[] header = [0xFF, 0xD8, 0xFF, 0xE0];
        var result = FileTypeValidator.DetectContentType(header);
        result.Should().Be("image/jpeg");
    }

    [Fact]
    public void DetectContentType_Png_ReturnsImagePng()
    {
        byte[] header = [0x89, 0x50, 0x4E, 0x47];
        var result = FileTypeValidator.DetectContentType(header);
        result.Should().Be("image/png");
    }

    [Fact]
    public void DetectContentType_UnknownBytes_ReturnsNull()
    {
        byte[] header = [0x00, 0x00, 0x00, 0x00];
        var result = FileTypeValidator.DetectContentType(header);
        result.Should().BeNull();
    }

    [Fact]
    public void DetectContentType_TooShort_ReturnsNull()
    {
        byte[] header = [0xFF, 0xD8];
        var result = FileTypeValidator.DetectContentType(header);
        result.Should().BeNull();
    }

    [Fact]
    public void DetectContentType_GifBytes_ReturnsNull()
    {
        // GIF89a — not allowed
        byte[] header = [0x47, 0x49, 0x46, 0x38];
        var result = FileTypeValidator.DetectContentType(header);
        result.Should().BeNull();
    }

    // --- IsValidFileSize ---

    [Fact]
    public void IsValidFileSize_WithinLimit_ReturnsTrue()
    {
        FileTypeValidator.IsValidFileSize(5 * 1024 * 1024).Should().BeTrue();
    }

    [Fact]
    public void IsValidFileSize_ExactlyAtLimit_ReturnsTrue()
    {
        FileTypeValidator.IsValidFileSize(10 * 1024 * 1024).Should().BeTrue();
    }

    [Fact]
    public void IsValidFileSize_OverLimit_ReturnsFalse()
    {
        FileTypeValidator.IsValidFileSize(10 * 1024 * 1024 + 1).Should().BeFalse();
    }

    [Fact]
    public void IsValidFileSize_Zero_ReturnsFalse()
    {
        FileTypeValidator.IsValidFileSize(0).Should().BeFalse();
    }

    [Fact]
    public void IsValidFileSize_Negative_ReturnsFalse()
    {
        FileTypeValidator.IsValidFileSize(-1).Should().BeFalse();
    }

    // --- IsValidFileCount ---

    [Fact]
    public void IsValidFileCount_One_ReturnsTrue()
    {
        FileTypeValidator.IsValidFileCount(1).Should().BeTrue();
    }

    [Fact]
    public void IsValidFileCount_Five_ReturnsTrue()
    {
        FileTypeValidator.IsValidFileCount(5).Should().BeTrue();
    }

    [Fact]
    public void IsValidFileCount_Six_ReturnsFalse()
    {
        FileTypeValidator.IsValidFileCount(6).Should().BeFalse();
    }

    [Fact]
    public void IsValidFileCount_Zero_ReturnsFalse()
    {
        FileTypeValidator.IsValidFileCount(0).Should().BeFalse();
    }
}
