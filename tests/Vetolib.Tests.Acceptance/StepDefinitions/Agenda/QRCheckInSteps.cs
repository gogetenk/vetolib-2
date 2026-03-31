using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Agenda;

[Binding]
[Scope(Feature = "QR code-based appointment check-in")]
internal class QRCheckInSteps
{
    private readonly ScenarioContext _ctx;

    public QRCheckInSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"a clinic ""(.*)"" with hours 9am-6pm")]
    public void GivenAClinicWithHours(string clinicName)
    {
        throw new PendingStepException();
    }

    [Given(@"a veterinarian ""(.*)"" with license ""(.*)""")]
    public void GivenAVeterinarianWithLicense(string vetName, string license)
    {
        throw new PendingStepException();
    }

    [Given(@"an animal ""(.*)"" breed ""(.*)"" belonging to ""(.*)""")]
    public void GivenAnAnimalBreedBelongingTo(string animalName, string breed, string ownerName)
    {
        throw new PendingStepException();
    }

    [Given(@"a scheduled appointment for ""(.*)"" with ""(.*)"" on ""(.*)"" at ""(.*)""")]
    public void GivenAScheduledAppointmentForWithOnAt(string animalName, string vetName, string date, string time)
    {
        throw new PendingStepException();
    }

    [Given(@"a QR code has been generated for this appointment")]
    public void GivenAQRCodeHasBeenGeneratedForThisAppointment()
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I request a QR code for this appointment")]
    public void WhenIRequestAQRCodeForThisAppointment()
    {
        throw new PendingStepException();
    }

    [When(@"the owner scans the QR code within the allowed time window")]
    public void WhenTheOwnerScansTheQRCodeWithinTheAllowedTimeWindow()
    {
        throw new PendingStepException();
    }

    [When(@"the owner scans the QR code more than 2 hours before the appointment")]
    public void WhenTheOwnerScansTheQRCodeMoreThan2HoursBeforeTheAppointment()
    {
        throw new PendingStepException();
    }

    [When(@"someone scans a QR code with a tampered signature")]
    public void WhenSomeoneScansAQRCodeWithATamperedSignature()
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"a unique QR code is generated")]
    public void ThenAUniqueQRCodeIsGenerated()
    {
        throw new PendingStepException();
    }

    [Then(@"the QR code is linked to the appointment")]
    public void ThenTheQRCodeIsLinkedToTheAppointment()
    {
        throw new PendingStepException();
    }

    [Then(@"the appointment status changes to ""(.*)""")]
    public void ThenTheAppointmentStatusChangesTo(string status)
    {
        throw new PendingStepException();
    }

    [Then(@"the clinic is notified that ""(.*)"" has arrived")]
    public void ThenTheClinicIsNotifiedThatHasArrived(string animalName)
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates the check-in window is not yet open")]
    public void ThenTheMessageIndicatesTheCheckInWindowIsNotYetOpen()
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates the QR code is not valid")]
    public void ThenTheMessageIndicatesTheQRCodeIsNotValid()
    {
        throw new PendingStepException();
    }
}
