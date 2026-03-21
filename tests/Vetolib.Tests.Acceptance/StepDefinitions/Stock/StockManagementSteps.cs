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

    [When(@"I attempt to create a stock item with an empty name")]
    public async Task WhenIAttemptToCreateAStockItemWithAnEmptyName()
    {
        var request = new CreateStockItemRequest("", "Medication", 10, "ml", 5, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);
    }

    [When(@"I attempt to create a stock item with a quantity of (-?\d+)")]
    public async Task WhenIAttemptToCreateAStockItemWithAQuantityOf(int quantity)
    {
        var request = new CreateStockItemRequest("Test Item", "Medication", quantity, "ml", 5, null);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/stock", request);
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"the stock item is created successfully")]
    public void ThenTheStockItemIsCreatedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {(int)_lastResponse.StatusCode}");
        _currentItem.Should().NotBeNull();
    }

    [Then(@"the stock item is created with an identifier")]
    public void ThenTheStockItemIsCreatedWithAnIdentifier()
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

    [Then(@"the stock item quantity is (\d+)")]
    public void ThenTheStockItemQuantityIs(int expectedQuantity)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.Quantity.Should().Be(expectedQuantity);
    }

    [Then(@"the system indicates insufficient stock")]
    public void ThenTheSystemIndicatesInsufficientStock()
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

    [Then(@"the stock item threshold is (\d+)")]
    public void ThenTheStockItemThresholdIs(int expectedThreshold)
    {
        _currentItem.Should().NotBeNull();
        _currentItem!.MinThreshold.Should().Be(expectedThreshold);
    }

    [Then(@"the stock list is returned successfully")]
    public void ThenTheStockListIsReturnedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {(int)_lastResponse.StatusCode}");
        _currentList.Should().NotBeNull();
    }

    [Then(@"the user is denied access")]
    public void ThenTheUserIsDeniedAccess()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"Expected access denied but got {(int)_lastResponse.StatusCode}");
    }

    [Then(@"I should only see clinic A's stock items")]
    public void ThenIShouldOnlySeeClinicAStockItems()
    {
        _currentList.Should().NotBeNull();
        _currentList!.Should().HaveCount(1, "Clinic A should only see its own stock items");
        _currentList!.All(i => i.Quantity == 100).Should().BeTrue();
    }

    [Then(@"the system rejects the input as invalid")]
    public void ThenTheSystemRejectsTheInputAsInvalid()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse(
            $"Expected rejection but got {(int)_lastResponse.StatusCode}");
    }

    // ─── Helpers ────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
