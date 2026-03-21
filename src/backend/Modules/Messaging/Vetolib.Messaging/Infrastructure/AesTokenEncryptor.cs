using System.Security.Cryptography;

namespace Vetolib.Messaging.Infrastructure;

/// <summary>
/// AES-256-GCM encryption for WhatsApp access tokens at rest.
/// The encryption key is read from configuration (WhatsApp:EncryptionKey).
/// </summary>
internal sealed class AesTokenEncryptor : ITokenEncryptor
{
    private readonly byte[] _key;

    public AesTokenEncryptor(byte[] key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Encryption key must be 32 bytes (AES-256).", nameof(key));
        _key = key;
    }

    public string Encrypt(string plainText)
    {
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Format: nonce (12) + tag (16) + cipher
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        cipherBytes.CopyTo(result, nonce.Length + tag.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var data = Convert.FromBase64String(cipherText);

        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;

        var nonce = data[..nonceSize];
        var tag = data[nonceSize..(nonceSize + tagSize)];
        var cipherBytes = data[(nonceSize + tagSize)..];

        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}
