using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class AppointmentCancelRaisesSlotAvailableEventTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly StartTime = new(10, 0);

    [Fact]
    public void Cancel_WhenScheduled_RaisesSlotAvailableEvent()
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid",
            AnimalId, "Luna", "Ahmed", null,
            FutureDate, StartTime, 30, "Checkup").Value;

        appointment.Cancel("Owner request");

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SlotAvailableEvent>()
            .Which.Should().Match<SlotAvailableEvent>(e =>
                e.ClinicId == ClinicId &&
                e.VeterinarianId == VetId &&
                e.Date == FutureDate &&
                e.StartTime == StartTime &&
                e.DurationMinutes == 30);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_DoesNotRaiseEvent()
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid",
            AnimalId, "Luna", "Ahmed", null,
            FutureDate, StartTime, 30, "Checkup").Value;

        appointment.Cancel(); // first cancel
        appointment.ClearDomainEvents();

        var result = appointment.Cancel(); // second cancel attempt

        result.IsSuccess.Should().BeFalse();
        appointment.DomainEvents.Should().BeEmpty();
    }
}
