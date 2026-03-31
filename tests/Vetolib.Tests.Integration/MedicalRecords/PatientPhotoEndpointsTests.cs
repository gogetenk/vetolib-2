using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Patient photo endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class PatientPhotoEndpointsTests : IntegrationTestBase
{
    public PatientPhotoEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/patients/{id}/photo ──────────────────────────────────

    [Fact]
    public async Task UploadPhoto_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        // Create a minimal valid image-like content
        var content = new MultipartFormDataContent();
        var fileBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG magic bytes
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "photo.jpg");

        var response = await vetClient.PostAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/photo", content);

        // Non-existent patient → business error, not 5xx
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadPhoto_Unauthenticated_Returns401()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "photo.jpg");

        var response = await Client.WithoutAuth().PostAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/photo", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/patients/{id}/photo ───────────────────────────────────

    [Fact]
    public async Task GetPhoto_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/photo");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetPhoto_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/photo");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
