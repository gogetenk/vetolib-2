using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Billing;

[Binding]
[Scope(Feature = "Facturation vétérinaire")]
internal class FacturationSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private Guid _clinicId;
    private Guid _animalId;
    private InvoiceDto? _currentInvoice;
    private HttpResponseMessage? _lastResponse;
    private string? _errorResponseBody;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FacturationSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN ──────────────────────────────────────────────────

    [Given(@"une clinique ""(.*)""")]
    public void GivenUneClinique(string clinicName)
    {
        _clinicId = GenerateGuidFromString(clinicName);
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    [Given(@"un animal ""(.*)"" dans la clinique")]
    public void GivenUnAnimalDansLaClinique(string animalName)
    {
        _animalId = GenerateGuidFromString(animalName);
    }

    [Given(@"je suis authentifié en tant que VET")]
    public async Task GivenJeSuisAuthentifieEnTantQueVet()
    {
        // Create a VET user and login
        var email = "vet@happypaws.ae";
        var password = "VetPass123!";

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userResult = User.Create(_clinicId, email, password, UserRole.Vet, "VET-001");
        userResult.IsSuccess.Should().BeTrue();
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"une facture ""(.*)"" pour ""(.*)""")]
    public async Task GivenUneFacturePourAnimal(string status, string animalName)
    {
        // Create a DRAFT invoice first
        var createRequest = new CreateInvoiceRequest(
            _animalId,
            "Consultation",
            200m);

        var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
        response.EnsureSuccessStatusCode();
        _currentInvoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        _currentInvoice.Should().NotBeNull();

        // Transition to the desired status
        if (status == "SENT" || status == "PAID")
        {
            var sentResponse = await _client.PatchAsJsonAsync(
                $"/api/v1/invoices/{_currentInvoice!.Id}/status",
                new UpdateInvoiceStatusRequest(InvoiceStatus.Sent));
            sentResponse.EnsureSuccessStatusCode();
            _currentInvoice = await sentResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }

        if (status == "PAID")
        {
            var paidResponse = await _client.PatchAsJsonAsync(
                $"/api/v1/invoices/{_currentInvoice!.Id}/status",
                new UpdateInvoiceStatusRequest(InvoiceStatus.Paid));
            paidResponse.EnsureSuccessStatusCode();
            _currentInvoice = await paidResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
    }

    [Given(@"une facture ""DRAFT"" pour ""(.*)"" avec au moins un item")]
    public async Task GivenUneFactureDraftAvecAuMoinsUnItem(string animalName)
    {
        var createRequest = new CreateInvoiceRequest(
            _animalId,
            "Consultation",
            200m);

        var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
        response.EnsureSuccessStatusCode();
        _currentInvoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        _currentInvoice.Should().NotBeNull();
    }

    [Given(@"(\d+) factures existantes pour ""(.*)""")]
    public async Task GivenFacturesExistantesPour(int count, string clinicName)
    {
        for (int i = 0; i < count; i++)
        {
            var createRequest = new CreateInvoiceRequest(
                _animalId,
                $"Service {i + 1}",
                100m);

            var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
            response.EnsureSuccessStatusCode();
        }
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"je crée une facture pour ""(.*)"" avec l'item ""(.*)"" à (\d+) AED")]
    public async Task WhenJeCreerUneFacture(string animalName, string itemDescription, decimal price)
    {
        var request = new CreateInvoiceRequest(_animalId, itemDescription, price);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/invoices", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"j'ajoute l'item ""(.*)"" à (\d+) AED")]
    public async Task WhenJAjouteLItem(string description, decimal price)
    {
        var request = new AddInvoiceItemRequest(description, price);
        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/items", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"je passe la facture à ""(.*)""")]
    public async Task WhenJePasseLaFactureA(string status)
    {
        var newStatus = Enum.Parse<InvoiceStatus>(status, ignoreCase: true);
        _lastResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/status",
            new UpdateInvoiceStatusRequest(newStatus));

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"je marque la facture comme ""(.*)""")]
    public async Task WhenJeMarqueLaFactureComme(string status)
    {
        var newStatus = Enum.Parse<InvoiceStatus>(status, ignoreCase: true);
        _lastResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/status",
            new UpdateInvoiceStatusRequest(newStatus));

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"je tente d'ajouter un item à la facture")]
    public async Task WhenJeTenteDajouterUnItem()
    {
        var request = new AddInvoiceItemRequest("Extra Service", 50m);
        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/items", request);
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    [When(@"je crée une nouvelle facture")]
    public async Task WhenJeCreerUneNouvelleFacture()
    {
        var request = new CreateInvoiceRequest(_animalId, "Service", 100m);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/invoices", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"la facture est créée avec le statut ""(.*)""")]
    public void ThenLaFactureEstCreeeAvecLeStatut(string status)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Status.ToString().ToUpper().Should().Be(status);
    }

    [Then(@"le numéro est au format ""(.*)""")]
    public void ThenLeNumeroEstAuFormat(string expectedPattern)
    {
        _currentInvoice.Should().NotBeNull();
        // The pattern is like "INV-2026-001", verify the prefix matches
        _currentInvoice!.InvoiceNumber.Should().MatchRegex(@"^INV-\d{4}-\d{3}$");
    }

    [Then(@"la TVA de 5% est calculée automatiquement \((\d+) AED\)")]
    public void ThenLaTvaEstCalculee(decimal expectedTax)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.VatAmount.Should().Be(expectedTax);
    }

    [Then(@"le total est ([\d.]+) AED")]
    public void ThenLeTotalEst(decimal expectedTotal)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Total.Should().Be(expectedTotal);
    }

    [Then(@"la facture contient (\d+) items")]
    public void ThenLaFactureContientItems(int count)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Items.Should().HaveCount(count);
    }

    [Then(@"le sous-total est ([\d.]+) AED")]
    public void ThenLeSousTotalEst(decimal expectedSubTotal)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Subtotal.Should().Be(expectedSubTotal);
    }

    [Then(@"la TVA totale est ([\d.]+) AED")]
    public void ThenLaTvaTotaleEst(decimal expectedTax)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.VatAmount.Should().Be(expectedTax);
    }

    [Then(@"le statut est ""(.*)""")]
    public void ThenLeStatutEst(string status)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Status.ToString().ToUpper().Should().Be(status);
    }

    [Then(@"la date d'échéance est fixée à 30 jours")]
    public void ThenLaDateEcheanceEstFixeeA30Jours()
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.DueDate.Should().NotBeNull();
        var expectedDate = DateTime.UtcNow.AddDays(30);
        _currentInvoice.DueDate!.Value.Should().BeCloseTo(expectedDate, TimeSpan.FromMinutes(5));
    }

    [Then(@"le système refuse avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseAvecLeCode(string errorCode)
    {
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNull();
        _errorResponseBody.Should().Contain(errorCode);
    }

    [Then(@"le message est ""(.*)""")]
    public void ThenLeMessageEst(string expectedMessage)
    {
        _errorResponseBody.Should().Contain(expectedMessage);
    }

    [Then(@"le numéro est ""(.*)""")]
    public void ThenLeNumeroEst(string expectedNumber)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.InvoiceNumber.Should().Be(expectedNumber);
    }

    // ─── Helpers ────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
