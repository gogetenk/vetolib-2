using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Stock.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Stock;

/// <summary>
/// Integration tests for Stock endpoints.
/// Tests HTTP contracts, auth/authz, and serialization — not business logic.
/// </summary>
public sealed class StockEndpointsTests : IntegrationTestBase
{
    public StockEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── Helper ────────────────────────────────────────────────────────────

    private static CreateStockItemRequest ValidCreateRequest(string name = "Amoxicillin 250mg") => new(
        Name: name,
        Category: "Medication",
        Quantity: 100,
        Unit: "tablets",
        MinThreshold: 10,
        ExpiryDate: DateTime.UtcNow.AddMonths(6));

    private async Task<StockItemDto> CreateStockItemAsync(HttpClient client, string name = "Amoxicillin 250mg")
    {
        var response = await client.PostAsJsonAsync("/api/v1/stock", ValidCreateRequest(name), JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        dto.Should().NotBeNull();
        return dto!;
    }

    // ── POST /api/v1/stock (Create) ──────────────────────────────────────

    [Fact]
    public async Task CreateStockItem_ValidRequest_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/stock", ValidCreateRequest(), JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Amoxicillin 250mg");
        body.Category.Should().Be("Medication");
        body.Quantity.Should().Be(100);
        body.ClinicId.Should().Be(TestClinicId);
    }

    [Fact]
    public async Task CreateStockItem_Unauthenticated_Returns401()
    {
        // Arrange & Act
        var response = await Client.WithoutAuth()
            .PostAsJsonAsync("/api/v1/stock", ValidCreateRequest(), JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateStockItem_Receptionist_Returns403()
    {
        // Arrange — receptionist does not have VetOrAdmin policy
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.PostAsJsonAsync("/api/v1/stock", ValidCreateRequest(), JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/stock (List) ─────────────────────────────────────────

    [Fact]
    public async Task ListStockItems_Authenticated_Returns200()
    {
        // Arrange — create an item first
        var adminClient = CreateAdminClient();
        await CreateStockItemAsync(adminClient, "Ketamine 50mg/ml");

        // Act
        var response = await adminClient.GetAsync("/api/v1/stock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<StockItemDto>>(JsonOptions);
        items.Should().NotBeNull();
        items.Should().HaveCountGreaterThanOrEqualTo(1);
        items!.Should().Contain(i => i.Name == "Ketamine 50mg/ml");
    }

    [Fact]
    public async Task ListStockItems_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/stock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PATCH /api/v1/stock/{id} (Update) ────────────────────────────────

    [Fact]
    public async Task UpdateStockItem_ValidRequest_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateStockItemAsync(adminClient, "Meloxicam 1.5mg/ml");

        var updateRequest = new UpdateStockItemRequest("Meloxicam 5mg/ml", 20);

        // Act
        var response = await adminClient.PatchAsJsonAsync($"/api/v1/stock/{created.Id}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Meloxicam 5mg/ml");
        updated.MinThreshold.Should().Be(20);
    }

    [Fact]
    public async Task UpdateStockItem_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var updateRequest = new UpdateStockItemRequest("Ghost Item", null);

        // Act
        var response = await adminClient.PatchAsJsonAsync($"/api/v1/stock/{Guid.NewGuid()}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/v1/stock/{id}/movements (RecordMovement) ──────────────

    [Fact]
    public async Task RecordMovement_ValidIntake_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateStockItemAsync(adminClient, "Bandages 10cm");

        var movementRequest = new CreateStockMovementRequest("In", 50, "Supplier delivery");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/stock/{created.Id}/movements", movementRequest, JsonOptions);

        // Assert — handler returns the updated StockItemDto (with new total quantity)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        updatedItem.Should().NotBeNull();
        updatedItem!.Id.Should().Be(created.Id);
        updatedItem.Quantity.Should().Be(150); // initial 100 + 50 intake
    }

    [Fact]
    public async Task RecordMovement_NonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var movementRequest = new CreateStockMovementRequest("In", 10, "Ghost item");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/stock/{Guid.NewGuid()}/movements", movementRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordMovement_Unauthenticated_Returns401()
    {
        // Arrange
        var movementRequest = new CreateStockMovementRequest("In", 10, "Test");

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/stock/{Guid.NewGuid()}/movements", movementRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/stock/alerts (GetAlerts) ─────────────────────────────

    [Fact]
    public async Task GetAlerts_Authenticated_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/stock/alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<StockAlertsDto>(JsonOptions);
        alerts.Should().NotBeNull();
        alerts!.LowStockItems.Should().NotBeNull();
        alerts.ExpiringItems.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAlerts_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/stock/alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
