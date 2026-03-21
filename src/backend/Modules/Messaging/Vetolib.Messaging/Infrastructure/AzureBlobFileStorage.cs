using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Infrastructure;

internal sealed class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "attachments";
}

internal sealed class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobFileStorage(IOptions<AzureBlobStorageOptions> options)
    {
        var opts = options.Value;
        var serviceClient = new BlobServiceClient(opts.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(opts.ContainerName);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobPath = $"{Guid.NewGuid():N}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobPath);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers }, ct);

        return blobPath;
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public Task<string> GetPresignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = path,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var uri = blobClient.GenerateSasUri(sasBuilder);
        return Task.FromResult(uri.ToString());
    }
}
