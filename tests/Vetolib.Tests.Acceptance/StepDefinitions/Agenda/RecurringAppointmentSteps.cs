using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Agenda;

[Binding]
[Scope(Feature = "Recurring appointment series")]
internal class RecurringAppointmentSteps
{
    private readonly ScenarioContext _ctx;

    public RecurringAppointmentSteps(ScenarioContext ctx)
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

    [Given(@"a weekly recurring series for ""(.*)"" starting ""(.*)"" with (.*) appointments")]
    public void GivenAWeeklyRecurringSeriesForStartingWithAppointments(string animalName, string startDate, int count)
    {
        throw new PendingStepException();
    }

    [Given(@"the appointment on ""(.*)"" has been completed")]
    public void GivenTheAppointmentOnHasBeenCompleted(string date)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I create a weekly recurring appointment for ""(.*)"" with ""(.*)"" starting ""(.*)"" at ""(.*)"" for (.*) minutes repeating (.*) times")]
    public void WhenICreateAWeeklyRecurringAppointment(string animalName, string vetName, string startDate, string startTime, int duration, int repeatCount)
    {
        throw new PendingStepException();
    }

    [When(@"I create a monthly recurring appointment for ""(.*)"" with ""(.*)"" starting ""(.*)"" at ""(.*)"" for (.*) minutes repeating (.*) times")]
    public void WhenICreateAMonthlyRecurringAppointment(string animalName, string vetName, string startDate, string startTime, int duration, int repeatCount)
    {
        throw new PendingStepException();
    }

    [When(@"I cancel all future appointments in the series")]
    public void WhenICancelAllFutureAppointmentsInTheSeries()
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"(.*) appointments are created")]
    public void ThenAppointmentsAreCreated(int count)
    {
        throw new PendingStepException();
    }

    [Then(@"the appointments are scheduled on ""(.*)"", ""(.*)"", ""(.*)"", and ""(.*)""")]
    public void ThenTheAppointmentsAreScheduledOnFourDates(string date1, string date2, string date3, string date4)
    {
        throw new PendingStepException();
    }

    [Then(@"the appointments are scheduled on ""(.*)"", ""(.*)"", and ""(.*)""")]
    public void ThenTheAppointmentsAreScheduledOnThreeDates(string date1, string date2, string date3)
    {
        throw new PendingStepException();
    }

    [Then(@"all appointments have status ""(.*)""")]
    public void ThenAllAppointmentsHaveStatus(string status)
    {
        throw new PendingStepException();
    }

    [Then(@"all appointments belong to the same series")]
    public void ThenAllAppointmentsBelongToTheSameSeries()
    {
        throw new PendingStepException();
    }

    [Then(@"the appointments on ""(.*)"", ""(.*)"", and ""(.*)"" are cancelled")]
    public void ThenTheAppointmentsOnAreCancelled(string date1, string date2, string date3)
    {
        throw new PendingStepException();
    }

    [Then(@"the completed appointment on ""(.*)"" remains unchanged")]
    public void ThenTheCompletedAppointmentOnRemainsUnchanged(string date)
    {
        throw new PendingStepException();
    }

    [Then(@"(.*) appointments remain with status ""(.*)""")]
    public void ThenAppointmentsRemainWithStatus(int count, string status)
    {
        throw new PendingStepException();
    }

    [Then(@"(.*) appointments are cancelled")]
    public void ThenAppointmentsAreCancelled(int count)
    {
        throw new PendingStepException();
    }
}
