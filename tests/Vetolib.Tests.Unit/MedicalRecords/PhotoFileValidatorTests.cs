using FluentAssertions;
using Vetolib.MedicalRecords.Application.Services;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class PhotoFileValidatorTests
{
    // --- DetectContentType ---

    [Fact]
    public void DetectContentType_Jpg_ReturnsImageJpeg()
    {
        byte[] header = [0xFF, 0xD8, 0xFF, 0xE0];
        PhotoFileValidator.DetectContentType(header).Should().Be("image/jpeg");
    }

    [Fact]
    public void DetectContentType_Png_ReturnsImagePng()
    {
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        PhotoFileValidator.DetectContentType(header).Should().Be("image/png");
    }

    [Fact]
    public void DetectContentType_WebP_ReturnsImageWebp()
    {
        // RIFF....WEBP
        byte[] header = [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];
        PhotoFileValidator.DetectContentType(header).Should().Be("image/webp");
    }

    [Fact]
    public void DetectContentType_UnknownBytes_ReturnsNull()
    {
        byte[] header = [0x00, 0x00, 0x00, 0x00];
        PhotoFileValidator.DetectContentType(header).Should().BeNull();
    }

    [Fact]
    public void DetectContentType_TooShort_ReturnsNull()
    {
        byte[] header = [0xFF, 0xD8];
        PhotoFileValidator.DetectContentType(header).Should().BeNull();
    }

    [Fact]
    public void DetectContentType_Pdf_ReturnsNull()
    {
        // PDF magic bytes — not allowed for photos
        byte[] header = [0x25, 0x50, 0x44, 0x46];
        PhotoFileValidator.DetectContentType(header).Should().BeNull();
    }

    [Fact]
    public void DetectContentType_Gif_ReturnsNull()
    {
        // GIF89a — not allowed for photos
        byte[] header = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];
        PhotoFileValidator.DetectContentType(header).Should().BeNull();
    }

    [Fact]
    public void DetectContentType_RiffButNotWebP_ReturnsNull()
    {
        // RIFF header but AVI instead of WEBP
        byte[] header = [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x41, 0x56, 0x49, 0x20];
        PhotoFileValidator.DetectContentType(header).Should().BeNull();
    }

    // --- IsValid ---

    [Fact]
    public void IsValid_JpegDeclaredAndMatches_ReturnsTrue()
    {
        byte[] data = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
        PhotoFileValidator.IsValid("image/jpeg", data).Should().BeTrue();
    }

    [Fact]
    public void IsValid_PngDeclaredAndMatches_ReturnsTrue()
    {
        byte[] data = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A];
        PhotoFileValidator.IsValid("image/png", data).Should().BeTrue();
    }

    [Fact]
    public void IsValid_WebPDeclaredAndMatches_ReturnsTrue()
    {
        byte[] data = [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];
        PhotoFileValidator.IsValid("image/webp", data).Should().BeTrue();
    }

    [Fact]
    public void IsValid_DeclaredJpegButActuallyPng_ReturnsFalse()
    {
        byte[] pngData = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A];
        PhotoFileValidator.IsValid("image/jpeg", pngData).Should().BeFalse();
    }

    [Fact]
    public void IsValid_DeclaredPngButActuallyJpeg_ReturnsFalse()
    {
        byte[] jpegData = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
        PhotoFileValidator.IsValid("image/png", jpegData).Should().BeFalse();
    }

    [Fact]
    public void IsValid_DisallowedContentType_ReturnsFalse()
    {
        byte[] pdfData = [0x25, 0x50, 0x44, 0x46];
        PhotoFileValidator.IsValid("application/pdf", pdfData).Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullContentType_ReturnsFalse()
    {
        byte[] data = [0xFF, 0xD8, 0xFF, 0xE0];
        PhotoFileValidator.IsValid(null, data).Should().BeFalse();
    }

    [Fact]
    public void IsValid_EmptyContentType_ReturnsFalse()
    {
        byte[] data = [0xFF, 0xD8, 0xFF, 0xE0];
        PhotoFileValidator.IsValid("", data).Should().BeFalse();
    }

    [Fact]
    public void IsValid_UnrecognizedMagicBytes_ReturnsFalse()
    {
        byte[] randomBytes = [0x00, 0x01, 0x02, 0x03, 0x04];
        PhotoFileValidator.IsValid("image/jpeg", randomBytes).Should().BeFalse();
    }

    [Fact]
    public void IsValid_DeclaredJpegButExeFile_ReturnsFalse()
    {
        // MZ header (Windows EXE)
        byte[] exeData = [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00];
        PhotoFileValidator.IsValid("image/jpeg", exeData).Should().BeFalse();
    }
}
