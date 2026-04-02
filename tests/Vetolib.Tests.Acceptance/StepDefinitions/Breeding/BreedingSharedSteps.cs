using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Breeding;

/// <summary>
/// Shared step definitions for all Breeding feature files.
/// Handles owner creation, patient creation with sex, and common assertions.
/// </summary>
[Binding]
[Scope(Feature = "Heat cycle tracking")]
[Scope(Feature = "Pregnancy and gestation tracking")]
[Scope(Feature = "Patient lineage and pedigree")]
[Scope(Feature = "Litter management")]
internal class BreedingSharedSteps
{
    private readonly ScenarioContext _ctx;

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public BreedingSharedSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── Owner setup ─────────────────────────────────────────────

    [Given(@"an owner ""([^""]*)"" with email ""([^""]*)""")]
    public async Task GivenAnOwnerWithEmail(string ownerName, string email)
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var clinicIds = GetClinicIds();
        var clinicId = clinicIds.Values.First();

        var names = ownerName.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : "";

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var ownerResult = Owner.Create(clinicId, firstName, lastName, email, null);
        ownerResult.IsSuccess.Should().BeTrue($"Owner creation should succeed for {ownerName}");

        db.Owners.Add(ownerResult.Value);
        await db.SaveChangesAsync();

        var ownerIds = GetOrCreateDict<Guid>("OwnerIds");
        ownerIds[ownerName] = ownerResult.Value.Id;
    }

    // ─── Patient setup with species, breed, sex ──────────────────

    [Given(@"a patient ""([^""]*)"" species ""([^""]*)"" breed ""([^""]*)"" sex ""([^""]*)"" belonging to ""([^""]*)""")]
    public async Task GivenAPatientWithSpeciesBreedSexBelongingTo(
        string patientName, string species, string breed, string sex, string ownerName)
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var clinicIds = GetClinicIds();
        var clinicId = clinicIds.Values.First();
        var ownerIds = GetOrCreateDict<Guid>("OwnerIds");
        var ownerId = ownerIds[ownerName];

        var parsedSpecies = Enum.Parse<Species>(species);
        var parsedSex = Enum.Parse<Sex>(sex);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var patientResult = Patient.Create(clinicId, patientName, parsedSpecies, breed,
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)), parsedSex);
        patientResult.IsSuccess.Should().BeTrue($"Patient creation should succeed for {patientName}");

        var patient = patientResult.Value;
        var patientOwner = PatientOwner.Create(clinicId, patient.Id, ownerId);
        patient.AddOwner(patientOwner);

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var patientIds = GetOrCreateDict<Guid>("PatientIds");
        patientIds[patientName] = patient.Id;
    }

    // ─── Shared error assertion ──────────────────────────────────

    [Then(@"the system rejects with reason ""([^""]*)""")]
    public async Task ThenTheSystemRejectsWithReason(string expectedReason)
    {
        var response = _ctx.Get<HttpResponseMessage>("LastResponse");
        response.IsSuccessStatusCode.Should().BeFalse(
            $"Expected an error response but got {(int)response.StatusCode}");

        // Authorization failures (403) from ASP.NET return an empty body — match by status code
        if (expectedReason == "Insufficient permissions")
        {
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden,
                $"Expected 403 Forbidden for '{expectedReason}' but got {(int)response.StatusCode}");
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(expectedReason,
            $"Expected error body to contain '{expectedReason}' but got: {body}");
    }

    // ─── Auth with clinic context ────────────────────────────────

    [Given(@"I am authenticated as VET in ""([^""]*)""")]
    public async Task GivenIAmAuthenticatedAsVetInClinic(string clinicName)
    {
        var clinicIds = GetClinicIds();
        var clinicId = clinicIds.ContainsKey(clinicName)
            ? clinicIds[clinicName]
            : SharedSteps.GenerateGuidFromString(clinicName);
        clinicIds[clinicName] = clinicId;

        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        // Re-authenticate as VET for the new clinic
        var client = _ctx.Get<HttpClient>();

        using var scope = factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<Vetolib.Auth.Infrastructure.AuthDbContext>();
        var email = $"vet-{clinicName.ToLowerInvariant().Replace(" ", "")}@test.com";
        var password = "SecurePass1!";

        var userResult = Vetolib.Auth.Application.Domain.User.Create(
            clinicId, email, password,
            Vetolib.Auth.Contracts.UserRole.Vet, "TEST-VET-002");
        userResult.IsSuccess.Should().BeTrue();

        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login",
            new Vetolib.Auth.Contracts.LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authToken = await loginResponse.Content.ReadFromJsonAsync<Vetolib.Auth.Contracts.AuthTokenDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _ctx.Set("VET", "CurrentRole");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    internal Dictionary<string, Guid> GetClinicIds()
    {
        if (_ctx.ContainsKey("ClinicIds"))
            return _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "ClinicIds");
        return dict;
    }

    internal Dictionary<string, T> GetOrCreateDict<T>(string key)
    {
        if (_ctx.ContainsKey(key))
            return _ctx.Get<Dictionary<string, T>>(key);
        var dict = new Dictionary<string, T>();
        _ctx.Set(dict, key);
        return dict;
    }
}
