using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Queries.SearchClinics;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class SearchClinicsHandlerTests
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

    private static Clinic CreateClinic(string name, string? city = null, string[]? species = null)
    {
        var result = Clinic.Create(name);
        result.IsSuccess.Should().BeTrue();
        var clinic = result.Value;
        if (city is not null || species is not null)
        {
            clinic.UpdateDirectory(city, null, species);
        }
        return clinic;
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllClinics()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.AddRange(
            CreateClinic("Desert Paws"),
            CreateClinic("Al Barsha Hospital"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery(null, null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NameFilter_PartialMatchCaseInsensitive()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.AddRange(
            CreateClinic("Desert Paws Vet Clinic"),
            CreateClinic("Al Barsha Pet Hospital"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery("desert", null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Desert Paws Vet Clinic");
    }

    [Fact]
    public async Task Handle_CityFilter_ExactMatchCaseInsensitive()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.AddRange(
            CreateClinic("Clinic A", "Dubai"),
            CreateClinic("Clinic B", "Abu Dhabi"),
            CreateClinic("Clinic C", "Dubai"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery(null, "dubai", null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.AddRange(
            CreateClinic("A Clinic"),
            CreateClinic("B Clinic"),
            CreateClinic("C Clinic"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery(null, null, null, Page: 2, PageSize: 2), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Handle_NoResults_ReturnsEmptyList()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.Add(CreateClinic("Desert Paws"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery("NonExistent", null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PageSizeExceeds100_ClampedTo100()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.Add(CreateClinic("Clinic A"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery(null, null, null, PageSize: 500), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task Handle_InvalidPage_DefaultsTo1()
    {
        // Arrange
        using var context = BuildContext();
        context.Clinics.Add(CreateClinic("Clinic A"));
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery(null, null, null, Page: -1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ResultIncludesSlugAndLogoUrl()
    {
        // Arrange
        using var context = BuildContext();
        var clinic = CreateClinic("Desert Paws Vet");
        clinic.UpdateDirectory("Dubai", "https://example.com/logo.png", ["Dog", "Cat"]);
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var handler = new SearchClinicsHandler(context);

        // Act
        var result = await handler.Handle(new SearchClinicsQuery(null, null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = result.Value.Items[0];
        item.Slug.Should().Be("desert-paws-vet");
        item.LogoUrl.Should().Be("https://example.com/logo.png");
        item.City.Should().Be("Dubai");
    }
}
