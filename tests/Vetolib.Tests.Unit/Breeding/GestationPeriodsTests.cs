using FluentAssertions;
using Vetolib.Breeding.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class GestationPeriodsTests
{
    [Theory]
    [InlineData("Dog", 63)]
    [InlineData("dog", 63)]
    [InlineData("DOG", 63)]
    [InlineData("Cat", 65)]
    [InlineData("Horse", 340)]
    [InlineData("Camel", 390)]
    [InlineData("Falcon", 32)]
    [InlineData("Rabbit", 31)]
    public void GetDays_KnownSpecies_ReturnsCorrectPeriod(string species, int expected)
    {
        GestationPeriods.GetDays(species).Should().Be(expected);
    }

    [Theory]
    [InlineData("Lizard")]
    [InlineData("Turtle")]
    [InlineData("UnknownSpecies")]
    public void GetDays_UnknownSpecies_ReturnsDefault60(string species)
    {
        GestationPeriods.GetDays(species).Should().Be(60);
    }
}
