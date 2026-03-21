using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class SubscriptionCheckerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private async Task<Clinic> SeedClinicAsync(AuthDbContext context, Vetolib.Auth.Contracts.SubscriptionPlan plan)
    {
        var clinicResult = Clinic.Create("Desert Paws Clinic");
        clinicResult.IsSuccess.Should().BeTrue();
        var clinic = clinicResult.Value;

        // Use reflection to set SubscriptionPlan and Id since they are private setters
        typeof(Clinic).GetProperty(nameof(Clinic.SubscriptionPlan))!
            .SetValue(clinic, plan);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!
            .SetValue(clinic, FixedClinicId);

        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();
        return clinic;
    }

    private async Task SeedVetsAsync(AuthDbContext context, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var userResult = User.Create(FixedClinicId, $"vet{i}@clinic.ae", "Password1!", UserRole.Vet, $"LIC-{i:D4}");
            userResult.IsSuccess.Should().BeTrue();
            context.Users.Add(userResult.Value);
        }
        await context.SaveChangesAsync();
    }

    // ─── CheckLimitAsync ──────────────────────────────────────────────

    [Fact]
    public async Task CheckLimit_ClinicNotFound_ReturnsNotFound()
    {
        using var context = BuildContext();
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(Guid.NewGuid(), LimitType.Vets);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task CheckLimit_FreePlan_UnderVetLimit_ReturnsSuccess()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Free);
        // No vets seeded — 0 < 1 limit
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Vets);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckLimit_FreePlan_AtVetLimit_ReturnsError()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Free);
        await SeedVetsAsync(context, 1); // 1 vet = at limit for Free plan
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Vets);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("upgrade"));
    }

    [Fact]
    public async Task CheckLimit_StarterPlan_UnderVetLimit_ReturnsSuccess()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Starter);
        await SeedVetsAsync(context, 2); // 2 < 3 limit for Starter
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Vets);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckLimit_StarterPlan_AtVetLimit_ReturnsError()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Starter);
        await SeedVetsAsync(context, 3); // 3 = at limit for Starter
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Vets);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("3"));
    }

    [Fact]
    public async Task CheckLimit_ProPlan_UnlimitedVets_ReturnsSuccess()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Pro);
        await SeedVetsAsync(context, 10); // Pro is unlimited
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Vets);

        result.IsSuccess.Should().BeTrue();
    }

    // ─── Boolean feature limits ──────────────────────────────────────

    [Fact]
    public async Task CheckLimit_FreePlan_AiTriage_ReturnsError()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Free);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.AiTriage);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("AI Triage"));
    }

    [Fact]
    public async Task CheckLimit_StarterPlan_AiTriage_ReturnsSuccess()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Starter);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.AiTriage);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckLimit_FreePlan_WhatsApp_ReturnsError()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Free);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.WhatsApp);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("WhatsApp"));
    }

    [Fact]
    public async Task CheckLimit_ProPlan_MultiClinic_ReturnsError()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Pro);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.MultiClinic);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Enterprise"));
    }

    [Fact]
    public async Task CheckLimit_EnterprisePlan_MultiClinic_ReturnsSuccess()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Enterprise);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.MultiClinic);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckLimit_ProPlan_Api_ReturnsError()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Pro);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Api);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Enterprise"));
    }

    [Fact]
    public async Task CheckLimit_EnterprisePlan_Api_ReturnsSuccess()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Enterprise);
        var checker = new SubscriptionChecker(context);

        var result = await checker.CheckLimitAsync(FixedClinicId, LimitType.Api);

        result.IsSuccess.Should().BeTrue();
    }

    // ─── GetCurrentUsageAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUsage_ClinicNotFound_ReturnsNotFound()
    {
        using var context = BuildContext();
        var checker = new SubscriptionChecker(context);

        var result = await checker.GetCurrentUsageAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task GetCurrentUsage_ReturnsCorrectPlanAndLimits()
    {
        using var context = BuildContext();
        await SeedClinicAsync(context, Vetolib.Auth.Contracts.SubscriptionPlan.Starter);
        await SeedVetsAsync(context, 2);
        var checker = new SubscriptionChecker(context);

        var result = await checker.GetCurrentUsageAsync(FixedClinicId);

        result.IsSuccess.Should().BeTrue();
        var usage = result.Value;
        usage.Plan.Should().Be(Vetolib.Auth.Contracts.SubscriptionPlan.Starter);
        usage.CurrentVets.Should().Be(2);
        usage.MaxVets.Should().Be(3);
        usage.MaxPatients.Should().Be(500);
        usage.HasAiTriage.Should().BeTrue();
        usage.HasWhatsApp.Should().BeTrue();
        usage.HasMultiClinic.Should().BeFalse();
        usage.HasApi.Should().BeFalse();
    }
}
