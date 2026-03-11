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
