using FluentAssertions;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class ClinicPreferenceDefaultDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AITriage, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(PreferenceCategory.AIFeatures);
        result.Value.Key.Should().Be(PreferenceKey.AITriage);
        result.Value.Value.Should().Be("true");
    }

    [Fact]
    public void Create_CategoryIsDerivedFromKey()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.NotificationEmail, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(PreferenceCategory.Notifications);
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = ClinicPreferenceDefault.Create(
            Guid.Empty, PreferenceKey.AITriage, "true");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyValue_ReturnsInvalid()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AITriage, "");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "value");
    }

    [Fact]
    public void Create_AIDrugInteractions_SetToFalse_ReturnsInvalid()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AIDrugInteractions, "false");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "key");
    }

    [Fact]
    public void Update_WithValidValue_ReturnsSuccess()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AnalyticsPosthog, "false").Value;

        var result = pref.Update("true");

        result.IsSuccess.Should().BeTrue();
        pref.Value.Should().Be("true");
    }

    [Fact]
    public void Update_WithEmptyValue_ReturnsInvalid()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AnalyticsPosthog, "false").Value;

        var result = pref.Update("");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "newValue");
    }

    [Fact]
    public void Update_WithWhitespaceValue_ReturnsInvalid()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AnalyticsPosthog, "false").Value;

        var result = pref.Update("   ");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "newValue");
    }

    [Fact]
    public void Update_AIDrugInteractions_SetToFalse_ReturnsInvalid()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AIDrugInteractions, "true").Value;

        var result = pref.Update("false");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_AIDrugInteractions_SetToFalseCaseInsensitive_ReturnsInvalid()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AIDrugInteractions, "true").Value;

        var result = pref.Update("FALSE");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "newValue");
    }

    [Fact]
    public void ToDto_ReturnsClinicSource()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.NotificationSms, "false").Value;

        var dto = pref.ToDto();

        dto.Source.Should().Be(PreferenceSource.Clinic);
        dto.Key.Should().Be(PreferenceKey.NotificationSms);
    }

    [Fact]
    public void Create_WithWhitespaceValue_ReturnsInvalid()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AITriage, "   ");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "value");
    }

    [Fact]
    public void Create_AIDrugInteractions_SetToFalseCaseInsensitive_ReturnsInvalid()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AIDrugInteractions, "FALSE");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "key");
    }

    [Fact]
    public void Create_AIDrugInteractions_SetToTrue_ReturnsSuccess()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AIDrugInteractions, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("true");
    }

    [Fact]
    public void Create_SetsClinicId()
    {
        var result = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.NotificationEmail, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ValidClinicId);
    }

    [Fact]
    public void Create_WithMultipleErrors_ReturnsAllValidationErrors()
    {
        var result = ClinicPreferenceDefault.Create(
            Guid.Empty, PreferenceKey.AIDrugInteractions, "false");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
        result.ValidationErrors.Should().Contain(e => e.Identifier == "key");
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.AnalyticsPosthog, "false").Value;
        var beforeUpdate = pref.UpdatedAt;

        var updateResult = pref.Update("true");
        updateResult.IsSuccess.Should().BeTrue();

        pref.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void ToDto_ReturnsCorrectCategoryAndValue()
    {
        var pref = ClinicPreferenceDefault.Create(
            ValidClinicId, PreferenceKey.CommunicationLanguage, "ar").Value;

        var dto = pref.ToDto();

        dto.Category.Should().Be(PreferenceCategory.Communication);
        dto.Value.Should().Be("ar");
    }

    [Theory]
    [InlineData(PreferenceKey.BookingEnabled, PreferenceCategory.Booking)]
    [InlineData(PreferenceKey.PrivacyMarketing, PreferenceCategory.Privacy)]
    [InlineData(PreferenceKey.AINoShow, PreferenceCategory.AIFeatures)]
    public void Create_AssignsCorrectCategory(PreferenceKey key, PreferenceCategory expectedCategory)
    {
        var result = ClinicPreferenceDefault.Create(ValidClinicId, key, "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(expectedCategory);
    }
}
