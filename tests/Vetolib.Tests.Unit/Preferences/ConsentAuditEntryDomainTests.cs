using FluentAssertions;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class ConsentAuditEntryDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();
    private static readonly Guid ValidUserId = Guid.NewGuid();

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ConsentAuditEntry.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail,
            previousValue: "true", newValue: "false",
            source: PreferenceSource.User);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreviousValue.Should().Be("true");
        result.Value.NewValue.Should().Be("false");
        result.Value.Source.Should().Be(PreferenceSource.User);
    }

    [Fact]
    public void Create_WithNullPreviousValue_ReturnsSuccess()
    {
        // First-time consent recording has no previous value
        var result = ConsentAuditEntry.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Privacy, PreferenceKey.PrivacyMarketing,
            previousValue: null, newValue: "false",
            source: PreferenceSource.User);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreviousValue.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = ConsentAuditEntry.Create(
            Guid.Empty, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail,
            null, "true", PreferenceSource.User);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyUserId_ReturnsInvalid()
    {
        var result = ConsentAuditEntry.Create(
            ValidClinicId, Guid.Empty,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail,
            null, "true", PreferenceSource.User);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "userId");
    }

    [Fact]
    public void Create_WithEmptyNewValue_ReturnsInvalid()
    {
        var result = ConsentAuditEntry.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Notifications, PreferenceKey.NotificationEmail,
            "true", "", PreferenceSource.User);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "newValue");
    }

    // ─── Immutability ─────────────────────────────────────────────────────────

    [Fact]
    public void ConsentAuditEntry_HasNoUpdateMethod()
    {
        // Verify the type does not expose any mutation method other than factory Create.
        // This is a structural test: the type should be append-only.
        var type = typeof(ConsentAuditEntry);
        var publicMethods = type.GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.Name != "ToString" && m.Name != "GetHashCode"
                        && m.Name != "GetType" && m.Name != "Equals"
                        && !m.Name.StartsWith("get_"))
            .Select(m => m.Name)
            .ToList();

        // Only ToDto and DomainEvent methods should be public instance methods
        publicMethods.Should().NotContain("Update",
            "ConsentAuditEntry is append-only and must not expose an Update method");
    }

    // ─── ToDto ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToDto_ReturnsCorrectData()
    {
        var entry = ConsentAuditEntry.Create(
            ValidClinicId, ValidUserId,
            PreferenceCategory.Privacy, PreferenceKey.PrivacyDataSharing,
            "true", "false", PreferenceSource.Clinic).Value;

        var dto = entry.ToDto();

        dto.UserId.Should().Be(ValidUserId);
        dto.Category.Should().Be(PreferenceCategory.Privacy);
        dto.Key.Should().Be(PreferenceKey.PrivacyDataSharing);
        dto.PreviousValue.Should().Be("true");
        dto.NewValue.Should().Be("false");
        dto.Source.Should().Be(PreferenceSource.Clinic);
    }
}
