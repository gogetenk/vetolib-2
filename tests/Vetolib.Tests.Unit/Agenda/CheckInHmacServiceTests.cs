using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Vetolib.Agenda.Application.Services;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CheckInHmacServiceTests
{
    private static readonly Guid AppointmentId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private const string PatientName = "Luna";
    private const string OwnerName = "Ahmed Al-Rashidi";
    private static readonly DateTime ScheduledTime = new(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

    private static CheckInHmacService CreateService(string key = "test-hmac-key-at-least-32-chars-long!")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CheckIn:HmacKey"] = key
            })
            .Build();

        return new CheckInHmacService(config);
    }

    // ── Signature Generation ──────────────────────────────────────────────────

    [Fact]
    public void ComputeSignature_ReturnsDeterministicResult()
    {
        var svc = CreateService();

        var sig1 = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);
        var sig2 = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        sig1.Should().Be(sig2);
    }

    [Fact]
    public void ComputeSignature_DifferentKeys_ProduceDifferentSignatures()
    {
        var svc1 = CreateService("key-one-that-is-long-enough-for-test");
        var svc2 = CreateService("key-two-that-is-long-enough-for-test");

        var sig1 = svc1.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);
        var sig2 = svc2.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void ComputeSignature_DifferentAppointmentId_ProducesDifferentSignature()
    {
        var svc = CreateService();

        var sig1 = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);
        var sig2 = svc.ComputeSignature(Guid.NewGuid(), PatientName, OwnerName, ScheduledTime, ClinicId);

        sig1.Should().NotBe(sig2);
    }

    // ── Signature Verification ────────────────────────────────────────────────

    [Fact]
    public void VerifyAndValidateTimeWindow_ValidSignature_WithinWindow_ReturnsSuccess()
    {
        var svc = CreateService();
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId,
            sig, ScheduledTime); // exact time

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void VerifyAndValidateTimeWindow_ValidSignature_29MinBefore_ReturnsSuccess()
    {
        var svc = CreateService();
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId,
            sig, ScheduledTime.AddMinutes(-29));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void VerifyAndValidateTimeWindow_ValidSignature_30MinAfter_ReturnsSuccess()
    {
        var svc = CreateService();
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId,
            sig, ScheduledTime.AddMinutes(30));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void VerifyAndValidateTimeWindow_ValidSignature_31MinBefore_ReturnsError()
    {
        var svc = CreateService();
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId,
            sig, ScheduledTime.AddMinutes(-31));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CHECKIN_WINDOW_EXPIRED"));
    }

    [Fact]
    public void VerifyAndValidateTimeWindow_ValidSignature_31MinAfter_ReturnsError()
    {
        var svc = CreateService();
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId,
            sig, ScheduledTime.AddMinutes(31));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CHECKIN_WINDOW_EXPIRED"));
    }

    [Fact]
    public void VerifyAndValidateTimeWindow_InvalidSignature_ReturnsError()
    {
        var svc = CreateService();

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId,
            "tampered-signature", ScheduledTime);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_SIGNATURE"));
    }

    [Fact]
    public void VerifyAndValidateTimeWindow_TamperedPatientName_ReturnsError()
    {
        var svc = CreateService();
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        var result = svc.VerifyAndValidateTimeWindow(
            AppointmentId, "TamperedName", OwnerName, ScheduledTime, ClinicId,
            sig, ScheduledTime);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_SIGNATURE"));
    }

    // ── Configuration ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_FallsBackToJwtKey_WhenCheckInHmacKeyNotConfigured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "fallback-jwt-key-that-is-long-enough"
            })
            .Build();

        var svc = new CheckInHmacService(config);
        var sig = svc.ComputeSignature(AppointmentId, PatientName, OwnerName, ScheduledTime, ClinicId);

        sig.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ThrowsWhenNoKeyConfigured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var act = () => new CheckInHmacService(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CheckIn:HmacKey*Jwt:Key*");
    }
}
