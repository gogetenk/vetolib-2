namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Validates file types using magic bytes (file signatures), not just Content-Type headers.
/// </summary>
internal static class FileTypeValidator
{
    private const int MinHeaderSize = 4;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const int MaxFilesPerUpload = 5;

    // Magic byte signatures
    private static readonly byte[] PdfSignature = "%PDF"u8.ToArray();
    private static readonly byte[] JpgSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];

    /// <summary>
    /// Returns the validated content type based on magic bytes, or null if invalid.
    /// </summary>
    public static string? DetectContentType(ReadOnlySpan<byte> header)
    {
        if (header.Length < MinHeaderSize)
            return null;

        if (header[..3].SequenceEqual(JpgSignature))
            return "image/jpeg";

        if (header[..4].SequenceEqual(PngSignature))
            return "image/png";

        if (header[..4].SequenceEqual(PdfSignature))
            return "application/pdf";

        return null;
    }

    public static bool IsValidFileSize(long sizeBytes) => sizeBytes > 0 && sizeBytes <= MaxFileSizeBytes;

    public static bool IsValidFileCount(int count) => count > 0 && count <= MaxFilesPerUpload;
}
