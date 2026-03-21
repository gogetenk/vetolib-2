using FluentAssertions;
using Microsoft.Extensions.Options;
using Vetolib.Messaging.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalFileStorage _sut;

    public LocalFileStorageTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"vetolib-test-{Guid.NewGuid():N}");
        var options = Options.Create(new LocalFileStorageOptions { BasePath = _testBasePath });
        _sut = new LocalFileStorage(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, recursive: true);
        }
    }

    [Fact]
    public async Task UploadAsync_StoresFileAndReturnsRelativePath()
    {
        // Arrange
        var content = "Hello, attachment!"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var path = await _sut.UploadAsync(stream, "photo.jpg", "image/jpeg");

        // Assert
        path.Should().EndWith("photo.jpg");
        path.Should().Contain(Path.DirectorySeparatorChar.ToString());

        var fullPath = Path.Combine(_testBasePath, path);
        File.Exists(fullPath).Should().BeTrue();
        var stored = await File.ReadAllBytesAsync(fullPath);
        stored.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingFile()
    {
        // Arrange
        using var stream = new MemoryStream("data"u8.ToArray());
        var path = await _sut.UploadAsync(stream, "to-delete.txt", "text/plain");
        var fullPath = Path.Combine(_testBasePath, path);
        File.Exists(fullPath).Should().BeTrue();

        // Act
        await _sut.DeleteAsync(path);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentFile_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _sut.DeleteAsync("nonexistent/file.txt");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPresignedUrlAsync_ReturnsFileUri()
    {
        // Arrange
        using var stream = new MemoryStream("url-test"u8.ToArray());
        var path = await _sut.UploadAsync(stream, "doc.pdf", "application/pdf");

        // Act
        var url = await _sut.GetPresignedUrlAsync(path, TimeSpan.FromMinutes(15));

        // Assert
        url.Should().StartWith("file:///");
        url.Should().Contain("doc.pdf");
    }
}
