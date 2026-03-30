using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class WaitlistEntryDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("44444444-4444-4444-4444-444444444444");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private static Result<WaitlistEntry> CreateEntry(
        Guid? clinicId = null,
        Guid? patientId = null,
        string ownerName = "Ahmed Al-Rashidi",
        string ownerPhone = "+971501234567",
        string? ownerEmail = "ahmed@email.ae",
        DateOnly? preferredDate = null,
        PreferredTimeSlot timeSlot = PreferredTimeSlot.Morning,
        Guid? vetPreference = null,
        string? reason = "Vaccination follow-up")
        => WaitlistEntry.Create(
            clinicId ?? ClinicId,
            patientId ?? PatientId,
            ownerName,
            ownerPhone,
            ownerEmail,
            preferredDate ?? FutureDate,
            timeSlot,
            vetPreference,
            reason);

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = CreateEntry();

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WaitlistEntryStatus.Pending);
        result.Value.OwnerName.Should().Be("Ahmed Al-Rashidi");
        result.Value.OwnerPhone.Should().Be("+971501234567");
        result.Value.PreferredTimeSlot.Should().Be(PreferredTimeSlot.Morning);
        result.Value.NotifiedAt.Should().BeNull();
        result.Value.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = CreateEntry(clinicId: Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyPatientId_ReturnsInvalid()
    {
        var result = CreateEntry(patientId: Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "patientId");
    }

    [Fact]
    public void Create_WithEmptyOwnerName_ReturnsInvalid()
    {
        var result = CreateEntry(ownerName: "");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "ownerName");
    }

    [Fact]
    public void Create_WithEmptyOwnerPhone_ReturnsInvalid()
    {
        var result = CreateEntry(ownerPhone: "");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "ownerPhone");
    }

    [Fact]
    public void Create_WithMultipleErrors_ReturnsAllErrors()
    {
        var result = CreateEntry(clinicId: Guid.Empty, patientId: Guid.Empty, ownerName: "", ownerPhone: "");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCount(4);
    }

    // ── Notify ──────────────────────────────────────────────────────────────

    [Fact]
    public void Notify_WhenPending_ReturnsSuccessAndSetsExpiresAt()
    {
        var entry = CreateEntry().Value;

        var result = entry.Notify();

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Notified);
        entry.NotifiedAt.Should().NotBeNull();
        entry.ExpiresAt.Should().NotBeNull();
        entry.ExpiresAt!.Value.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Notify_WhenAlreadyNotified_ReturnsError()
    {
        var entry = CreateEntry().Value;
        entry.Notify();

        var result = entry.Notify();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    // ── MarkBooked ──────────────────────────────────────────────────────────

    [Fact]
    public void MarkBooked_WhenNotified_ReturnsSuccess()
    {
        var entry = CreateEntry().Value;
        entry.Notify();

        var result = entry.MarkBooked();

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Booked);
    }

    [Fact]
    public void MarkBooked_WhenPending_ReturnsError()
    {
        var entry = CreateEntry().Value;

        var result = entry.MarkBooked();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    // ── Expire ──────────────────────────────────────────────────────────────

    [Fact]
    public void Expire_WhenNotified_ReturnsSuccess()
    {
        var entry = CreateEntry().Value;
        entry.Notify();

        var result = entry.Expire();

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Expired);
    }

    [Fact]
    public void Expire_WhenPending_ReturnsError()
    {
        var entry = CreateEntry().Value;

        var result = entry.Expire();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    // ── ToDto ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var entry = CreateEntry().Value;
        var dto = entry.ToDto();

        dto.Id.Should().Be(entry.Id);
        dto.ClinicId.Should().Be(ClinicId);
        dto.PatientId.Should().Be(PatientId);
        dto.OwnerName.Should().Be("Ahmed Al-Rashidi");
        dto.OwnerPhone.Should().Be("+971501234567");
        dto.OwnerEmail.Should().Be("ahmed@email.ae");
        dto.PreferredDate.Should().Be(FutureDate);
        dto.PreferredTimeSlot.Should().Be(PreferredTimeSlot.Morning);
        dto.Reason.Should().Be("Vaccination follow-up");
        dto.Status.Should().Be(WaitlistEntryStatus.Pending);
    }
}
