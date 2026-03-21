using Microsoft.Extensions.Options;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Infrastructure;

internal sealed class LocalFileStorageOptions
{
    public const string SectionName = "LocalFileStorage";
    public string BasePath { get; set; } = Path.Combine(Path.GetTempPath(), "vetolib-attachments");
}

internal sealed class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options)
    {
        _basePath = options.Value.BasePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var relativePath = Path.Combine(Guid.NewGuid().ToString("N"), fileName);
        var fullPath = Path.Combine(_basePath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        return relativePath;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(_basePath)))
            throw new InvalidOperationException("Invalid file path");

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default)
    {
        // In local dev, return a file:// URI pointing to the stored file
        var fullPath = Path.Combine(_basePath, path);
        if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(_basePath)))
            throw new InvalidOperationException("Invalid file path");

        return Task.FromResult(new Uri(fullPath).AbsoluteUri);
    }
}
