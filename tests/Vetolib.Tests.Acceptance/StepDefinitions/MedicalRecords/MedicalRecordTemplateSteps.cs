using Reqnroll;

namespace Vetolib.Tests.Acceptance.StepDefinitions.MedicalRecords;

[Binding]
[Scope(Feature = "Medical record templates")]
internal class MedicalRecordTemplateSteps
{
    private readonly ScenarioContext _ctx;

    public MedicalRecordTemplateSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"a system template ""(.*)""")]
    public void GivenASystemTemplate(string templateName)
    {
        throw new PendingStepException();
    }

    [Given(@"a custom template ""(.*)"" belonging to ""(.*)""")]
    public void GivenACustomTemplateBelongingTo(string templateName, string clinicName)
    {
        throw new PendingStepException();
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I list the available medical record templates")]
    public void WhenIListTheAvailableMedicalRecordTemplates()
    {
        throw new PendingStepException();
    }

    [When(@"I create a custom template named ""(.*)"" with the following sections:")]
    public void WhenICreateACustomTemplateNamedWithTheFollowingSections(string templateName, Table table)
    {
        throw new PendingStepException();
    }

    [When(@"I attempt to modify the system template ""(.*)""")]
    public void WhenIAttemptToModifyTheSystemTemplate(string templateName)
    {
        throw new PendingStepException();
    }

    [When(@"I delete the template ""(.*)""")]
    public void WhenIDeleteTheTemplate(string templateName)
    {
        throw new PendingStepException();
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"I see system templates including ""(.*)"" and ""(.*)""")]
    public void ThenISeeSystemTemplatesIncludingAnd(string template1, string template2)
    {
        throw new PendingStepException();
    }

    [Then(@"each template has a name, category, and content structure")]
    public void ThenEachTemplateHasANameCategoryAndContentStructure()
    {
        throw new PendingStepException();
    }

    [Then(@"the template ""(.*)"" is created")]
    public void ThenTheTemplateIsCreated(string templateName)
    {
        throw new PendingStepException();
    }

    [Then(@"it is marked as a custom template for ""(.*)""")]
    public void ThenItIsMarkedAsACustomTemplateFor(string clinicName)
    {
        throw new PendingStepException();
    }

    [Then(@"the message indicates that system templates cannot be modified")]
    public void ThenTheMessageIndicatesThatSystemTemplatesCannotBeModified()
    {
        throw new PendingStepException();
    }

    [Then(@"the template is removed from the clinic's template list")]
    public void ThenTheTemplateIsRemovedFromTheClinicsTemplateList()
    {
        throw new PendingStepException();
    }

    [Then(@"existing records using this template are not affected")]
    public void ThenExistingRecordsUsingThisTemplateAreNotAffected()
    {
        throw new PendingStepException();
    }
}
