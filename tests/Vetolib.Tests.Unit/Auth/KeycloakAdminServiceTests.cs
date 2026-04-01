using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Vetolib.Auth.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class KeycloakAdminServiceTests
{
    private static readonly Guid FixedUserId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid FixedOrgId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid FixedClinicId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private readonly ILogger<KeycloakAdminService> _logger = Substitute.For<ILogger<KeycloakAdminService>>();

    private readonly KeycloakAdminOptions _options = new()
    {
        Realm = "vetolib",
        ClientId = "vetolib-api",
        ClientSecret = "test-secret"
    };

    private (KeycloakAdminService Service, MockHttpMessageHandler Handler) BuildService()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        var options = Options.Create(_options);
        var service = new KeycloakAdminService(httpClient, options, _logger);
        return (service, handler);
    }

    // --- CreateUserAsync ---

    [Fact]
    public async Task CreateUserAsync_Success_ReturnsUserId()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.Created,
                Headers: new Dictionary<string, string>
                {
                    ["Location"] = $"http://localhost:8080/admin/realms/vetolib/users/{FixedUserId}"
                }));

        var result = await service.CreateUserAsync("vet@desertpaws.ae", "Pass1234!", "Ahmed", "Al Maktoum");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(FixedUserId);
    }

    [Fact]
    public async Task CreateUserAsync_Conflict_ReturnsConflict()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.Conflict));

        var result = await service.CreateUserAsync("vet@desertpaws.ae", "Pass1234!", "Ahmed", "Al Maktoum");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task CreateUserAsync_AuthFails_ReturnsError()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            new MockResponse(HttpStatusCode.Unauthorized, Content: "Invalid credentials"));

        var result = await service.CreateUserAsync("vet@desertpaws.ae", "Pass1234!", "Ahmed", "Al Maktoum");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    // --- CreateOrganizationAsync ---

    [Fact]
    public async Task CreateOrganizationAsync_Success_ReturnsOrgId()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.Created,
                Headers: new Dictionary<string, string>
                {
                    ["Location"] = $"http://localhost:8080/admin/realms/vetolib/organizations/{FixedOrgId}"
                }));

        var result = await service.CreateOrganizationAsync("Desert Paws Clinic", FixedClinicId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(FixedOrgId);
    }

    [Fact]
    public async Task CreateOrganizationAsync_Conflict_ReturnsConflict()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.Conflict));

        var result = await service.CreateOrganizationAsync("Desert Paws Clinic", FixedClinicId);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    // --- DeactivateUserAsync ---

    [Fact]
    public async Task DeactivateUserAsync_Success_ReturnsSuccess()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.NoContent));

        var result = await service.DeactivateUserAsync(FixedUserId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUserAsync_NotFound_ReturnsNotFound()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.NotFound));

        var result = await service.DeactivateUserAsync(FixedUserId);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // --- RemoveUserFromOrganizationAsync ---

    [Fact]
    public async Task RemoveUserFromOrganizationAsync_NotFound_ReturnsNotFound()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.NotFound));

        var result = await service.RemoveUserFromOrganizationAsync(FixedUserId, FixedOrgId);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task RemoveUserFromOrganizationAsync_Success_ReturnsSuccess()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.NoContent));

        var result = await service.RemoveUserFromOrganizationAsync(FixedUserId, FixedOrgId);

        result.IsSuccess.Should().BeTrue();
    }

    // --- ListUserOrganizationsAsync ---

    [Fact]
    public async Task ListUserOrganizationsAsync_Success_ReturnsDtos()
    {
        var orgsJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                id = FixedOrgId,
                name = "Desert Paws Clinic",
                attributes = new Dictionary<string, string[]>
                {
                    ["clinicId"] = new[] { FixedClinicId.ToString() }
                }
            }
        });

        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.OK, Content: orgsJson));

        var result = await service.ListUserOrganizationsAsync(FixedUserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(FixedOrgId);
        result.Value[0].Name.Should().Be("Desert Paws Clinic");
        result.Value[0].ClinicId.Should().Be(FixedClinicId);
    }

    [Fact]
    public async Task ListUserOrganizationsAsync_UserNotFound_ReturnsNotFound()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.NotFound));

        var result = await service.ListUserOrganizationsAsync(FixedUserId);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // --- Token caching ---

    [Fact]
    public async Task TokenIsCached_SecondCallDoesNotRequestNewToken()
    {
        var (service, handler) = BuildService();
        handler.SetupSequence(
            TokenSuccessResponse(),
            new MockResponse(HttpStatusCode.NoContent), // first DeactivateUser
            new MockResponse(HttpStatusCode.NoContent)  // second DeactivateUser — no new token request
        );

        var result1 = await service.DeactivateUserAsync(FixedUserId);
        var result2 = await service.DeactivateUserAsync(Guid.NewGuid());

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        handler.TotalRequestCount.Should().Be(3); // 1 token + 2 deactivate
    }

    // --- Helpers ---

    private static MockResponse TokenSuccessResponse() => new(
        HttpStatusCode.OK,
        Content: JsonSerializer.Serialize(new
        {
            access_token = "mock-access-token",
            expires_in = 300
        }));

    internal record MockResponse(
        HttpStatusCode StatusCode,
        string? Content = null,
        Dictionary<string, string>? Headers = null);

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<MockResponse> _responses = new();
        public int TotalRequestCount { get; private set; }

        public void SetupSequence(params MockResponse[] responses)
        {
            foreach (var r in responses)
                _responses.Enqueue(r);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TotalRequestCount++;

            if (_responses.Count == 0)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("No more mock responses configured")
                });

            var mock = _responses.Dequeue();
            var response = new HttpResponseMessage(mock.StatusCode);

            if (mock.Content is not null)
                response.Content = new StringContent(mock.Content, System.Text.Encoding.UTF8, "application/json");

            if (mock.Headers is not null)
            {
                foreach (var (key, value) in mock.Headers)
                {
                    if (key == "Location")
                        response.Headers.Location = new Uri(value);
                    else
                        response.Headers.TryAddWithoutValidation(key, value);
                }
            }

            return Task.FromResult(response);
        }
    }
}
