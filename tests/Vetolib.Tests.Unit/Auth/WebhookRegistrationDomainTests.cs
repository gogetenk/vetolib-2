using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class WebhookRegistrationDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();
    private const string ValidName = "IDEXX Lab Integration";
    private const string ValidSecret = "super-secret-key-1234567890";
    private static readonly List<string> ValidEventTypes = ["lab.result"];

    [Fact]
    public void Create_with_valid_data_succeeds()
    {
        var result = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ValidClinicId);
        result.Value.Name.Should().Be(ValidName);
        result.Value.Secret.Should().Be(ValidSecret);
        result.Value.EventTypes.Should().ContainSingle().Which.Should().Be("lab.result");
        result.Value.IsActive.Should().BeTrue();
        result.Value.LastCalledAt.Should().BeNull();
    }

    [Fact]
    public void Create_with_empty_clinicId_fails()
    {
        var result = WebhookRegistration.Create(Guid.Empty, ValidName, ValidSecret, ValidEventTypes);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_with_empty_name_fails()
    {
        var result = WebhookRegistration.Create(ValidClinicId, "", ValidSecret, ValidEventTypes);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "name");
    }

    [Fact]
    public void Create_with_empty_secret_fails()
    {
        var result = WebhookRegistration.Create(ValidClinicId, ValidName, "", ValidEventTypes);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "secret");
    }

    [Fact]
    public void Create_with_short_secret_fails()
    {
        var result = WebhookRegistration.Create(ValidClinicId, ValidName, "short", ValidEventTypes);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "secret");
    }

    [Fact]
    public void Create_with_empty_event_types_fails()
    {
        var result = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, []);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "eventTypes");
    }

    [Fact]
    public void Create_with_invalid_event_type_fails()
    {
        var result = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ["invalid.type"]);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "eventTypes");
    }

    [Fact]
    public void Create_with_multiple_valid_event_types_succeeds()
    {
        var result = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ["lab.result", "external.record"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.EventTypes.Should().HaveCount(2);
    }

    [Fact]
    public void Deactivate_active_registration_succeeds()
    {
        var registration = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes).Value;

        var result = registration.Deactivate();

        result.IsSuccess.Should().BeTrue();
        registration.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_already_inactive_registration_fails()
    {
        var registration = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes).Value;
        registration.Deactivate();

        var result = registration.Deactivate();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void RecordCall_updates_LastCalledAt()
    {
        var registration = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes).Value;

        registration.RecordCall();

        registration.LastCalledAt.Should().NotBeNull();
        registration.LastCalledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SupportsEventType_returns_true_for_registered_type()
    {
        var registration = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes).Value;

        registration.SupportsEventType("lab.result").Should().BeTrue();
    }

    [Fact]
    public void SupportsEventType_returns_false_for_unregistered_type()
    {
        var registration = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes).Value;

        registration.SupportsEventType("external.record").Should().BeFalse();
    }

    [Fact]
    public void ToDto_maps_correctly()
    {
        var registration = WebhookRegistration.Create(ValidClinicId, ValidName, ValidSecret, ValidEventTypes).Value;

        var dto = registration.ToDto();

        dto.Id.Should().Be(registration.Id);
        dto.Name.Should().Be(ValidName);
        dto.EventTypes.Should().BeEquivalentTo(ValidEventTypes);
        dto.IsActive.Should().BeTrue();
        dto.LastCalledAt.Should().BeNull();
    }
}
