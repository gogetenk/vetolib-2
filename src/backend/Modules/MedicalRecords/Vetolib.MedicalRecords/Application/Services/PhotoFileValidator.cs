namespace Vetolib.MedicalRecords.Application.Services;

/// <summary>
/// Validates patient photo files using magic bytes (file signatures), not just Content-Type headers.
/// Prevents uploading disguised files (e.g., EXE renamed to .jpg).
/// </summary>
internal static class PhotoFileValidator
{
    private const int MinHeaderSize = 4;

    /// <summary>
    /// Allowed content types for patient photos.
    /// </summary>
    public static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    // Magic byte signatures
    private static readonly byte[] JpgSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] WebPSignature = [0x52, 0x49, 0x46, 0x46]; // "RIFF"
    private static readonly byte[] WebPFormat = [0x57, 0x45, 0x42, 0x50];    // "WEBP" at offset 8

    /// <summary>
    /// Returns the validated content type based on magic bytes, or null if the file type is not recognized.
    /// </summary>
    public static string? DetectContentType(ReadOnlySpan<byte> header)
    {
        if (header.Length < MinHeaderSize)
            return null;

        if (header.Length >= 3 && header[..3].SequenceEqual(JpgSignature))
            return "image/jpeg";

        if (header[..4].SequenceEqual(PngSignature))
            return "image/png";

        // WebP: starts with "RIFF" and has "WEBP" at offset 8
        if (header.Length >= 12
            && header[..4].SequenceEqual(WebPSignature)
            && header[8..12].SequenceEqual(WebPFormat))
            return "image/webp";

        return null;
    }

    /// <summary>
    /// Validates that the declared content type is allowed AND matches the actual file magic bytes.
    /// Returns true if valid, false otherwise.
    /// </summary>
    public static bool IsValid(string? declaredContentType, ReadOnlySpan<byte> fileData)
    {
        if (string.IsNullOrWhiteSpace(declaredContentType))
            return false;

        if (!AllowedContentTypes.Contains(declaredContentType))
            return false;

        var detected = DetectContentType(fileData);

        // Magic bytes must match declared content type
        return detected is not null && detected == declaredContentType;
    }
}
