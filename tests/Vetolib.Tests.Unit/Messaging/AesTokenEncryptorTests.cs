using FluentAssertions;
using Vetolib.Messaging.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class AesTokenEncryptorTests
{
    private readonly AesTokenEncryptor _encryptor;

    public AesTokenEncryptorTests()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        _encryptor = new AesTokenEncryptor(key);
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginalText()
    {
        var original = "EAABzbX1234567890_test_token";

        var encrypted = _encryptor.Encrypt(original);
        var decrypted = _encryptor.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachTime()
    {
        var text = "same_token";

        var encrypted1 = _encryptor.Encrypt(text);
        var encrypted2 = _encryptor.Encrypt(text);

        encrypted1.Should().NotBe(encrypted2, "random nonce should produce different ciphertexts");
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_ThrowsArgumentException()
    {
        var shortKey = new byte[16];

        var act = () => new AesTokenEncryptor(shortKey);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*32 bytes*");
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsCryptographicException()
    {
        var key1 = new byte[32];
        var key2 = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key1);
        System.Security.Cryptography.RandomNumberGenerator.Fill(key2);

        var encryptor1 = new AesTokenEncryptor(key1);
        var encryptor2 = new AesTokenEncryptor(key2);

        var encrypted = encryptor1.Encrypt("my_secret_token");

        var act = () => encryptor2.Decrypt(encrypted);

        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }
}
