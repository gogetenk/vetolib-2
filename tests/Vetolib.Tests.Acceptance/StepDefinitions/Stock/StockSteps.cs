using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Stock.Contracts;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Stock;

[Binding]
[Scope(Feature = "Gestion de stock médicaments et vaccins")]
internal class StockSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private StockItemDto? _currentItem;
    private HttpResponseMessage? _lastResponse;
    private string? _errorResponseBody;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public StockSteps(ScenarioContext ctx)
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

    [Given(@"un item de stock ""(.*)"" catégorie ""(.*)"" quantité (\d+) unité ""(.*)"" seuil (\d+)")]
    public async Task GivenUnItemDeStock(string name, string category, int quantity, string unit, int minThreshold)
    {
        var request = new CreateStockItemRequest(name, category, quantity, unit, minThreshold, null);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
        _currentItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        _currentItem.Should().NotBeNull();
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"je crée un item de stock ""(.*)"" catégorie ""(.*)"" quantité (\d+) unité ""(.*)"" seuil (\d+)")]
    public async Task WhenJeCreerUnItemDeStock(string name, string category, int quantity, string unit, int minThreshold)
    {
        var request = new CreateStockItemRequest(name, category, quantity, unit, minThreshold, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentItem = await _lastResponse.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"je liste les items de stock")]
    public async Task WhenJeListeLesItemsDeStock()
    {
        _lastResponse = await _client.GetAsync("/api/v1/stock");
        _lastResponse.EnsureSuccessStatusCode();
    }

    [When(@"j'enregistre un mouvement de stock ""(.*)"" quantité (\d+) raison ""(.*)""")]
    public async Task WhenJEnregistreUnMouvementDeStock(string movementType, int quantity, string reason)
    {
        var request = new CreateStockMovementRequest(movementType, quantity, reason);
        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/v1/stock/{_currentItem!.Id}/movements", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentItem = await _lastResponse.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"je consulte les alertes de stock")]
    public async Task WhenJeConsulteLesAlertesDeStock()
    {
        _lastResponse = await _client.GetAsync("/api/v1/stock/alerts");
        _lastResponse.EnsureSuccessStatusCode();
    }

    [When(@"je modifie le seuil de l'item à (\d+)")]
    public async Task WhenJeModifieLeSeuilDeLItemA(int newThreshold)
    {
        var request = new UpdateStockItemRequest(null, newThreshold);
        _lastResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/stock/{_currentItem!.Id}", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentItem = await _lastResponse.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"je tente de créer un item de stock avec un nom vide")]
    public async Task WhenJeTenteDeCreerUnItemAvecNomVide()
    {
        var request = new CreateStockItemRequest("", "Medication", 10, "ml", 5, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    [When(@"je tente de créer un item de stock avec une quantité de (-?\d+)")]
    public async Task WhenJeTenteDeCreerUnItemAvecQuantiteNegative(int quantity)
    {
        var request = new CreateStockItemRequest("Test Item", "Medication", quantity, "ml", 5, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"l'item de stock est créé avec le statut actif")]
    public void ThenLItemDeStockEstCreeAvecLeStatutActif()
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Id.Should().NotBeEmpty();
    }

    [Then(@"la quantité est (\d+)")]
    public void ThenLaQuantiteEst(int expectedQuantity)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Quantity.Should().Be(expectedQuantity);
    }

    [Then(@"la liste contient au moins (\d+) item")]
    public async Task ThenLaListeContientAuMoins(int minCount)
    {
        _lastResponse.Should().NotBeNull();
        var items = await _lastResponse!.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
        items.Should().NotBeNull();
        items!.Count.Should().BeGreaterThanOrEqualTo(minCount);
    }

    [Then(@"la nouvelle quantité est (\d+)")]
    public void ThenLaNouvelleQuantiteEst(int expectedQuantity)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Quantity.Should().Be(expectedQuantity);
    }

    [Then(@"l'alerte contient ""(.*)"" pour stock bas")]
    public async Task ThenLAlerteContientPourStockBas(string itemName)
    {
        _lastResponse.Should().NotBeNull();
        var alerts = await _lastResponse!.Content.ReadFromJsonAsync<StockAlertsDto>(JsonOptions);
        alerts.Should().NotBeNull();
        alerts!.LowStockItems.Should().Contain(i => i.Name == itemName);
    }

    [Then(@"le seuil est mis à jour à (\d+)")]
    public void ThenLeSeuilEstMisAJourA(int expectedThreshold)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.MinThreshold.Should().Be(expectedThreshold);
    }

    [Then(@"le système refuse avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseAvecLeCode(string errorCode)
    {
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNull();
        _errorResponseBody.Should().Contain(errorCode);
    }
}
