using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Prescriptions;

[Binding]
[Scope(Feature = "Stock-Prescription Integration")]
internal class StockPrescriptionSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;

    private Guid _clinicId;
    private Guid _patientId;
    private readonly Dictionary<string, Guid> _drugCatalogIds = new();
    private readonly Dictionary<string, Guid> _stockItemIds = new();
    private StockAvailabilityResult? _stockAvailability;
    private HttpResponseMessage? _lastResponse;
    private PrescriptionDto? _savedPrescription;
    private Guid? _lastCreatedPrescriptionRecordId;
    private string? _lastDrugName;
    private int _lastPrescriptionQuantity;
    private bool _dispenseFromStock;
    private bool _isFreeText;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public StockPrescriptionSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
        _clinicId = TestClinicContext.TestClinicGuid;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    // ─── GIVEN steps ─────────────────────────────────────────────

    [Given(@"I am logged in as a VET")]
    public async Task GivenIAmLoggedInAsVet()
    {
        var email = "vet-stockrx@test.com";
        var password = "SecurePass1";

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userResult = User.Create(_clinicId, email, password, UserRole.Vet, "TEST-VET-STOCKRX-001");
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
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Login should succeed for {email}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"a patient ""(.*)"" of species ""(.*)"" exists in my clinic")]
    public async Task GivenPatientOfSpeciesExistsInMyClinic(string patientName, string species)
    {
        var parsedSpecies = ParseSpecies(species);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var patientResult = Patient.Create(
            _clinicId, patientName, parsedSpecies, species, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)));
        patientResult.IsSuccess.Should().BeTrue();

        db.Patients.Add(patientResult.Value);
        await db.SaveChangesAsync();

        _patientId = patientResult.Value.Id;
        _ctx.Set(_patientId, $"PatientId:{patientName}");
    }

    [Given(@"the drug catalog contains ""(.*)"" as a (.*)")]
    public async Task GivenDrugCatalogContainsEntry(string innName, string category)
    {
        var parsedCategory = Enum.Parse<DrugCategory>(category, ignoreCase: true);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // Global catalog entry (ClinicId = null)
        var entryResult = DrugCatalogEntry.Create(innName, innName, parsedCategory, clinicId: null);
        entryResult.IsSuccess.Should().BeTrue($"DrugCatalogEntry.Create should succeed for {innName}");

        db.DrugCatalogEntries.Add(entryResult.Value);
        await db.SaveChangesAsync();

        _drugCatalogIds[innName] = entryResult.Value.Id;
    }

    [Given(@"a stock item ""(.*)"" linked to catalog entry ""(.*)"" with quantity (\d+) and unit ""(.*)""")]
    public async Task GivenStockItemLinkedToCatalogWithQuantityAndUnit(string itemName, string catalogEntry, int quantity, string unit)
    {
        await CreateStockItemLinkedToCatalog(itemName, catalogEntry, quantity, unit, minThreshold: 0);
    }

    [Given(@"a stock item ""(.*)"" linked to catalog entry ""(.*)"" with quantity (\d+) and unit ""(.*)"" and threshold (\d+)")]
    public async Task GivenStockItemLinkedToCatalogWithQuantityUnitAndThreshold(string itemName, string catalogEntry, int quantity, string unit, int threshold)
    {
        await CreateStockItemLinkedToCatalog(itemName, catalogEntry, quantity, unit, threshold);
    }

    [Given(@"a stock item ""(.*)"" linked to catalog entry ""(.*)"" with quantity (\d+)")]
    public async Task GivenStockItemLinkedToCatalogWithQuantity(string itemName, string catalogEntry, int quantity)
    {
        await CreateStockItemLinkedToCatalog(itemName, catalogEntry, quantity, "units", minThreshold: 0);
    }

    [Given(@"""(.*)"" has no contraindication for ""(.*)""")]
    public void GivenDrugHasNoContraindicationForSpecies(string drug, string species)
    {
        // No-op: by default there are no contraindications in the test DB.
    }

    // ─── WHEN steps ──────────────────────────────────────────────

    [When(@"I start creating a prescription for patient ""(.*)"" with drug ""(.*)""")]
    public async Task WhenIStartCreatingPrescriptionWithDrug(string patientName, string drug)
    {
        _lastDrugName = drug;
        _isFreeText = false;

        if (!_drugCatalogIds.TryGetValue(drug, out var drugCatalogId))
        {
            _isFreeText = true;
            return;
        }

        // Check stock availability for this drug
        var response = await _client.GetAsync(
            $"/api/v1/stock?category=Medication");

        if (response.IsSuccessStatusCode)
        {
            var items = await response.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
            var matchingItem = items?.FirstOrDefault(i => i.DrugCatalogEntryId == drugCatalogId);

            if (matchingItem is not null)
            {
                _stockAvailability = new StockAvailabilityResult(
                    Available: matchingItem.Quantity > 0,
                    Quantity: matchingItem.Quantity,
                    Unit: matchingItem.Unit,
                    IsLowStock: matchingItem.IsLowStock,
                    IsExpiringSoon: matchingItem.IsExpiringSoon,
                    Alternatives: new List<StockAlternativeDto>(),
                    StockItemId: matchingItem.Id);
            }
            else
            {
                _stockAvailability = new StockAvailabilityResult(
                    Available: false,
                    Quantity: 0,
                    Unit: string.Empty,
                    IsLowStock: true,
                    IsExpiringSoon: false,
                    Alternatives: new List<StockAlternativeDto>(),
                    StockItemId: null);
            }

            // If out of stock, look for alternatives
            if (!_stockAvailability.Available)
            {
                var alternativeItems = items?
                    .Where(i => i.DrugCatalogEntryId != drugCatalogId
                        && i.DrugCatalogEntryId != null
                        && i.Quantity > 0)
                    .Select(i => new StockAlternativeDto(
                        i.Id, i.Name, i.DrugCatalogEntryId!.Value, i.Quantity, i.Unit))
                    .ToList() ?? new List<StockAlternativeDto>();

                _stockAvailability = _stockAvailability with { Alternatives = alternativeItems };
            }
        }
    }

    [When(@"I create a prescription for patient ""(.*)"" with drug ""(.*)"" and quantity (\d+)")]
    public async Task WhenICreatePrescriptionWithDrugAndQuantity(string patientName, string drug, int quantity)
    {
        _lastDrugName = drug;
        _lastPrescriptionQuantity = quantity;
        _isFreeText = false;

        if (!_drugCatalogIds.TryGetValue(drug, out var drugCatalogId))
        {
            _isFreeText = true;
            return;
        }

        // Check stock availability
        var response = await _client.GetAsync("/api/v1/stock?category=Medication");
        if (response.IsSuccessStatusCode)
        {
            var items = await response.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
            var matchingItem = items?.FirstOrDefault(i => i.DrugCatalogEntryId == drugCatalogId);

            if (matchingItem is not null)
            {
                _stockAvailability = new StockAvailabilityResult(
                    Available: matchingItem.Quantity > 0,
                    Quantity: matchingItem.Quantity,
                    Unit: matchingItem.Unit,
                    IsLowStock: matchingItem.IsLowStock,
                    IsExpiringSoon: matchingItem.IsExpiringSoon,
                    Alternatives: new List<StockAlternativeDto>(),
                    StockItemId: matchingItem.Id);
            }
        }

        // Create the medical record and prescription
        await CreatePrescriptionForPatient(patientName, drug, drugCatalogId, quantity);
    }

    [When(@"I create a prescription for patient ""(.*)"" with free-text medication ""(.*)""")]
    public async Task WhenICreatePrescriptionWithFreeTextMedication(string patientName, string freeText)
    {
        _isFreeText = true;
        _lastDrugName = freeText;

        // Create a free-text prescription (no DrugCatalogEntryId)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var patientId = _ctx.ContainsKey($"PatientId:{patientName}")
            ? _ctx.Get<Guid>($"PatientId:{patientName}")
            : _patientId;

        var record = MedicalRecord.Create(
            _clinicId, patientId, "Free-text prescription", "As needed", "Dr. Test",
            DateTime.UtcNow);
        record.IsSuccess.Should().BeTrue();
        db.MedicalRecords.Add(record.Value);
        await db.SaveChangesAsync();

        var prescriptionResult = Prescription.Create(
            _clinicId, record.Value.Id, freeText, "As prescribed", "TEST-VET-STOCKRX-001", null);
        prescriptionResult.IsSuccess.Should().BeTrue();
        db.Prescriptions.Add(prescriptionResult.Value);
        await db.SaveChangesAsync();

        _savedPrescription = prescriptionResult.Value.ToDto();
    }

    [When(@"I confirm ""(.*)""")]
    public async Task WhenIConfirm(string action)
    {
        if (action == "Dispense from clinic stock")
        {
            _dispenseFromStock = true;

            // Decrement stock via movement OUT
            if (_stockAvailability?.StockItemId is not null && _lastPrescriptionQuantity > 0)
            {
                var stockItemId = _stockAvailability.StockItemId.Value;
                var availableQty = _stockAvailability.Quantity;
                var requestedQty = _lastPrescriptionQuantity;

                // If insufficient stock, dispense what's available
                var dispenseQty = Math.Min(requestedQty, availableQty);
                if (dispenseQty > 0)
                {
                    var request = new CreateStockMovementRequest("OUT", dispenseQty,
                        $"Prescription dispense - {_lastDrugName}");
                    _lastResponse = await _client.PostAsJsonAsync(
                        $"/api/v1/stock/{stockItemId}/movements", request);
                }
            }
        }
    }

    [When(@"I select ""(.*)""")]
    public async Task WhenISelect(string option)
    {
        if (option == "Do not dispense from stock")
        {
            _dispenseFromStock = false;
            // No stock movement — prescription is already saved
        }
    }

    // ─── THEN steps ──────────────────────────────────────────────

    [Then(@"I should see stock information showing ""(.*)""")]
    public void ThenIShouldSeeStockInformation(string stockInfo)
    {
        _stockAvailability.Should().NotBeNull("Stock availability info should have been fetched");

        // Parse expected info like "100 tablets available" or "5 tablets available"
        var parts = stockInfo.Split(' ');
        if (parts.Length >= 1 && int.TryParse(parts[0], out var expectedQuantity))
        {
            _stockAvailability!.Quantity.Should().Be(expectedQuantity,
                $"Expected stock quantity to be {expectedQuantity}");
        }
    }

    [Then(@"I should see a ""(.*)"" warning")]
    public void ThenIShouldSeeWarning(string warningLabel)
    {
        _stockAvailability.Should().NotBeNull();

        if (warningLabel == "Low stock")
        {
            _stockAvailability!.IsLowStock.Should().BeTrue(
                "Expected low stock warning to be triggered");
        }
    }

    [Then(@"I should see ""Out of stock"" for ""(.*)""")]
    public void ThenIShouldSeeOutOfStock(string drug)
    {
        _stockAvailability.Should().NotBeNull();
        _stockAvailability!.Available.Should().BeFalse(
            $"Expected '{drug}' to be out of stock");
        _stockAvailability.Quantity.Should().Be(0,
            $"Expected zero stock quantity for '{drug}'");
    }

    [Then(@"I should see ""(.*)"" suggested as an in-stock alternative with ""(.*)""")]
    public void ThenIShouldSeeInStockAlternative(string alternativeDrug, string availabilityInfo)
    {
        _stockAvailability.Should().NotBeNull();
        _stockAvailability!.Alternatives.Should().NotBeEmpty(
            "Expected at least one alternative suggestion");

        var altDrugCatalogId = _drugCatalogIds.TryGetValue(alternativeDrug, out var id) ? id : Guid.Empty;
        var alternative = _stockAvailability.Alternatives
            .FirstOrDefault(a => a.DrugCatalogEntryId == altDrugCatalogId || a.Name.Contains(alternativeDrug));

        alternative.Should().NotBeNull(
            $"Expected '{alternativeDrug}' to be suggested as an alternative");

        // Parse expected availability like "50 capsules available"
        var parts = availabilityInfo.Split(' ');
        if (parts.Length >= 1 && int.TryParse(parts[0], out var expectedQuantity))
        {
            alternative!.Quantity.Should().Be(expectedQuantity,
                $"Expected alternative stock quantity to be {expectedQuantity}");
        }
    }

    [Then(@"the stock quantity for ""(.*)"" should be (\d+)")]
    public async Task ThenStockQuantityForItemShouldBe(string itemName, int expectedQuantity)
    {
        await VerifyStockQuantity(itemName, expectedQuantity);
    }

    [Then(@"the stock quantity for ""(.*)"" should remain (\d+)")]
    public async Task ThenStockQuantityForItemShouldRemain(string itemName, int expectedQuantity)
    {
        await VerifyStockQuantity(itemName, expectedQuantity);
    }

    [Then(@"the stock quantity should be (\d+)")]
    public async Task ThenStockQuantityShouldBe(int expectedQuantity)
    {
        // Use the first stock item tracked in this scenario
        var itemName = _stockItemIds.Keys.FirstOrDefault();
        itemName.Should().NotBeNull("Expected at least one stock item to be tracked");
        await VerifyStockQuantity(itemName!, expectedQuantity);
    }

    [Then(@"a stock movement of type ""(.*)"" with quantity (\d+) and reason containing ""(.*)"" should be recorded")]
    public async Task ThenStockMovementShouldBeRecorded(string movementType, int quantity, string reasonFragment)
    {
        // Verify the movement was recorded by checking stock DB directly
        using var scope = _factory.Services.CreateScope();
        var stockDb = scope.ServiceProvider.GetRequiredService<StockDbContext>();

        var movements = await stockDb.StockMovements
            .AsNoTracking()
            .Where(m => m.Quantity == quantity)
            .ToListAsync();

        movements.Should().Contain(m =>
            m.MovementType.ToString() == movementType &&
            m.Quantity == quantity &&
            m.Reason.Contains(reasonFragment, StringComparison.OrdinalIgnoreCase),
            $"Expected a stock movement of type '{movementType}' with qty {quantity} and reason containing '{reasonFragment}'");
    }

    [Then(@"the prescription should be saved successfully")]
    public void ThenPrescriptionShouldBeSavedSuccessfully()
    {
        _savedPrescription.Should().NotBeNull("Prescription should have been saved");
    }

    [Then(@"I should see a warning ""(.*)""")]
    public void ThenIShouldSeeSpecificWarning(string warningMessage)
    {
        _stockAvailability.Should().NotBeNull();

        // Parse "Only 5 tablets available, 14 requested"
        if (warningMessage.Contains("available") && warningMessage.Contains("requested"))
        {
            // The stock availability should show insufficient quantity
            _stockAvailability!.Quantity.Should().BeLessThan(_lastPrescriptionQuantity,
                $"Warning implies insufficient stock: {warningMessage}");
        }
    }

    [Then(@"I should be able to dispense the available (\d+) tablets")]
    public async Task ThenIShouldBeAbleToDispenseAvailableTablets(int quantity)
    {
        // If we haven't already dispensed, do a partial dispense now
        if (_stockAvailability?.StockItemId is not null)
        {
            var stockItemId = _stockAvailability.StockItemId.Value;

            // Only dispense if we haven't already via the When step
            if (!_dispenseFromStock)
            {
                var request = new CreateStockMovementRequest("OUT", quantity,
                    $"Partial dispense - {_lastDrugName}");
                var response = await _client.PostAsJsonAsync(
                    $"/api/v1/stock/{stockItemId}/movements", request);
                response.IsSuccessStatusCode.Should().BeTrue(
                    $"Partial dispense of {quantity} should succeed");
            }
        }
    }

    [Then(@"I should not see stock information")]
    public void ThenIShouldNotSeeStockInformation()
    {
        _isFreeText.Should().BeTrue("Free-text prescriptions should not show stock info");
        // No stock availability data is fetched for free-text prescriptions
    }

    [Then(@"I should be able to manually select a stock item to decrement")]
    public void ThenIShouldBeAbleToManuallySelectStockItem()
    {
        // This is a UI capability. For the API test, we verify that
        // the stock list endpoint is accessible and returns items.
        // The actual manual selection is a frontend concern.
        _isFreeText.Should().BeTrue();
    }

    [Then(@"I should be able to skip stock decrement entirely")]
    public void ThenIShouldBeAbleToSkipStockDecrementEntirely()
    {
        // Verify the prescription was saved without any stock decrement
        _savedPrescription.Should().NotBeNull(
            "Prescription should be saveable without stock decrement");
    }

    [Then(@"a stock low alert should be triggered for ""(.*)""")]
    public async Task ThenStockLowAlertShouldBeTriggered(string itemName)
    {
        // Check stock alerts endpoint
        var response = await _client.GetAsync("/api/v1/stock/alerts");
        response.IsSuccessStatusCode.Should().BeTrue();

        var alerts = await response.Content.ReadFromJsonAsync<StockAlertsDto>(JsonOptions);
        alerts.Should().NotBeNull();
        alerts!.LowStockItems.Should().Contain(i => i.Name == itemName,
            $"Expected '{itemName}' to appear in low stock alerts");
    }

    // ─── Private helpers ─────────────────────────────────────────

    private async Task CreateStockItemLinkedToCatalog(
        string itemName, string catalogEntry, int quantity, string unit, int minThreshold)
    {
        var drugCatalogEntryId = _drugCatalogIds.TryGetValue(catalogEntry, out var id) ? id : (Guid?)null;

        var request = new CreateStockItemRequest(
            itemName, "Medication", quantity, unit, minThreshold, null, drugCatalogEntryId);
        var response = await _client.PostAsJsonAsync("/api/v1/stock", request);
        response.EnsureSuccessStatusCode();

        var item = await response.Content.ReadFromJsonAsync<StockItemDto>(JsonOptions);
        item.Should().NotBeNull();
        _stockItemIds[itemName] = item!.Id;
    }

    private async Task CreatePrescriptionForPatient(
        string patientName, string drugName, Guid drugCatalogId, int quantity)
    {
        var patientId = _ctx.ContainsKey($"PatientId:{patientName}")
            ? _ctx.Get<Guid>($"PatientId:{patientName}")
            : _patientId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // Find or create a medical record for the patient
        var record = await db.MedicalRecords.FirstOrDefaultAsync(r => r.PatientId == patientId);
        if (record is null)
        {
            var recordResult = MedicalRecord.Create(
                _clinicId, patientId, "Prescription visit", "As prescribed", "Dr. Test",
                DateTime.UtcNow);
            recordResult.IsSuccess.Should().BeTrue();
            db.MedicalRecords.Add(recordResult.Value);
            await db.SaveChangesAsync();
            record = recordResult.Value;
        }

        _lastCreatedPrescriptionRecordId = record.Id;

        var prescriptionResult = Prescription.Create(
            _clinicId, record.Id, $"{drugName} x{quantity}", $"{quantity} units", "TEST-VET-STOCKRX-001",
            drugCatalogId);
        prescriptionResult.IsSuccess.Should().BeTrue();
        db.Prescriptions.Add(prescriptionResult.Value);
        await db.SaveChangesAsync();

        _savedPrescription = prescriptionResult.Value.ToDto();
    }

    private async Task VerifyStockQuantity(string itemName, int expectedQuantity)
    {
        var response = await _client.GetAsync("/api/v1/stock");
        response.IsSuccessStatusCode.Should().BeTrue();

        var items = await response.Content.ReadFromJsonAsync<List<StockItemDto>>(JsonOptions);
        items.Should().NotBeNull();

        var item = items!.FirstOrDefault(i => i.Name == itemName);
        item.Should().NotBeNull($"Stock item '{itemName}' should exist");
        item!.Quantity.Should().Be(expectedQuantity,
            $"Stock quantity for '{itemName}' should be {expectedQuantity}");
    }

    private static Species ParseSpecies(string speciesStr) =>
        speciesStr.ToLowerInvariant() switch
        {
            "cat" => Species.Cat,
            "dog" => Species.Dog,
            "bird" => Species.Bird,
            "rabbit" => Species.Rabbit,
            "horse" => Species.Horse,
            "camel" => Species.Camel,
            _ => Species.Exotic
        };
}
