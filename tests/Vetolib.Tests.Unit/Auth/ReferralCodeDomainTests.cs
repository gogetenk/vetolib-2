using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ReferralCodeDomainTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    // ─── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ReferralCode.Create(ValidUserId, "ABCD1234");

        result.IsSuccess.Should().BeTrue();
        result.Value.OwnerUserId.Should().Be(ValidUserId);
        result.Value.Code.Should().Be("ABCD1234");
        result.Value.UsageCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithEmptyUserId_ReturnsInvalid()
    {
        var result = ReferralCode.Create(Guid.Empty, "ABCD1234");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "ownerUserId");
    }

    [Fact]
    public void Create_WithEmptyCode_ReturnsInvalid()
    {
        var result = ReferralCode.Create(ValidUserId, "");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "code");
    }

    [Fact]
    public void Create_WithTooLongCode_ReturnsInvalid()
    {
        var result = ReferralCode.Create(ValidUserId, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "code");
    }

    [Fact]
    public void Create_NormalizesCodeToUpperCase()
    {
        var result = ReferralCode.Create(ValidUserId, "abcd1234");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("ABCD1234");
    }

    // ─── IncrementUsage ──────────────────────────────────────────

    [Fact]
    public void IncrementUsage_IncrementsCount()
    {
        var referralCode = ReferralCode.Create(ValidUserId, "ABCD1234").Value;

        var result = referralCode.IncrementUsage();

        result.IsSuccess.Should().BeTrue();
        referralCode.UsageCount.Should().Be(1);
    }

    [Fact]
    public void IncrementUsage_MultipleTimes_TracksCorrectly()
    {
        var referralCode = ReferralCode.Create(ValidUserId, "ABCD1234").Value;

        referralCode.IncrementUsage();
        referralCode.IncrementUsage();
        referralCode.IncrementUsage();

        referralCode.UsageCount.Should().Be(3);
    }

    // ─── GenerateCode ────────────────────────────────────────────

    [Fact]
    public void GenerateCode_Returns8Characters()
    {
        var code = ReferralCode.GenerateCode();

        code.Should().HaveLength(8);
    }

    [Fact]
    public void GenerateCode_ContainsOnlyValidCharacters()
    {
        const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        var code = ReferralCode.GenerateCode();

        code.Should().MatchRegex($"^[{validChars}]+$");
    }

    [Fact]
    public void GenerateCode_ProducesUniqueValues()
    {
        var codes = Enumerable.Range(0, 100).Select(_ => ReferralCode.GenerateCode()).ToHashSet();

        // With 8 chars from 32-char alphabet, collisions in 100 should be essentially impossible
        codes.Count.Should().Be(100);
    }
}
