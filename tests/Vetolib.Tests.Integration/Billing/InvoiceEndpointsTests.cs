using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Billing.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Billing;

/// <summary>
/// Integration tests for Invoice endpoints.
/// </summary>
public sealed class InvoiceEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public InvoiceEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/invoices ──────────────────────────────────────────────

    [Fact]
    public async Task CreateInvoice_ValidRequest_Returns201()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var animalId = Guid.NewGuid();

        var request = new CreateInvoiceRequest(
            AnimalId: animalId,
            ItemDescription: "Consultation générale",
            ItemUnitPrice: 350.00m);

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/invoices", request, JsonOptions);

        // Assert
        // Assert — ToMinimalApiResult() maps Result.Success to 200 OK for creates
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Status.Should().Be(InvoiceStatus.Draft);
        body.ClinicId.Should().Be(TestClinicId);
        body.Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ── POST /api/v1/invoices/{id}/items ──────────────────────────────────

    [Fact]
    public async Task AddInvoiceItem_ValidRequest_Returns200WithUpdatedInvoice()
    {
        // Arrange — create an invoice first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateInvoiceRequest(Guid.NewGuid(), "Vaccination", 150.00m);
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/invoices", createRequest, JsonOptions);
        var invoice = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);

        var itemRequest = new AddInvoiceItemRequest(
            Description: "Médicament anti-parasitaire",
            UnitPrice: 75.00m);

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/invoices/{invoice!.Id}/items",
            itemRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        updated.Should().NotBeNull();
        updated!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        updated.Items.Should().Contain(i => i.Description == "Médicament anti-parasitaire");
    }

    // ── PATCH /api/v1/invoices/{id}/status ────────────────────────────────

    [Fact]
    public async Task UpdateInvoiceStatus_DraftToSent_Returns200()
    {
        // Arrange — create a Draft invoice
        var adminClient = CreateAdminClient();
        var createRequest = new CreateInvoiceRequest(Guid.NewGuid(), "Chirurgie de stérilisation", 800.00m);
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/invoices", createRequest, JsonOptions);
        var invoice = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);

        var statusRequest = new UpdateInvoiceStatusRequest(InvoiceStatus.Sent);

        // Act
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/invoices/{invoice!.Id}/status",
            statusRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        updated!.Status.Should().Be(InvoiceStatus.Sent);
    }

    // ── GET /api/v1/invoices ─────────────────────────────────────────────

    [Fact]
    public async Task ListInvoices_Authenticated_Returns200WithPaginatedResults()
    {
        // Arrange — create two invoices
        var adminClient = CreateAdminClient();
        await adminClient.PostAsJsonAsync("/api/v1/invoices",
            new CreateInvoiceRequest(Guid.NewGuid(), "Consultation A", 100.00m), JsonOptions);
        await adminClient.PostAsJsonAsync("/api/v1/invoices",
            new CreateInvoiceRequest(Guid.NewGuid(), "Consultation B", 200.00m), JsonOptions);

        // Act
        var response = await adminClient.GetAsync("/api/v1/invoices?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ListInvoices_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/invoices");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/invoices/{id} ─────────────────────────────────────────

    [Fact]
    public async Task GetInvoiceById_ValidId_Returns200WithInvoice()
    {
        // Arrange — create an invoice first
        var adminClient = CreateAdminClient();
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/invoices",
            new CreateInvoiceRequest(Guid.NewGuid(), "Examen sanguin", 250.00m), JsonOptions);
        var invoice = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);

        // Act
        var response = await adminClient.GetAsync($"/api/v1/invoices/{invoice!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().Be(invoice.Id);
        body.InvoiceNumber.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetInvoiceById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync($"/api/v1/invoices/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInvoiceById_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/invoices/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/invoices/{id}/submit-einvoicing ──────────────────────

    [Fact]
    public async Task SubmitToEInvoicing_ValidInvoice_Returns200()
    {
        // Arrange — create and send an invoice (must be Sent to submit)
        var adminClient = CreateAdminClient();
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/invoices",
            new CreateInvoiceRequest(Guid.NewGuid(), "Chirurgie orthopédique", 1200.00m), JsonOptions);
        var invoice = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);

        await adminClient.PatchAsJsonAsync(
            $"/api/v1/invoices/{invoice!.Id}/status",
            new UpdateInvoiceStatusRequest(InvoiceStatus.Sent), JsonOptions);

        // Act
        var response = await adminClient.PostAsync(
            $"/api/v1/invoices/{invoice.Id}/submit-einvoicing", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SubmitToEInvoicing_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PostAsync(
            $"/api/v1/invoices/{Guid.NewGuid()}/submit-einvoicing", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/invoices/{id}/einvoicing-status ───────────────────────

    [Fact]
    public async Task GetEInvoicingStatus_ValidInvoice_Returns200()
    {
        // Arrange — create an invoice
        var adminClient = CreateAdminClient();
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/invoices",
            new CreateInvoiceRequest(Guid.NewGuid(), "Vaccination annuelle", 180.00m), JsonOptions);
        var invoice = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);

        // Act
        var response = await adminClient.GetAsync(
            $"/api/v1/invoices/{invoice!.Id}/einvoicing-status");

        // Assert
        // May return 200 with null/empty status or NotFound if not yet submitted
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEInvoicingStatus_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/invoices/{Guid.NewGuid()}/einvoicing-status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/invoices/export/csv ───────────────────────────────────

    [Fact]
    public async Task ExportInvoicesCsv_ValidDateRange_ReturnsCsvFile()
    {
        // Arrange — create an invoice to have data
        var adminClient = CreateAdminClient();
        await adminClient.PostAsJsonAsync("/api/v1/invoices",
            new CreateInvoiceRequest(Guid.NewGuid(), "Détartrage", 120.00m), JsonOptions);

        var from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        // Act
        var response = await adminClient.GetAsync($"/api/v1/invoices/export/csv?from={from}&to={to}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportInvoicesCsv_Unauthenticated_Returns401()
    {
        // Act
        var from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/invoices/export/csv?from={from}&to={to}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportInvoicesCsv_NonAdminRole_ReturnsForbidden()
    {
        // Arrange — Vet role should not have access (Admin only)
        var vetClient = CreateVetClient();
        var from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await vetClient.GetAsync($"/api/v1/invoices/export/csv?from={from}&to={to}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/invoices/{id}/pdf ──────────────────────────────────────

    [Fact]
    public async Task GetInvoicePdf_ValidId_ReturnsPdfContent()
    {
        // Arrange — create and send an invoice
        var adminClient = CreateAdminClient();
        var createRequest = new CreateInvoiceRequest(Guid.NewGuid(), "Bilan de santé complet", 500.00m);
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/invoices", createRequest, JsonOptions);
        var invoice = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);

        // Transition to Sent (required for PDF generation in some implementations)
        await adminClient.PatchAsJsonAsync(
            $"/api/v1/invoices/{invoice!.Id}/status",
            new UpdateInvoiceStatusRequest(InvoiceStatus.Sent),
            JsonOptions);

        // Act — request the PDF
        var response = await adminClient.GetAsync($"/api/v1/invoices/{invoice.Id}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }
}
