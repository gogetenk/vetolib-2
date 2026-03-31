using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Vetolib.Auth.Application.Commands.ReceiveWebhook;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ReceiveWebhookHmacTests
{
    private const string Secret = "super-secret-key-1234567890";

    [Fact]
    public void VerifyHmacSignature_valid_signature_returns_true()
    {
        var body = "{\"eventType\":\"lab.result\",\"payload\":{}}";
        var signature = ComputeSignature(body, Secret);

        var result = ReceiveWebhookHandler.VerifyHmacSignature(body, signature, Secret);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHmacSignature_invalid_signature_returns_false()
    {
        var body = "{\"eventType\":\"lab.result\",\"payload\":{}}";

        var result = ReceiveWebhookHandler.VerifyHmacSignature(body, "invalidsignature", Secret);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHmacSignature_wrong_secret_returns_false()
    {
        var body = "{\"eventType\":\"lab.result\",\"payload\":{}}";
        var signature = ComputeSignature(body, Secret);

        var result = ReceiveWebhookHandler.VerifyHmacSignature(body, signature, "wrong-secret-key-1234567890");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHmacSignature_tampered_body_returns_false()
    {
        var originalBody = "{\"eventType\":\"lab.result\",\"payload\":{}}";
        var signature = ComputeSignature(originalBody, Secret);
        var tamperedBody = "{\"eventType\":\"lab.result\",\"payload\":{\"hacked\":true}}";

        var result = ReceiveWebhookHandler.VerifyHmacSignature(tamperedBody, signature, Secret);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHmacSignature_empty_body_with_matching_signature_returns_true()
    {
        var body = "";
        var signature = ComputeSignature(body, Secret);

        var result = ReceiveWebhookHandler.VerifyHmacSignature(body, signature, Secret);

        result.Should().BeTrue();
    }

    private static string ComputeSignature(string body, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bodyBytes);
        return Convert.ToHexStringLower(hash);
    }
}
