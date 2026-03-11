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
[Scope(Feature = "Medication and vaccine stock management")]
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

    [Given(@"a stock item ""(.*)"" category ""(.*)"" quantity (\d+) unit ""(.*)"" threshold (\d+)")]
    public async Task GivenAStockItem(string name, string category, int quantity, string unit, int minThreshold)
    {
        var request = new CreateStockItemRequest(name, category, quantity, unit, minThreshold, null);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
        _currentItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        _currentItem.Should().NotBeNull();
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"I create a stock item ""(.*)"" category ""(.*)"" quantity (\d+) unit ""(.*)"" threshold (\d+)")]
    public async Task WhenICreateAStockItem(string name, string category, int quantity, string unit, int minThreshold)
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

    [When(@"I list stock items")]
    public async Task WhenIListStockItems()
    {
        _lastResponse = await _client.GetAsync("/api/v1/stock");
        _lastResponse.EnsureSuccessStatusCode();
    }

    [When(@"I record a stock movement ""(.*)"" quantity (\d+) reason ""(.*)""")]
    public async Task WhenIRecordAStockMovement(string movementType, int quantity, string reason)
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

    [When(@"I check stock alerts")]
    public async Task WhenICheckStockAlerts()
    {
        _lastResponse = await _client.GetAsync("/api/v1/stock/alerts");
        _lastResponse.EnsureSuccessStatusCode();
    }

    [When(@"I update the item threshold to (\d+)")]
    public async Task WhenIUpdateTheItemThresholdTo(int newThreshold)
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

    [When(@"I attempt to create a stock item with an empty name")]
    public async Task WhenIAttemptToCreateAStockItemWithAnEmptyName()
    {
        var request = new CreateStockItemRequest("", "Medication", 10, "ml", 5, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    [When(@"I attempt to create a stock item with a quantity of (-?\d+)")]
    public async Task WhenIAttemptToCreateAStockItemWithAQuantityOf(int quantity)
    {
        var request = new CreateStockItemRequest("Test Item", "Medication", quantity, "ml", 5, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"the stock item is created with active status")]
    public void ThenTheStockItemIsCreatedWithActiveStatus()
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Id.Should().NotBeEmpty();
    }

    [Then(@"the quantity is (\d+)")]
    public void ThenTheQuantityIs(int expectedQuantity)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Quantity.Should().Be(expectedQuantity);
    }

    [Then(@"the list contains at least (\d+) item")]
    public async Task ThenTheListContainsAtLeast(int minCount)
    {
        _lastResponse.Should().NotBeNull();
        var items = await _lastResponse!.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
        items.Should().NotBeNull();
        items!.Count.Should().BeGreaterThanOrEqualTo(minCount);
    }

    [Then(@"the new quantity is (\d+)")]
    public void ThenTheNewQuantityIs(int expectedQuantity)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Quantity.Should().Be(expectedQuantity);
    }

    [Then(@"the alert includes ""(.*)"" for low stock")]
    public async Task ThenTheAlertIncludesForLowStock(string itemName)
    {
        _lastResponse.Should().NotBeNull();
        var alerts = await _lastResponse!.Content.ReadFromJsonAsync<StockAlertsDto>(JsonOptions);
        alerts.Should().NotBeNull();
        alerts!.LowStockItems.Should().Contain(i => i.Name == itemName);
    }

    [Then(@"the threshold is updated to (\d+)")]
    public void ThenTheThresholdIsUpdatedTo(int expectedThreshold)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.MinThreshold.Should().Be(expectedThreshold);
    }

    [Then(@"the system rejects with code ""(.*)""")]
    public void ThenTheSystemRejectsWithCode(string errorCode)
    {
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNull();
        _errorResponseBody.Should().Contain(errorCode);
    }
}
