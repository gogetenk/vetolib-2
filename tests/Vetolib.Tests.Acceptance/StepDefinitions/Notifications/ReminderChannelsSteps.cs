using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Notifications;

[Binding]
[Scope(Feature = "Appointment reminder channel preferences")]
internal class ReminderChannelsSteps
{
    private readonly ScenarioContext _ctx;

    public ReminderChannelsSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"a newly created clinic ""(.*)""")]
    public void GivenANewlyCreatedClinic(string clinicName)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I set the reminder channel to ""(.*)"" only")]
    public void WhenISetTheReminderChannelToOnly(string channel)
    {
        throw new PendingStepException();
    }

    [When(@"I set the reminder channels to ""(.*)"" and ""(.*)""")]
    public void WhenISetTheReminderChannelsToAnd(string channel1, string channel2)
    {
        throw new PendingStepException();
    }

    [When(@"I view the reminder channel settings for ""(.*)""")]
    public void WhenIViewTheReminderChannelSettingsFor(string clinicName)
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"the clinic reminder settings show ""(.*)"" as the active channel")]
    public void ThenTheClinicReminderSettingsShowAsTheActiveChannel(string channel)
    {
        throw new PendingStepException();
    }

    [Then(@"""(.*)"" is not an active channel")]
    public void ThenIsNotAnActiveChannel(string channel)
    {
        throw new PendingStepException();
    }

    [Then(@"the clinic reminder settings show both ""(.*)"" and ""(.*)"" as active channels")]
    public void ThenTheClinicReminderSettingsShowBothAsActiveChannels(string channel1, string channel2)
    {
        throw new PendingStepException();
    }

    [Then(@"the default active channel is ""(.*)""")]
    public void ThenTheDefaultActiveChannelIs(string channel)
    {
        throw new PendingStepException();
    }
}
