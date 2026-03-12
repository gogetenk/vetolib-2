using Ardalis.Result;
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

    // ─── Booking defaults ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(PreferenceKey.BookingEnabled)]
    [InlineData(PreferenceKey.BookingMaxAdvanceDays)]
    [InlineData(PreferenceKey.BookingMinCancelHours)]
    [InlineData(PreferenceKey.BookingMaxReschedules)]
    [InlineData(PreferenceKey.BookingSlotGridMinutes)]
    public void BookingKeys_HaveADefault(PreferenceKey key)
    {
        SystemDefaults.HasDefault(key).Should().BeTrue(
            $"PreferenceKey.{key} must have a hardcoded system default in SystemDefaults");
    }

    [Theory]
    [InlineData(PreferenceKey.BookingEnabled, PreferenceCategory.Booking)]
    [InlineData(PreferenceKey.BookingMaxAdvanceDays, PreferenceCategory.Booking)]
    [InlineData(PreferenceKey.BookingMinCancelHours, PreferenceCategory.Booking)]
    [InlineData(PreferenceKey.BookingMaxReschedules, PreferenceCategory.Booking)]
    [InlineData(PreferenceKey.BookingSlotGridMinutes, PreferenceCategory.Booking)]
    public void BookingKeys_HaveBookingCategory(PreferenceKey key, PreferenceCategory expected)
    {
        var result = SystemDefaults.GetCategory(key);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void BookingEnabled_DefaultIsFalse()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.BookingEnabled);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public void BookingMaxAdvanceDays_DefaultIs28()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.BookingMaxAdvanceDays);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("28");
    }

    [Fact]
    public void BookingMinCancelHours_DefaultIs24()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.BookingMinCancelHours);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("24");
    }

    [Fact]
    public void BookingMaxReschedules_DefaultIs2()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.BookingMaxReschedules);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("2");
    }

    [Fact]
    public void BookingSlotGridMinutes_DefaultIs30()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.BookingSlotGridMinutes);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("30");
    }

    [Fact]
    public void GetDefault_UnknownKey_ReturnsNotFound()
    {
        // Cast an out-of-range int to PreferenceKey to simulate an unknown key
        var unknownKey = (PreferenceKey)9999;

        var result = SystemDefaults.GetDefault(unknownKey);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainMatch("*No system default defined for PreferenceKey*");
    }

    [Fact]
    public void GetCategory_UnknownKey_ReturnsNotFound()
    {
        var unknownKey = (PreferenceKey)9999;

        var result = SystemDefaults.GetCategory(unknownKey);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainMatch("*No category mapping defined for PreferenceKey*");
    }

    [Fact]
    public void HasDefault_UnknownKey_ReturnsFalse()
    {
        var unknownKey = (PreferenceKey)9999;

        SystemDefaults.HasDefault(unknownKey).Should().BeFalse();
    }

    // ─── AI defaults ────────────────────────────────────────────────────────────

    [Fact]
    public void AITriage_DefaultIsTrue()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.AITriage);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public void AINoShow_DefaultIsTrue()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.AINoShow);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public void AIMessaging_DefaultIsTrue()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.AIMessaging);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    // ─── Communication defaults ─────────────────────────────────────────────────

    [Fact]
    public void CommunicationQuietHoursStart_DefaultIs2200()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.CommunicationQuietHoursStart);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("22:00");
    }

    [Fact]
    public void CommunicationQuietHoursEnd_DefaultIs0700()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.CommunicationQuietHoursEnd);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("07:00");
    }

    [Fact]
    public void CommunicationPreferredChannel_DefaultIsEmail()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.CommunicationPreferredChannel);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Email");
    }

    // ─── Notification defaults ──────────────────────────────────────────────────

    [Fact]
    public void NotificationAppointmentReminder_DefaultIsTrue()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.NotificationAppointmentReminder);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public void NotificationInvoice_DefaultIsTrue()
    {
        var result = SystemDefaults.GetDefault(PreferenceKey.NotificationInvoice);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    // ─── All dictionary ─────────────────────────────────────────────────────────

    [Fact]
    public void All_ContainsEveryDefinedKey()
    {
        var allKeys = Enum.GetValues<PreferenceKey>();

        foreach (var key in allKeys)
        {
            SystemDefaults.All.Should().ContainKey(key,
                $"SystemDefaults.All must include PreferenceKey.{key}");
        }
    }

    [Fact]
    public void All_ValuesAreNotNullOrEmpty()
    {
        foreach (var kvp in SystemDefaults.All)
        {
            kvp.Value.Should().NotBeNullOrWhiteSpace(
                $"SystemDefaults.All[{kvp.Key}] must have a non-empty value");
        }
    }
}
