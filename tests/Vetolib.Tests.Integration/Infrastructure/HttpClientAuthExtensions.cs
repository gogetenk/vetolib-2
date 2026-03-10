using System.Net.Http.Headers;
using Vetolib.Auth.Contracts;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Extension methods for configuring HttpClient authentication in integration tests.
/// </summary>
public static class HttpClientAuthExtensions
{
    public static HttpClient WithAdminAuth(this HttpClient client, Guid clinicId)
    {
        var token = TestJwtGenerator.GenerateAdminToken(clinicId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithVetAuth(this HttpClient client, Guid clinicId, Guid? userId = null)
    {
        var token = TestJwtGenerator.GenerateVetToken(clinicId, userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithReceptionistAuth(this HttpClient client, Guid clinicId)
    {
        var token = TestJwtGenerator.GenerateReceptionistToken(clinicId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithRole(
        this HttpClient client,
        Guid clinicId,
        UserRole role,
        Guid? userId = null,
        string? vetLicenseNumber = null)
    {
        var token = TestJwtGenerator.GenerateToken(
            userId: userId ?? Guid.NewGuid(),
            email: $"{role.ToString().ToLowerInvariant()}@test.ae",
            clinicId: clinicId,
            role: role,
            vetLicenseNumber: vetLicenseNumber);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithoutAuth(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }
}
