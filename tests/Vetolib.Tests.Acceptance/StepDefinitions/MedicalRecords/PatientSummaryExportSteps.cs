using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.MedicalRecords;

[Binding]
[Scope(Feature = "Patient medical summary export")]
internal class PatientSummaryExportSteps
{
    private readonly ScenarioContext _ctx;

    public PatientSummaryExportSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"an animal ""(.*)"" breed ""(.*)"" belonging to ""(.*)""")]
    public void GivenAnAnimalBreedBelongingTo(string animalName, string breed, string ownerName)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has medical records in the system")]
    public void GivenHasMedicalRecordsInTheSystem(string animalName)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has the following vaccination records:")]
    public void GivenHasTheFollowingVaccinationRecords(string animalName, Table table)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has an active prescription for ""(.*)""")]
    public void GivenHasAnActivePrescriptionFor(string animalName, string medication)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has a medical record with an internal note ""(.*)""")]
    public void GivenHasAMedicalRecordWithAnInternalNote(string animalName, string noteText)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I export the medical summary for ""(.*)""")]
    public void WhenIExportTheMedicalSummaryFor(string animalName)
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"a summary document is generated")]
    public void ThenASummaryDocumentIsGenerated()
    {
        throw new PendingStepException();
    }

    [Then(@"the summary contains the patient name ""(.*)"" and owner ""(.*)""")]
    public void ThenTheSummaryContainsThePatientNameAndOwner(string patientName, string ownerName)
    {
        throw new PendingStepException();
    }

    [Then(@"the summary includes the vaccination for ""(.*)"" on ""(.*)""")]
    public void ThenTheSummaryIncludesTheVaccinationForOn(string vaccine, string date)
    {
        throw new PendingStepException();
    }

    [Then(@"the summary includes the prescription for ""(.*)""")]
    public void ThenTheSummaryIncludesThePrescriptionFor(string medication)
    {
        throw new PendingStepException();
    }

    [Then(@"the summary does not contain internal notes")]
    public void ThenTheSummaryDoesNotContainInternalNotes()
    {
        throw new PendingStepException();
    }

    [Then(@"the summary does not include the text ""(.*)""")]
    public void ThenTheSummaryDoesNotIncludeTheText(string text)
    {
        throw new PendingStepException();
    }
}
