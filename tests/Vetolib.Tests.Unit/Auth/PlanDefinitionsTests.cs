using FluentAssertions;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class PlanDefinitionsTests
{
    [Fact]
    public void Free_Returns_CorrectLimits()
    {
        var limits = PlanDefinitions.GetLimits(SubscriptionPlan.Free);

        limits.MaxVets.Should().Be(1);
        limits.MaxPatients.Should().Be(50);
        limits.MaxWhatsAppMessagesPerMonth.Should().Be(0);
        limits.MaxStorageGB.Should().Be(1);
        limits.HasAiTriage.Should().BeFalse();
        limits.HasWhatsApp.Should().BeFalse();
        limits.HasMultiClinic.Should().BeFalse();
        limits.HasApi.Should().BeFalse();
    }

    [Fact]
    public void Starter_Returns_CorrectLimits()
    {
        var limits = PlanDefinitions.GetLimits(SubscriptionPlan.Starter);

        limits.MaxVets.Should().Be(3);
        limits.MaxPatients.Should().Be(500);
        limits.MaxWhatsAppMessagesPerMonth.Should().Be(50);
        limits.MaxStorageGB.Should().Be(10);
        limits.HasAiTriage.Should().BeTrue();
        limits.HasWhatsApp.Should().BeTrue();
        limits.HasMultiClinic.Should().BeFalse();
        limits.HasApi.Should().BeFalse();
    }

    [Fact]
    public void Pro_Returns_UnlimitedVetsAndPatients()
    {
        var limits = PlanDefinitions.GetLimits(SubscriptionPlan.Pro);

        limits.MaxVets.Should().Be(int.MaxValue);
        limits.MaxPatients.Should().Be(int.MaxValue);
        limits.MaxWhatsAppMessagesPerMonth.Should().Be(int.MaxValue);
        limits.MaxStorageGB.Should().Be(50);
        limits.HasAiTriage.Should().BeTrue();
        limits.HasWhatsApp.Should().BeTrue();
        limits.HasMultiClinic.Should().BeFalse();
        limits.HasApi.Should().BeFalse();
    }

    [Fact]
    public void Enterprise_Returns_AllFeaturesEnabled()
    {
        var limits = PlanDefinitions.GetLimits(SubscriptionPlan.Enterprise);

        limits.MaxVets.Should().Be(int.MaxValue);
        limits.MaxPatients.Should().Be(int.MaxValue);
        limits.MaxWhatsAppMessagesPerMonth.Should().Be(int.MaxValue);
        limits.MaxStorageGB.Should().Be(int.MaxValue);
        limits.HasAiTriage.Should().BeTrue();
        limits.HasWhatsApp.Should().BeTrue();
        limits.HasMultiClinic.Should().BeTrue();
        limits.HasApi.Should().BeTrue();
    }

    [Theory]
    [InlineData(SubscriptionPlan.Free)]
    [InlineData(SubscriptionPlan.Starter)]
    [InlineData(SubscriptionPlan.Pro)]
    [InlineData(SubscriptionPlan.Enterprise)]
    public void AllPlans_AreDefinedAndReturnable(SubscriptionPlan plan)
    {
        var limits = PlanDefinitions.GetLimits(plan);

        limits.Should().NotBeNull();
        limits.MaxVets.Should().BeGreaterThan(0);
        limits.MaxPatients.Should().BeGreaterThan(0);
    }
}
