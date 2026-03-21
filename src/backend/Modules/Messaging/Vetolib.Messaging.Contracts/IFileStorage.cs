namespace Vetolib.Messaging.Contracts;

public interface IFileStorage
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default);
}
