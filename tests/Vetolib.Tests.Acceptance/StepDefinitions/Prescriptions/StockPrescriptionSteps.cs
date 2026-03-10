using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Prescriptions;

[Binding]
[Scope(Feature = "Stock-Prescription Integration")]
internal class StockPrescriptionSteps
{
    private readonly ScenarioContext _ctx;

    public StockPrescriptionSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN steps ─────────────────────────────────────────────

    [Given(@"a patient ""(.*)"" of species ""(.*)"" exists in my clinic")]
    public void GivenPatientOfSpeciesExistsInMyClinic(string patientName, string species)
    {
        throw new PendingStepException();
    }

    [Given(@"the drug catalog contains ""(.*)"" as a (.*)")]
    public void GivenDrugCatalogContainsEntry(string innName, string category)
    {
        throw new PendingStepException();
    }

    [Given(@"a stock item ""(.*)"" linked to catalog entry ""(.*)"" with quantity (\d+) and unit ""(.*)""")]
    public void GivenStockItemLinkedToCatalogWithQuantityAndUnit(string itemName, string catalogEntry, int quantity, string unit)
    {
        throw new PendingStepException();
    }

    [Given(@"a stock item ""(.*)"" linked to catalog entry ""(.*)"" with quantity (\d+) and unit ""(.*)"" and threshold (\d+)")]
    public void GivenStockItemLinkedToCatalogWithQuantityUnitAndThreshold(string itemName, string catalogEntry, int quantity, string unit, int threshold)
    {
        throw new PendingStepException();
    }

    [Given(@"a stock item ""(.*)"" linked to catalog entry ""(.*)"" with quantity (\d+)")]
    public void GivenStockItemLinkedToCatalogWithQuantity(string itemName, string catalogEntry, int quantity)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has no contraindication for ""(.*)""")]
    public void GivenDrugHasNoContraindicationForSpecies(string drug, string species)
    {
        throw new PendingStepException();
    }

    // ─── WHEN steps ──────────────────────────────────────────────

    [When(@"I start creating a prescription for patient ""(.*)"" with drug ""(.*)""")]
    public async Task WhenIStartCreatingPrescriptionWithDrug(string patientName, string drug)
    {
        throw new PendingStepException();
    }

    [When(@"I create a prescription for patient ""(.*)"" with drug ""(.*)"" and quantity (\d+)")]
    public async Task WhenICreatePrescriptionWithDrugAndQuantity(string patientName, string drug, int quantity)
    {
        throw new PendingStepException();
    }

    [When(@"I create a prescription for patient ""(.*)"" with free-text medication ""(.*)""")]
    public async Task WhenICreatePrescriptionWithFreeTextMedication(string patientName, string freeText)
    {
        throw new PendingStepException();
    }

    [When(@"I confirm ""(.*)""")]
    public async Task WhenIConfirm(string action)
    {
        throw new PendingStepException();
    }

    [When(@"I select ""(.*)""")]
    public async Task WhenISelect(string option)
    {
        throw new PendingStepException();
    }

    // ─── THEN steps ──────────────────────────────────────────────

    [Then(@"I should see stock information showing ""(.*)""")]
    public void ThenIShouldSeeStockInformation(string stockInfo)
    {
        throw new PendingStepException();
    }

    [Then(@"I should see a ""(.*)"" warning")]
    public void ThenIShouldSeeWarning(string warningLabel)
    {
        throw new PendingStepException();
    }

    [Then(@"I should see ""Out of stock"" for ""(.*)""")]
    public void ThenIShouldSeeOutOfStock(string drug)
    {
        throw new PendingStepException();
    }

    [Then(@"I should see ""(.*)"" suggested as an in-stock alternative with ""(.*)""")]
    public void ThenIShouldSeeInStockAlternative(string alternativeDrug, string availabilityInfo)
    {
        throw new PendingStepException();
    }

    [Then(@"the stock quantity for ""(.*)"" should be (\d+)")]
    public async Task ThenStockQuantityForItemShouldBe(string itemName, int expectedQuantity)
    {
        throw new PendingStepException();
    }

    [Then(@"the stock quantity for ""(.*)"" should remain (\d+)")]
    public async Task ThenStockQuantityForItemShouldRemain(string itemName, int expectedQuantity)
    {
        throw new PendingStepException();
    }

    [Then(@"the stock quantity should be (\d+)")]
    public async Task ThenStockQuantityShouldBe(int expectedQuantity)
    {
        throw new PendingStepException();
    }

    [Then(@"a stock movement of type ""(.*)"" with quantity (\d+) and reason containing ""(.*)"" should be recorded")]
    public async Task ThenStockMovementShouldBeRecorded(string movementType, int quantity, string reasonFragment)
    {
        throw new PendingStepException();
    }

    [Then(@"the prescription should be saved successfully")]
    public void ThenPrescriptionShouldBeSavedSuccessfully()
    {
        throw new PendingStepException();
    }

    [Then(@"I should see a warning ""(.*)""")]
    public void ThenIShouldSeeSpecificWarning(string warningMessage)
    {
        throw new PendingStepException();
    }

    [Then(@"I should be able to dispense the available (\d+) tablets")]
    public void ThenIShouldBeAbleToDispenseAvailableTablets(int quantity)
    {
        throw new PendingStepException();
    }

    [Then(@"I should not see stock information")]
    public void ThenIShouldNotSeeStockInformation()
    {
        throw new PendingStepException();
    }

    [Then(@"I should be able to manually select a stock item to decrement")]
    public void ThenIShouldBeAbleToManuallySelectStockItem()
    {
        throw new PendingStepException();
    }

    [Then(@"Or skip stock decrement entirely")]
    public void ThenOrSkipStockDecrement()
    {
        throw new PendingStepException();
    }

    [Then(@"a stock low alert should be triggered for ""(.*)""")]
    public async Task ThenStockLowAlertShouldBeTriggered(string itemName)
    {
        throw new PendingStepException();
    }
}
