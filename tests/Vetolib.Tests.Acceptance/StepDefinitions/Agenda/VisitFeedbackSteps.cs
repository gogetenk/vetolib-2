using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Agenda;

[Binding]
[Scope(Feature = "Post-visit feedback collection")]
internal class VisitFeedbackSteps
{
    private readonly ScenarioContext _ctx;

    public VisitFeedbackSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"an animal ""(.*)"" breed ""(.*)"" belonging to ""(.*)""")]
    public void GivenAnAnimalBreedBelongingTo(string animalName, string breed, string ownerName)
    {
        throw new PendingStepException();
    }

    [Given(@"a veterinarian ""(.*)"" with license ""(.*)""")]
    public void GivenAVeterinarianWithLicense(string vetName, string license)
    {
        throw new PendingStepException();
    }

    [Given(@"a completed appointment for ""(.*)"" with ""(.*)""")]
    public void GivenACompletedAppointmentForWith(string animalName, string vetName)
    {
        throw new PendingStepException();
    }

    [Given(@"a scheduled appointment for ""(.*)"" with ""(.*)""")]
    public void GivenAScheduledAppointmentForWith(string animalName, string vetName)
    {
        throw new PendingStepException();
    }

    [Given(@"the owner has already submitted feedback for that appointment")]
    public void GivenTheOwnerHasAlreadySubmittedFeedbackForThatAppointment()
    {
        throw new PendingStepException();
    }

    [Given(@"(.*) completed appointments with feedback ratings: (.*)")]
    public void GivenCompletedAppointmentsWithFeedbackRatings(int count, string ratings)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"the owner of ""(.*)"" submits feedback with a rating of (.*) out of 10 and comment ""(.*)""")]
    public void WhenTheOwnerOfSubmitsFeedbackWithRatingAndComment(string animalName, int rating, string comment)
    {
        throw new PendingStepException();
    }

    [When(@"the owner of ""(.*)"" attempts to submit feedback for that appointment")]
    public void WhenTheOwnerOfAttemptsToSubmitFeedbackForThatAppointment(string animalName)
    {
        throw new PendingStepException();
    }

    [When(@"the owner of ""(.*)"" attempts to submit feedback again")]
    public void WhenTheOwnerOfAttemptsToSubmitFeedbackAgain(string animalName)
    {
        throw new PendingStepException();
    }

    [When(@"I view the feedback statistics for ""(.*)""")]
    public void WhenIViewTheFeedbackStatisticsFor(string clinicName)
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"the feedback is recorded for that appointment")]
    public void ThenTheFeedbackIsRecordedForThatAppointment()
    {
        throw new PendingStepException();
    }

    [Then(@"the feedback shows rating (.*) and comment ""(.*)""")]
    public void ThenTheFeedbackShowsRatingAndComment(int rating, string comment)
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates feedback can only be submitted after a completed visit")]
    public void ThenTheMessageIndicatesFeedbackCanOnlyBeSubmittedAfterACompletedVisit()
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates that feedback was already provided for this visit")]
    public void ThenTheMessageIndicatesThatFeedbackWasAlreadyProvidedForThisVisit()
    {
        throw new PendingStepException();
    }

    [Then(@"the average rating is (.*)")]
    public void ThenTheAverageRatingIs(decimal averageRating)
    {
        throw new PendingStepException();
    }

    [Then(@"the NPS score is calculated based on promoters, passives, and detractors")]
    public void ThenTheNpsScoreIsCalculatedBasedOnPromotersPassivesAndDetractors()
    {
        throw new PendingStepException();
    }
}
