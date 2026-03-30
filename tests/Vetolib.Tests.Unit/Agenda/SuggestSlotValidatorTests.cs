using FluentAssertions;
using Vetolib.Agenda.Application.Queries.SuggestSlot;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class SuggestSlotValidatorTests
{
    private readonly SuggestSlotValidator _validator = new();

    [Fact]
    public void Valid_query_passes()
    {
        var query = new SuggestSlotQuery("General Checkup", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), null, null);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_consultation_type_fails()
    {
        var query = new SuggestSlotQuery("", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), null, null);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Consultation type is required");
    }

    [Fact]
    public void Past_preferred_date_fails()
    {
        var query = new SuggestSlotQuery("General Checkup", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            new TimeOnly(10, 0), null, null);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Preferred date cannot be in the past");
    }

    [Fact]
    public void Zero_duration_fails()
    {
        var query = new SuggestSlotQuery("General Checkup", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), null, 0);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Duration must be greater than 0");
    }

    [Fact]
    public void Null_duration_passes()
    {
        var query = new SuggestSlotQuery("General Checkup", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), null, null);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
