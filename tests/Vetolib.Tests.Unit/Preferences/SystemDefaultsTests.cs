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
        SystemDefaults.GetDefault(PreferenceKey.AIDrugInteractions).Should().Be("true");
    }

    [Fact]
    public void AnalyticsPosthog_DefaultIsFalse()
    {
        // GDPR-ready: analytics OFF by default
        SystemDefaults.GetDefault(PreferenceKey.AnalyticsPosthog).Should().Be("false");
    }

    [Fact]
    public void AnalyticsUsageData_DefaultIsFalse()
    {
        // GDPR-ready: analytics OFF by default
        SystemDefaults.GetDefault(PreferenceKey.AnalyticsUsageData).Should().Be("false");
    }

    [Fact]
    public void NotificationEmail_DefaultIsTrue()
    {
        SystemDefaults.GetDefault(PreferenceKey.NotificationEmail).Should().Be("true");
    }

    [Fact]
    public void NotificationPush_DefaultIsFalse()
    {
        // Push not yet implemented
        SystemDefaults.GetDefault(PreferenceKey.NotificationPush).Should().Be("false");
    }

    [Fact]
    public void NotificationSms_DefaultIsFalse()
    {
        // SMS not yet implemented
        SystemDefaults.GetDefault(PreferenceKey.NotificationSms).Should().Be("false");
    }

    [Fact]
    public void PrivacyDataSharing_DefaultIsFalse()
    {
        SystemDefaults.GetDefault(PreferenceKey.PrivacyDataSharing).Should().Be("false");
    }

    [Fact]
    public void PrivacyMarketing_DefaultIsFalse()
    {
        SystemDefaults.GetDefault(PreferenceKey.PrivacyMarketing).Should().Be("false");
    }

    [Fact]
    public void CommunicationLanguage_DefaultIsEnglish()
    {
        // UAE market — English default
        SystemDefaults.GetDefault(PreferenceKey.CommunicationLanguage).Should().Be("en");
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
            var act = () => SystemDefaults.GetCategory(key);
            act.Should().NotThrow();
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
        SystemDefaults.GetCategory(key).Should().Be(expected);
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
