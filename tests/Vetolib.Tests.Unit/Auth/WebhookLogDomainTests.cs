using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class WebhookLogDomainTests
{
    [Fact]
    public void Create_sets_initial_values()
    {
        var clinicId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();

        var log = WebhookLog.Create(clinicId, registrationId, "lab.result", "{\"test\":true}");

        log.ClinicId.Should().Be(clinicId);
        log.WebhookRegistrationId.Should().Be(registrationId);
        log.EventType.Should().Be("lab.result");
        log.Payload.Should().Be("{\"test\":true}");
        log.Status.Should().Be(WebhookLogStatus.Received);
        log.ReceivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        log.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void MarkProcessed_sets_status_and_timestamp()
    {
        var log = WebhookLog.Create(Guid.NewGuid(), Guid.NewGuid(), "lab.result", "{}");

        log.MarkProcessed();

        log.Status.Should().Be(WebhookLogStatus.Processed);
        log.ProcessedAt.Should().NotBeNull();
        log.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkFailed_sets_status_and_timestamp()
    {
        var log = WebhookLog.Create(Guid.NewGuid(), Guid.NewGuid(), "lab.result", "{}");

        log.MarkFailed();

        log.Status.Should().Be(WebhookLogStatus.Failed);
        log.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void ToDto_maps_correctly()
    {
        var log = WebhookLog.Create(Guid.NewGuid(), Guid.NewGuid(), "lab.result", "{}");
        log.MarkProcessed();

        var dto = log.ToDto();

        dto.Id.Should().Be(log.Id);
        dto.WebhookRegistrationId.Should().Be(log.WebhookRegistrationId);
        dto.EventType.Should().Be("lab.result");
        dto.Status.Should().Be("Processed");
        dto.ProcessedAt.Should().NotBeNull();
    }
}
