using FluentAssertions;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class SystemDefaultsTests
{
    // ─── Coverage ─────────────────────────────────────────────────────────────

    [Fact]
    public void AllPreferenceKeys_HaveASystemDefault()
    {
        var allKeys = Enum.GetValues<PreferenceKey>();

        foreach (var key in allKeys)
        {
            SystemDefaults.HasDefault(key).Should().BeTrue(
                $"PreferenceKey.{key} must have a hardcoded system default in SystemDefaults");
        }
    }

    [Fact]
    public void GetDefault_ForEveryKey_DoesNotThrow()
    {
        var allKeys = Enum.GetValues<PreferenceKey>();

        var act = () =>
        {
            foreach (var key in allKeys)
                SystemDefaults.GetDefault(key);
        };

        act.Should().NotThrow();
    }

    // ─── Specific business defaults ───────────────────────────────────────────

    [Fact]
    public void AIDrugInteractions_DefaultIsTrue()
    {
        // ALWAYS ON per PO decision
        var result = SystemDefaults.GetDefault(PreferenceKey.AIDrugInteractions);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public void AnalyticsPosthog_DefaultIsFalse()
    {
        // GDPR-ready: analytics OFF by default
        var result = SystemDefaults.GetDefault(PreferenceKey.AnalyticsPosthog);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void AnalyticsUsageData_DefaultIsFalse()
    {
        // GDPR-ready: analytics OFF by default
        var result = SystemDefaults.GetDefault(PreferenceKey.AnalyticsUsageData);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void NotificationEmail_DefaultIsTrue()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public void NotificationPush_DefaultIsFalse()
    {
        // Push not yet implemented
        var result = SystemDefaults.GetDefault(PreferenceKey.NotificationPush);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void NotificationSms_DefaultIsFalse()
    {
        // SMS not yet implemented
        var result = SystemDefaults.GetDefault(PreferenceKey.NotificationSms);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void PrivacyDataSharing_DefaultIsFalse()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.PrivacyDataSharing);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void PrivacyMarketing_DefaultIsFalse()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.PrivacyMarketing);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void CommunicationLanguage_DefaultIsEnglish()
    {
        // UAE market — English default
        var result = SystemDefaults.GetDefault(PreferenceKey.CommunicationLanguage);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("en");
    }

    [Fact]
    public void All_ReturnsAllKeys()
    {
        var allKeyCount = Enum.GetValues<PreferenceKey>().Length;
        SystemDefaults.All.Count.Should().Be(allKeyCount);
    }

    [Fact]
    public void AllPreferenceKeys_HaveACategoryMapping()
    {
        var allKeys = Enum.GetValues<PreferenceKey>();
        foreach (var key in allKeys)
        {
            var result = SystemDefaults.GetCategory(key);
            result.IsSuccess.Should().BeTrue(
                $"PreferenceKey.{key} must have a category mapping in SystemDefaults");
        }
    }

    [Theory]
    [InlineData(PreferenceKey.NotificationEmail, PreferenceCategory.Notifications)]
    [InlineData(PreferenceKey.AnalyticsPosthog, PreferenceCategory.Analytics)]
    [InlineData(PreferenceKey.AITriage, PreferenceCategory.AIFeatures)]
    [InlineData(PreferenceKey.AIDrugInteractions, PreferenceCategory.AIFeatures)]
    [InlineData(PreferenceKey.CommunicationLanguage, PreferenceCategory.Communication)]
    [InlineData(PreferenceKey.PrivacyDataSharing, PreferenceCategory.Privacy)]
    public void GetCategory_ReturnsCorrectCategoryForKey(PreferenceKey key, PreferenceCategory expected)
    {
        var result = SystemDefaults.GetCategory(key);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void GetDefault_UnknownKey_ThrowsInvalidOperationException()
    {
        // Cast an out-of-range int to PreferenceKey to simulate an unknown key
        var unknownKey = (PreferenceKey)9999;

        var act = () => SystemDefaults.GetDefault(unknownKey);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No system default defined for PreferenceKey*");
    }

    [Fact]
    public void GetCategory_UnknownKey_ThrowsInvalidOperationException()
    {
        var unknownKey = (PreferenceKey)9999;

        var act = () => SystemDefaults.GetCategory(unknownKey);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No category mapping defined for PreferenceKey*");
    }

    [Fact]
    public void HasDefault_UnknownKey_ReturnsFalse()
    {
        var unknownKey = (PreferenceKey)9999;

        SystemDefaults.HasDefault(unknownKey).Should().BeFalse();
    }
}
