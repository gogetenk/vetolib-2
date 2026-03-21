using System.Net;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class WhatsAppSenderTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly ITokenEncryptor _tokenEncryptor;
    private readonly ILogger<WhatsAppSender> _logger;

    public WhatsAppSenderTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, publisher);
        _tokenEncryptor = Substitute.For<ITokenEncryptor>();
        _logger = Substitute.For<ILogger<WhatsAppSender>>();
    }

    private WhatsAppSender CreateSender(HttpResponseMessage response)
    {
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("WhatsApp").Returns(httpClient);

        return new WhatsAppSender(factory, _context, _tokenEncryptor, _logger);
    }

    private async Task SeedWaba(string encryptedToken = "encrypted_token_123")
    {
        var wabaResult = Vetolib.Messaging.Application.Domain.WhatsAppBusinessAccount.Create(
            ClinicId, "waba_123", "phone_123", encryptedToken);
        wabaResult.IsSuccess.Should().BeTrue();
        _context.WhatsAppBusinessAccounts.Add(wabaResult.Value);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task SendAsync_WhenNoWabaConfigured_ReturnsNotFound()
    {
        var sender = CreateSender(new HttpResponseMessage(HttpStatusCode.OK));
        var message = new ChannelMessage("+971501234567", "hello_world", new(), ClinicId);

        var result = await sender.SendAsync(message);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task SendAsync_WhenWabaConfigured_SendsToCorrectUrl()
    {
        await SeedWaba();
        _tokenEncryptor.Decrypt("encrypted_token_123").Returns("plain_access_token");

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"messages\":[{\"id\":\"wamid.123\"}]}")
        };

        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("WhatsApp").Returns(httpClient);

        var sender = new WhatsAppSender(factory, _context, _tokenEncryptor, _logger);
        var message = new ChannelMessage("+971501234567", "appointment_reminder", new(), ClinicId);

        var result = await sender.SendAsync(message);

        result.IsSuccess.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString()
            .Should().Be("https://graph.facebook.com/v21.0/phone_123/messages");
        handler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be("plain_access_token");
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsError_ReturnsError()
    {
        await SeedWaba();
        _tokenEncryptor.Decrypt("encrypted_token_123").Returns("plain_access_token");

        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"error\":{\"message\":\"Invalid token\"}}")
        };

        var sender = CreateSender(response);
        var message = new ChannelMessage("+971501234567", "hello_world", new(), ClinicId);

        var result = await sender.SendAsync(message);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("WHATSAPP_API_ERROR"));
    }

    [Fact]
    public async Task SendAsync_WhenDecryptionFails_ReturnsError()
    {
        await SeedWaba();
        _tokenEncryptor.When(x => x.Decrypt(Arg.Any<string>()))
            .Do(_ => throw new InvalidOperationException("Decryption failed"));

        var sender = CreateSender(new HttpResponseMessage(HttpStatusCode.OK));
        var message = new ChannelMessage("+971501234567", "hello_world", new(), ClinicId);

        var result = await sender.SendAsync(message);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("DECRYPTION_FAILED"));
    }

    [Fact]
    public async Task SendAsync_WithParameters_IncludesTemplateComponents()
    {
        await SeedWaba();
        _tokenEncryptor.Decrypt("encrypted_token_123").Returns("plain_access_token");

        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"messages\":[{\"id\":\"wamid.123\"}]}")
        });
        var httpClient = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("WhatsApp").Returns(httpClient);

        var sender = new WhatsAppSender(factory, _context, _tokenEncryptor, _logger);
        var parameters = new Dictionary<string, string>
        {
            { "pet_name", "Buddy" },
            { "date", "2026-03-21" }
        };
        var message = new ChannelMessage("+971501234567", "appointment_reminder", parameters, ClinicId);

        var result = await sender.SendAsync(message);

        result.IsSuccess.Should().BeTrue();

        handler.LastRequestBody.Should().NotBeNull();
        handler.LastRequestBody.Should().Contain("Buddy");
        handler.LastRequestBody.Should().Contain("2026-03-21");
    }

    public void Dispose() => _context.Dispose();

    /// <summary>
    /// Fake HTTP handler that captures the request and returns a predetermined response.
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            return _response;
        }
    }
}
