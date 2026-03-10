using FluentAssertions;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class UserPreferenceDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();
    private static readonly Guid ValidUserId = Guid.NewGuid();

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(PreferenceCategory.Notifications);
        result.Value.Key.Should().Be(PreferenceKey.NotificationEmail);
        result.Value.Value.Should().Be("true");
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = UserPreference.Create(
            Guid.Empty, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail, "true");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyUserId_ReturnsInvalid()
    {
        var result = UserPreference.Create(
            ValidClinicId, Guid.Empty,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail, "true");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "userId");
    }

    [Fact]
    public void Create_WithEmptyValue_ReturnsInvalid()
    {
        var result = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail, "");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "value");
    }

    [Fact]
    public void Create_AIDrugInteractions_SetToFalse_ReturnsInvalid()
    {
        // PO decision: AIDrugInteractions is ALWAYS ON and cannot be disabled
        var result = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.AIFeatures, PreferenceKey.AIDrugInteractions, "false");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "key");
    }

    [Fact]
    public void Create_AIDrugInteractions_SetToTrue_ReturnsSuccess()
    {
        var result = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.AIFeatures, PreferenceKey.AIDrugInteractions, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("true");
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidValue_ReturnsSuccess()
    {
        var pref = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail, "true").Value;

        var result = pref.Update("false");

        result.IsSuccess.Should().BeTrue();
        pref.Value.Should().Be("false");
    }

    [Fact]
    public void Update_WithEmptyValue_ReturnsInvalid()
    {
        var pref = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail, "true").Value;

        var result = pref.Update("");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "newValue");
    }

    [Fact]
    public void Update_AIDrugInteractions_SetToFalse_ReturnsInvalid()
    {
        // PO decision: AIDrugInteractions is ALWAYS ON and cannot be disabled
        var pref = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.AIFeatures, PreferenceKey.AIDrugInteractions, "true").Value;

        var result = pref.Update("false");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "newValue");
    }

    // ─── ToDto ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToDto_ReturnsUserSource()
    {
        var pref = UserPreference.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Analytics, PreferenceKey.AnalyticsPosthog, "false").Value;

        var dto = pref.ToDto();

        dto.Source.Should().Be(PreferenceSource.User);
        dto.Key.Should().Be(PreferenceKey.AnalyticsPosthog);
        dto.Value.Should().Be("false");
    }
}
