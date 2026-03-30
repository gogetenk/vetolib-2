using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class RecurrenceRuleTests
{
    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidInput_ReturnsSuccess()
    {
        var result = RecurrenceRule.Create(RecurrenceFrequency.Weekly, 4);

        result.IsSuccess.Should().BeTrue();
        result.Value.Frequency.Should().Be(RecurrenceFrequency.Weekly);
        result.Value.Count.Should().Be(4);
    }

    [Fact]
    public void Create_WithCountBelow2_ReturnsInvalid()
    {
        var result = RecurrenceRule.Create(RecurrenceFrequency.Weekly, 1);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Create_WithCountAbove52_ReturnsInvalid()
    {
        var result = RecurrenceRule.Create(RecurrenceFrequency.Monthly, 53);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Create_WithCount52_ReturnsSuccess()
    {
        var result = RecurrenceRule.Create(RecurrenceFrequency.Daily, 52);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithCount2_ReturnsSuccess()
    {
        var result = RecurrenceRule.Create(RecurrenceFrequency.Weekly, 2);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Date Generation: Daily ────────────────────────────────────────────────

    [Fact]
    public void GenerateDates_Daily_ReturnsConsecutiveDays()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Daily, 5).Value;
        var startDate = new DateOnly(2026, 4, 1);

        var dates = rule.GenerateDates(startDate);

        dates.Should().HaveCount(5);
        dates[0].Should().Be(new DateOnly(2026, 4, 1));
        dates[1].Should().Be(new DateOnly(2026, 4, 2));
        dates[2].Should().Be(new DateOnly(2026, 4, 3));
        dates[3].Should().Be(new DateOnly(2026, 4, 4));
        dates[4].Should().Be(new DateOnly(2026, 4, 5));
    }

    // ── Date Generation: Weekly ───────────────────────────────────────────────

    [Fact]
    public void GenerateDates_Weekly_Returns7DayIntervals()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Weekly, 4).Value;
        var startDate = new DateOnly(2026, 4, 1);

        var dates = rule.GenerateDates(startDate);

        dates.Should().HaveCount(4);
        dates[0].Should().Be(new DateOnly(2026, 4, 1));
        dates[1].Should().Be(new DateOnly(2026, 4, 8));
        dates[2].Should().Be(new DateOnly(2026, 4, 15));
        dates[3].Should().Be(new DateOnly(2026, 4, 22));
    }

    // ── Date Generation: Biweekly ─────────────────────────────────────────────

    [Fact]
    public void GenerateDates_Biweekly_Returns14DayIntervals()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Biweekly, 3).Value;
        var startDate = new DateOnly(2026, 4, 1);

        var dates = rule.GenerateDates(startDate);

        dates.Should().HaveCount(3);
        dates[0].Should().Be(new DateOnly(2026, 4, 1));
        dates[1].Should().Be(new DateOnly(2026, 4, 15));
        dates[2].Should().Be(new DateOnly(2026, 4, 29));
    }

    // ── Date Generation: Monthly ──────────────────────────────────────────────

    [Fact]
    public void GenerateDates_Monthly_ReturnsMonthlyIntervals()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly, 4).Value;
        var startDate = new DateOnly(2026, 1, 15);

        var dates = rule.GenerateDates(startDate);

        dates.Should().HaveCount(4);
        dates[0].Should().Be(new DateOnly(2026, 1, 15));
        dates[1].Should().Be(new DateOnly(2026, 2, 15));
        dates[2].Should().Be(new DateOnly(2026, 3, 15));
        dates[3].Should().Be(new DateOnly(2026, 4, 15));
    }

    [Fact]
    public void GenerateDates_Monthly_HandlesEndOfMonthCorrectly()
    {
        // January 31 -> Feb 28 (non-leap year), March 31, April 30
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly, 4).Value;
        var startDate = new DateOnly(2027, 1, 31);

        var dates = rule.GenerateDates(startDate);

        dates.Should().HaveCount(4);
        dates[0].Should().Be(new DateOnly(2027, 1, 31));
        dates[1].Should().Be(new DateOnly(2027, 2, 28)); // Feb has 28 days in 2027
        dates[2].Should().Be(new DateOnly(2027, 3, 31));
        dates[3].Should().Be(new DateOnly(2027, 4, 30)); // April has 30 days
    }

    // ── First date is always the start date ───────────────────────────────────

    [Theory]
    [InlineData(RecurrenceFrequency.Daily)]
    [InlineData(RecurrenceFrequency.Weekly)]
    [InlineData(RecurrenceFrequency.Biweekly)]
    [InlineData(RecurrenceFrequency.Monthly)]
    public void GenerateDates_FirstDateIsAlwaysStartDate(RecurrenceFrequency frequency)
    {
        var rule = RecurrenceRule.Create(frequency, 3).Value;
        var startDate = new DateOnly(2026, 6, 15);

        var dates = rule.GenerateDates(startDate);

        dates[0].Should().Be(startDate);
    }
}
