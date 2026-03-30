using FluentAssertions;
using Vetolib.Agenda.Application.Commands.CreateAppointment;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateAppointmentValidatorTests
{
    private readonly CreateAppointmentValidator _validator = new();

    private static CreateAppointmentCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), "Dr. Ahmed", Guid.NewGuid(),
        "Buddy", "Mohammed Al Rashid",
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
        new TimeOnly(10, 0), 30, "Annual checkup");

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = ValidCommand() with { ClinicId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void Empty_veterinarian_id_fails()
    {
        var cmd = ValidCommand() with { VeterinarianId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VeterinarianId");
    }

    [Fact]
    public void Empty_veterinarian_name_fails()
    {
        var cmd = ValidCommand() with { VeterinarianName = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VeterinarianName");
    }

    [Fact]
    public void Empty_animal_id_fails()
    {
        var cmd = ValidCommand() with { AnimalId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AnimalId");
    }

    [Fact]
    public void Empty_animal_name_fails()
    {
        var cmd = ValidCommand() with { AnimalName = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AnimalName");
    }

    [Fact]
    public void Empty_owner_name_fails()
    {
        var cmd = ValidCommand() with { OwnerName = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OwnerName");
    }

    [Fact]
    public void Zero_duration_fails()
    {
        var cmd = ValidCommand() with { DurationMinutes = 0 };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationMinutes");
    }

    [Fact]
    public void Negative_duration_fails()
    {
        var cmd = ValidCommand() with { DurationMinutes = -5 };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationMinutes");
    }
}
