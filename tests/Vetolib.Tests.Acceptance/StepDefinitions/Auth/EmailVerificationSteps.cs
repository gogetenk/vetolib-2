using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Auth;

[Binding]
[Scope(Feature = "Email verification after registration")]
internal class EmailVerificationSteps
{
    private readonly ScenarioContext _ctx;

    public EmailVerificationSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"a user registered with email ""(.*)"" and password ""(.*)""")]
    public void GivenAUserRegisteredWithEmailAndPassword(string email, string password)
    {
        throw new PendingStepException();
    }

    [Given(@"the user ""(.*)"" has a pending verification token")]
    public void GivenTheUserHasAPendingVerificationToken(string email)
    {
        throw new PendingStepException();
    }

    [Given(@"the user ""(.*)"" has a verification token that expired 25 hours ago")]
    public void GivenTheUserHasAVerificationTokenThatExpired25HoursAgo(string email)
    {
        throw new PendingStepException();
    }

    [Given(@"the user ""(.*)"" has already verified their email")]
    public void GivenTheUserHasAlreadyVerifiedTheirEmail(string email)
    {
        throw new PendingStepException();
    }

    [Given(@"the user ""(.*)"" has not verified their email")]
    public void GivenTheUserHasNotVerifiedTheirEmail(string email)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"the user verifies their email with the valid token")]
    public void WhenTheUserVerifiesTheirEmailWithTheValidToken()
    {
        throw new PendingStepException();
    }

    [When(@"the user attempts to verify their email with the expired token")]
    public void WhenTheUserAttemptsToVerifyTheirEmailWithTheExpiredToken()
    {
        throw new PendingStepException();
    }

    [When(@"the user attempts to verify their email again")]
    public void WhenTheUserAttemptsToVerifyTheirEmailAgain()
    {
        throw new PendingStepException();
    }

    [When(@"I log in with email ""(.*)"" and password ""(.*)""")]
    public void WhenILogInWithEmailAndPassword(string email, string password)
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"the email is marked as verified")]
    public void ThenTheEmailIsMarkedAsVerified()
    {
        throw new PendingStepException();
    }

    [Then(@"the user can access all clinic features")]
    public void ThenTheUserCanAccessAllClinicFeatures()
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates the verification link has expired")]
    public void ThenTheMessageIndicatesTheVerificationLinkHasExpired()
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates the email is already verified")]
    public void ThenTheMessageIndicatesTheEmailIsAlreadyVerified()
    {
        throw new PendingStepException();
    }

    [Then(@"I am successfully authenticated")]
    public void ThenIAmSuccessfullyAuthenticated()
    {
        throw new PendingStepException();
    }

    [Then(@"the response includes a warning that the email is unverified")]
    public void ThenTheResponseIncludesAWarningThatTheEmailIsUnverified()
    {
        throw new PendingStepException();
    }
}
