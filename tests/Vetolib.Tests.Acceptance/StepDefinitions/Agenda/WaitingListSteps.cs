using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Agenda;

[Binding]
[Scope(Feature = "Appointment waiting list")]
internal class WaitingListSteps
{
    private readonly ScenarioContext _ctx;

    public WaitingListSteps(ScenarioContext ctx)
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

    [Given(@"all slots for ""(.*)"" on ""(.*)"" are booked")]
    public void GivenAllSlotsForOnAreBooked(string vetName, string date)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" is on the waiting list for ""(.*)"" on ""(.*)""")]
    public void GivenIsOnTheWaitingListForOn(string animalName, string vetName, string date)
    {
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" was added to the waiting list for ""(.*)"" on ""(.*)"" (.*) hours ago")]
    public void GivenWasAddedToTheWaitingListForOnHoursAgo(string animalName, string vetName, string date, int hoursAgo)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I add ""(.*)"" to the waiting list for ""(.*)"" on ""(.*)""")]
    public void WhenIAddToTheWaitingListForOn(string animalName, string vetName, string date)
    {
        throw new PendingStepException();
    }

    [When(@"an appointment for ""(.*)"" on ""(.*)"" is cancelled")]
    public void WhenAnAppointmentForOnIsCancelled(string vetName, string date)
    {
        throw new PendingStepException();
    }

    [When(@"I remove ""(.*)"" from the waiting list")]
    public void WhenIRemoveFromTheWaitingList(string animalName)
    {
        throw new PendingStepException();
    }

    [When(@"the system processes expired waitlist entries")]
    public void WhenTheSystemProcessesExpiredWaitlistEntries()
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"""(.*)"" appears on the waiting list for ""(.*)"" on ""(.*)""")]
    public void ThenAppearsOnTheWaitingListForOn(string animalName, string vetName, string date)
    {
        throw new PendingStepException();
    }

    [Then(@"the waitlist entry has status ""(.*)""")]
    public void ThenTheWaitlistEntryHasStatus(string status)
    {
        throw new PendingStepException();
    }

    [Then(@"a notification is sent to the owner of ""(.*)"" about the available slot")]
    public void ThenANotificationIsSentToTheOwnerOfAboutTheAvailableSlot(string animalName)
    {
        throw new PendingStepException();
    }

    [Then(@"""(.*)"" no longer appears on the waiting list for ""(.*)"" on ""(.*)""")]
    public void ThenNoLongerAppearsOnTheWaitingListForOn(string animalName, string vetName, string date)
    {
        throw new PendingStepException();
    }

    [Then(@"the waitlist entry for ""(.*)"" is marked as ""(.*)""")]
    public void ThenTheWaitlistEntryForIsMarkedAs(string animalName, string status)
    {
        throw new PendingStepException();
    }

    [Then(@"""(.*)"" no longer appears on the active waiting list")]
    public void ThenNoLongerAppearsOnTheActiveWaitingList(string animalName)
    {
        throw new PendingStepException();
    }
}
