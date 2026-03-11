using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Billing;

[Binding]
[Scope(Feature = "Veterinary invoicing")]
internal class FacturationSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private Guid _clinicId;
    private Guid _animalId;
    private InvoiceDto? _currentInvoice;
    private HttpResponseMessage? _lastResponse;
    private string? _errorResponseBody;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FacturationSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN ──────────────────────────────────────────────────

    [Given(@"a clinic ""(.*)""")]
    public void GivenAClinic(string clinicName)
    {
        _clinicId = GenerateGuidFromString(clinicName);
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    [Given(@"an animal ""(.*)"" in the clinic")]
    public void GivenAnAnimalInTheClinic(string animalName)
    {
        _animalId = GenerateGuidFromString(animalName);
    }

    [Given(@"I am authenticated as VET")]
    public async Task GivenIAmAuthenticatedAsVet()
    {
        // Create a VET user and login
        var email = "vet@happypaws.ae";
        var password = "VetPass123!";

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userResult = User.Create(_clinicId, email, password, UserRole.Vet, "VET-001");
        userResult.IsSuccess.Should().BeTrue();
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"a ""(.*)"" invoice for ""(.*)""")]
    public async Task GivenAnInvoiceForAnimal(string status, string animalName)
    {
        // Create a DRAFT invoice first
        var createRequest = new CreateInvoiceRequest(
            _animalId,
            "Consultation",
            200m);

        var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
        response.EnsureSuccessStatusCode();
        _currentInvoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        _currentInvoice.Should().NotBeNull();

        // Transition to the desired status
        if (status == "SENT" || status == "PAID")
        {
            var sentResponse = await _client.PatchAsJsonAsync(
                $"/api/v1/invoices/{_currentInvoice!.Id}/status",
                new UpdateInvoiceStatusRequest(InvoiceStatus.Sent));
            sentResponse.EnsureSuccessStatusCode();
            _currentInvoice = await sentResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }

        if (status == "PAID")
        {
            var paidResponse = await _client.PatchAsJsonAsync(
                $"/api/v1/invoices/{_currentInvoice!.Id}/status",
                new UpdateInvoiceStatusRequest(InvoiceStatus.Paid));
            paidResponse.EnsureSuccessStatusCode();
            _currentInvoice = await paidResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
    }

    [Given(@"a ""SENT"" invoice for ""(.*)"" with at least one item")]
    public async Task GivenASentInvoiceWithAtLeastOneItem(string animalName)
    {
        // Create a DRAFT invoice first
        var createRequest = new CreateInvoiceRequest(
            _animalId,
            "Consultation",
            200m);

        var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
        response.EnsureSuccessStatusCode();
        _currentInvoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        _currentInvoice.Should().NotBeNull();

        // Transition to SENT
        var sentResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/status",
            new UpdateInvoiceStatusRequest(InvoiceStatus.Sent));
        sentResponse.EnsureSuccessStatusCode();
        _currentInvoice = await sentResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
    }

    [Given(@"a ""DRAFT"" invoice for ""(.*)"" with at least one item")]
    public async Task GivenADraftInvoiceWithAtLeastOneItem(string animalName)
    {
        var createRequest = new CreateInvoiceRequest(
            _animalId,
            "Consultation",
            200m);

        var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
        response.EnsureSuccessStatusCode();
        _currentInvoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        _currentInvoice.Should().NotBeNull();
    }

    [Given(@"(\d+) existing invoices for ""(.*)""")]
    public async Task GivenExistingInvoicesFor(int count, string clinicName)
    {
        for (int i = 0; i < count; i++)
        {
            var createRequest = new CreateInvoiceRequest(
                _animalId,
                $"Service {i + 1}",
                100m);

            var response = await _client.PostAsJsonAsync("/api/v1/invoices", createRequest);
            response.EnsureSuccessStatusCode();
        }
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"I create an invoice for ""(.*)"" with item ""(.*)"" at (\d+) AED")]
    public async Task WhenICreateAnInvoice(string animalName, string itemDescription, decimal price)
    {
        var request = new CreateInvoiceRequest(_animalId, itemDescription, price);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/invoices", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I add item ""(.*)"" at (\d+) AED")]
    public async Task WhenIAddItem(string description, decimal price)
    {
        var request = new AddInvoiceItemRequest(description, price);
        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/items", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I change the invoice status to ""(.*)""")]
    public async Task WhenIChangeTheInvoiceStatusTo(string status)
    {
        var newStatus = Enum.Parse<InvoiceStatus>(status, ignoreCase: true);
        _lastResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/status",
            new UpdateInvoiceStatusRequest(newStatus));

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I mark the invoice as ""(.*)""")]
    public async Task WhenIMarkTheInvoiceAs(string status)
    {
        var newStatus = Enum.Parse<InvoiceStatus>(status, ignoreCase: true);
        _lastResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/status",
            new UpdateInvoiceStatusRequest(newStatus));

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I attempt to add an item to the invoice")]
    public async Task WhenIAttemptToAddAnItem()
    {
        var request = new AddInvoiceItemRequest("Extra Service", 50m);
        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/v1/invoices/{_currentInvoice!.Id}/items", request);
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    [When(@"I create a new invoice")]
    public async Task WhenICreateANewInvoice()
    {
        var request = new CreateInvoiceRequest(_animalId, "Service", 100m);
        _lastResponse = await _client.PostAsJsonAsync("/api/v1/invoices", request);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _currentInvoice = await _lastResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I download the PDF of this invoice")]
    public async Task WhenIDownloadThePdfOfThisInvoice()
    {
        _lastResponse = await _client.GetAsync($"/api/v1/invoices/{_currentInvoice!.Id}/pdf");
        if (!_lastResponse.IsSuccessStatusCode)
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I download the PDF of an invoice with a random non-existent ID")]
    public async Task WhenIDownloadThePdfOfANonExistentInvoice()
    {
        var randomId = Guid.NewGuid();
        _lastResponse = await _client.GetAsync($"/api/v1/invoices/{randomId}/pdf");
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"the invoice is created with status ""(.*)""")]
    public void ThenTheInvoiceIsCreatedWithStatus(string status)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Status.ToString().ToUpper().Should().Be(status);
    }

    [Then(@"the number matches format ""(.*)""")]
    public void ThenTheNumberMatchesFormat(string expectedPattern)
    {
        _currentInvoice.Should().NotBeNull();
        // The pattern is like "INV-2026-001", verify the prefix matches
        _currentInvoice!.InvoiceNumber.Should().MatchRegex(@"^INV-\d{4}-\d{3}$");
    }

    [Then(@"the 5% VAT is calculated automatically \((\d+) AED\)")]
    public void ThenTheVatIsCalculated(decimal expectedTax)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.VatAmount.Should().Be(expectedTax);
    }

    [Then(@"the total is ([\d.]+) AED")]
    public void ThenTheTotalIs(decimal expectedTotal)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Total.Should().Be(expectedTotal);
    }

    [Then(@"the invoice contains (\d+) items")]
    public void ThenTheInvoiceContainsItems(int count)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Items.Should().HaveCount(count);
    }

    [Then(@"the subtotal is ([\d.]+) AED")]
    public void ThenTheSubtotalIs(decimal expectedSubTotal)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Subtotal.Should().Be(expectedSubTotal);
    }

    [Then(@"the total VAT is ([\d.]+) AED")]
    public void ThenTheTotalVatIs(decimal expectedTax)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.VatAmount.Should().Be(expectedTax);
    }

    [Then(@"the status is ""(.*)""")]
    public void ThenTheStatusIs(string status)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.Status.ToString().ToUpper().Should().Be(status);
    }

    [Then(@"the due date is set to 30 days")]
    public void ThenTheDueDateIsSetTo30Days()
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.DueDate.Should().NotBeNull();
        var expectedDate = DateTime.UtcNow.AddDays(30);
        _currentInvoice.DueDate!.Value.Should().BeCloseTo(expectedDate, TimeSpan.FromMinutes(5));
    }

    [Then(@"the system rejects with code ""(.*)""")]
    public void ThenTheSystemRejectsWithCode(string errorCode)
    {
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNull();
        _errorResponseBody.Should().Contain(errorCode);
    }

    [Then(@"the error message is ""(.*)""")]
    public void ThenTheErrorMessageIs(string expectedMessage)
    {
        _errorResponseBody.Should().Contain(expectedMessage);
    }

    [Then(@"the number is ""(.*)""")]
    public void ThenTheNumberIs(string expectedNumber)
    {
        _currentInvoice.Should().NotBeNull();
        _currentInvoice!.InvoiceNumber.Should().Be(expectedNumber);
    }

    [Then(@"the response has status (\d+)")]
    public void ThenTheResponseHasStatus(int statusCode)
    {
        _lastResponse.Should().NotBeNull();
        ((int)_lastResponse!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"the Content-Type is ""(.*)""")]
    public void ThenTheContentTypeIs(string expectedContentType)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.Content.Headers.ContentType?.MediaType.Should().Be(expectedContentType);
    }

    [Then(@"the content is not empty")]
    public async Task ThenTheContentIsNotEmpty()
    {
        _lastResponse.Should().NotBeNull();
        var bytes = await _lastResponse!.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    // ─── Helpers ────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
