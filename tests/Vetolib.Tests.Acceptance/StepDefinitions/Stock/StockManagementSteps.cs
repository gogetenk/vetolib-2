using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Stock;

[Binding]
[Scope(Feature = "Stock Management")]
internal class StockManagementSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;

    private Guid _defaultClinicId;
    private StockItemDto? _currentItem;
    private List<StockItemDto>? _currentList;
    private StockAlertsDto? _currentAlerts;
    private HttpResponseMessage? _lastResponse;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public StockManagementSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();

        // Use the fixed TestClinicGuid so the EF Core compiled query filter matches.
        _defaultClinicId = TestClinicContext.TestClinicGuid;
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _defaultClinicId;
    }

    // ─── AUTH ────────────────────────────────────────────────────

    [Given(@"I am authenticated as a user with role ""(.*)""")]
    public async Task GivenIAmAuthenticatedAsUserWithRole(string role)
    {
        var email = $"stock-{role.ToLowerInvariant()}@test.ae";
        var password = "SecurePass1";

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _defaultClinicId;

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "ADMIN" => UserRole.Admin,
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ASSISTANT" => UserRole.Assistant,
            _ => UserRole.Vet
        };

        var vetLicense = userRole == UserRole.Vet ? "STOCK-VET-001" : null;
        var userResult = User.Create(_defaultClinicId, email, password, userRole, vetLicense);
        userResult.IsSuccess.Should().BeTrue();

        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login should succeed for {email}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    // ─── GIVEN ──────────────────────────────────────────────────

    [Given(@"the following stock items exist:")]
    public async Task GivenTheFollowingStockItemsExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            var name = row["Name"];
            var category = row["Category"];
            var quantity = int.Parse(row["Quantity"]);
            var minThreshold = int.Parse(row["MinThreshold"]);
            var unit = row.ContainsKey("Unit") ? row["Unit"] : "units";

            var request = new CreateStockItemRequest(name, category, quantity, unit, minThreshold, null);
            var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
            response.EnsureSuccessStatusCode();
        }
    }

    [Given(@"a stock item ""(.*)"" exists with quantity (\d+)")]
    public async Task GivenAStockItemExistsWithQuantity(string name, int quantity)
    {
        var request = new CreateStockItemRequest(name, "Medication", quantity, "units", 20, null);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
        _currentItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        _currentItem.Should().NotBeNull();
    }

    [Given(@"a stock item ""(.*)"" exists with quantity (\d+) and threshold (\d+)")]
    public async Task GivenAStockItemExistsWithQuantityAndThreshold(string name, int quantity, int threshold)
    {
        var request = new CreateStockItemRequest(name, "Vaccine", quantity, "doses", threshold, null);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
        _currentItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        _currentItem.Should().NotBeNull();
    }

    [Given(@"a stock item ""(.*)"" exists with threshold (\d+)")]
    public async Task GivenAStockItemExistsWithThreshold(string name, int threshold)
    {
        var request = new CreateStockItemRequest(name, "Medication", 100, "units", threshold, null);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
        _currentItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        _currentItem.Should().NotBeNull();
    }

    [Given(@"a stock item ""(.*)"" exists with expiry date in (\d+) days")]
    public async Task GivenAStockItemExistsWithExpiryDateInDays(string name, int daysFromNow)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysFromNow);
        var request = new CreateStockItemRequest(name, "Medication", 50, "units", 10, expiryDate);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
        _currentItem = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        _currentItem.Should().NotBeNull();
    }

    // Multi-tenant scenario givens

    [Given(@"clinic A has stock item ""(.*)"" with quantity (\d+)")]
    public async Task GivenClinicAHasStockItemWithQuantity(string name, int quantity)
    {
        // Use TestClinicGuid for clinic A so the EF Core compiled query filter matches.
        // The compiled query filter uses the initial ClinicId value captured at model creation.
        var clinicAId = TestClinicContext.TestClinicGuid;
        await CreateStockItemForClinic(clinicAId, "admin-a@clinic-a.ae", name, quantity);
    }

    [Given(@"clinic B has stock item ""(.*)"" with quantity (\d+)")]
    public async Task GivenClinicBHasStockItemWithQuantity(string name, int quantity)
    {
        var clinicBId = GenerateGuidFromString("Clinic-B");
        await CreateStockItemForClinic(clinicBId, "admin-b@clinic-b.ae", name, quantity);
    }

    private async Task CreateStockItemForClinic(Guid clinicId, string email, string name, int quantity)
    {
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userResult = User.Create(clinicId, email, "SecurePass1", UserRole.Admin, null);
        var existing = await authDb.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "SecurePass1"));
        loginResponse.EnsureSuccessStatusCode();
        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        var request = new CreateStockItemRequest(name, "Medication", quantity, "units", 10, null);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"I create a stock item with:")]
    public async Task WhenICreateAStockItemWith(Table table)
    {
        var row = table.Rows[0];
        var name = row["Name"];
        var category = row["Category"];
        var quantity = int.Parse(row["Quantity"]);
        var unit = row.ContainsKey("Unit") ? row["Unit"] : "units";
        var minThreshold = int.Parse(row["MinThreshold"]);
        DateTime? expiryDate = null;
        if (row.ContainsKey("ExpiryDate") && !string.IsNullOrWhiteSpace(row["ExpiryDate"]))
            expiryDate = DateTime.SpecifyKind(DateTime.Parse(row["ExpiryDate"]), DateTimeKind.Utc);

        var request = new CreateStockItemRequest(name, category, quantity, unit, minThreshold, expiryDate);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);

        if (_lastResponse.IsSuccessStatusCode)
            _currentItem = await _lastResponse.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
    }

    [When(@"I request stock items filtered by category ""(.*)""")]
    public async Task WhenIRequestStockItemsFilteredByCategory(string category)
    {
        _lastResponse = await _client.GetAsync($"/api/v1/stock?category={category}");
        if (_lastResponse.IsSuccessStatusCode)
            _currentList = await _lastResponse.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
    }

    [When(@"I record a stock movement:")]
    public async Task WhenIRecordAStockMovement(Table table)
    {
        var row = table.Rows[0];
        var movementType = row["MovementType"];
        var quantity = int.Parse(row["Quantity"]);
        var reason = row["Reason"];

        var request = new CreateStockMovementRequest(movementType, quantity, reason);
        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/v1/stock/{_currentItem!.Id}/movements", request);

        if (_lastResponse.IsSuccessStatusCode)
            _currentItem = await _lastResponse.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
    }

    [When(@"I request stock alerts")]
    public async Task WhenIRequestStockAlerts()
    {
        _lastResponse = await _client.GetAsync("/api/v1/stock/alerts");
        if (_lastResponse.IsSuccessStatusCode)
            _currentAlerts = await _lastResponse.Content.ReadFromJsonAsync<StockAlertsDto>(JsonOptions);
    }

    [When(@"I update the stock item threshold to (\d+)")]
    public async Task WhenIUpdateTheStockItemThresholdTo(int newThreshold)
    {
        var request = new UpdateStockItemRequest(null, newThreshold);
        _lastResponse = await _client.PatchAsJsonAsync($"/api/v1/stock/{_currentItem!.Id}", request);

        if (_lastResponse.IsSuccessStatusCode)
            _currentItem = await _lastResponse.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
    }

    [When(@"I request stock items")]
    public async Task WhenIRequestStockItems()
    {
        _lastResponse = await _client.GetAsync("/api/v1/stock");
        if (_lastResponse.IsSuccessStatusCode)
            _currentList = await _lastResponse.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
    }

    [When(@"I am authenticated in clinic A")]
    public async Task WhenIAmAuthenticatedInClinicA()
    {
        // Must use TestClinicGuid so EF Core compiled query filter returns clinic A items.
        var clinicAId = TestClinicContext.TestClinicGuid;
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicAId;

        var email = "admin-a@clinic-a.ae";
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "SecurePass1"));
        loginResponse.EnsureSuccessStatusCode();
        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"the stock item should be created successfully")]
    public void ThenTheStockItemShouldBeCreatedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_lastResponse.StatusCode}");
        _currentItem.Should().NotBeNull();
    }

    [Then(@"the response should contain the stock item ID")]
    public void ThenTheResponseShouldContainTheStockItemId()
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Id.Should().NotBeEmpty();
    }

    [Then(@"I should receive (\d+) stock item")]
    public void ThenIShouldReceiveStockItems(int count)
    {
        _currentList.Should().NotBeNull();
        _currentList!.Should().HaveCount(count);
    }

    [Then(@"the item should be ""(.*)""")]
    public void ThenTheItemShouldBe(string name)
    {
        _currentList.Should().NotBeNull();
        _currentList!.Should().Contain(i => i.Name == name);
    }

    [Then(@"the stock item quantity should be (\d+)")]
    public void ThenTheStockItemQuantityShouldBe(int expectedQuantity)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Quantity.Should().Be(expectedQuantity);
    }

    [Then(@"I should receive an error indicating insufficient stock")]
    public void ThenIShouldReceiveAnErrorIndicatingInsufficientStock()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse(
            $"Expected error but got {(int)_lastResponse.StatusCode}");
    }

    [Then(@"the alerts should include ""(.*)"" as low-stock")]
    public void ThenTheAlertsShouldIncludeAsLowStock(string itemName)
    {
        _currentAlerts.Should().NotBeNull();
        _currentAlerts!.LowStockItems.Should().Contain(i => i.Name == itemName,
            $"Expected '{itemName}' in low stock alerts");
    }

    [Then(@"the alerts should include ""(.*)"" as expiring-soon")]
    public void ThenTheAlertsShouldIncludeAsExpiringSoon(string itemName)
    {
        _currentAlerts.Should().NotBeNull();
        _currentAlerts!.ExpiringItems.Should().Contain(i => i.Name == itemName,
            $"Expected '{itemName}' in expiring soon alerts");
    }

    [Then(@"the stock item threshold should be (\d+)")]
    public void ThenTheStockItemThresholdShouldBe(int expectedThreshold)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.MinThreshold.Should().Be(expectedThreshold);
    }

    [Then(@"I should receive the stock list successfully")]
    public void ThenIShouldReceiveTheStockListSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_lastResponse.StatusCode}");
        _currentList.Should().NotBeNull();
    }

    [Then(@"I should receive a 403 Forbidden response")]
    public void ThenIShouldReceiveA403ForbiddenResponse()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"Expected 403 but got {(int)_lastResponse.StatusCode}");
    }

    [Then(@"I should only see clinic A's stock items")]
    public void ThenIShouldOnlySeeClinicAStockItems()
    {
        _currentList.Should().NotBeNull();
        _currentList!.Should().HaveCount(1, "Clinic A should only see its own stock items");
        _currentList!.All(i => i.Quantity == 100).Should().BeTrue();
    }

    // ─── Helpers ────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
