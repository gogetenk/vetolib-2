using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.UpdateWhatsAppConfig;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class UpdateWhatsAppConfigHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly ITokenEncryptor _tokenEncryptor;
    private readonly UpdateWhatsAppConfigHandler _handler;

    public UpdateWhatsAppConfigHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();
        _tokenEncryptor = Substitute.For<ITokenEncryptor>();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, publisher);
        _handler = new UpdateWhatsAppConfigHandler(_context, clinicContext, _tokenEncryptor);
    }

    [Fact]
    public async Task Handle_WhenNoExistingConfig_CreatesNewWaba()
    {
        _tokenEncryptor.Encrypt("my_access_token").Returns("encrypted_token");

        var cmd = new UpdateWhatsAppConfigCommand("waba_123", "phone_123", "my_access_token");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.WabaId.Should().Be("waba_123");
        result.Value.PhoneNumberId.Should().Be("phone_123");
        result.Value.HasAccessToken.Should().BeTrue();

        var saved = await _context.WhatsAppBusinessAccounts.FirstAsync();
        saved.EncryptedAccessToken.Should().Be("encrypted_token");
    }

    [Fact]
    public async Task Handle_WhenExistingConfig_UpdatesWaba()
    {
        // Seed existing
        _tokenEncryptor.Encrypt("old_token").Returns("encrypted_old");
        var createResult = WhatsAppBusinessAccount.Create(ClinicId, "old_waba", "old_phone", "encrypted_old");
        _context.WhatsAppBusinessAccounts.Add(createResult.Value);
        await _context.SaveChangesAsync();

        _tokenEncryptor.Encrypt("new_access_token").Returns("encrypted_new");

        var cmd = new UpdateWhatsAppConfigCommand("new_waba", "new_phone", "new_access_token");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.WabaId.Should().Be("new_waba");
        result.Value.PhoneNumberId.Should().Be("new_phone");

        var count = await _context.WhatsAppBusinessAccounts.CountAsync();
        count.Should().Be(1, "should update existing, not create duplicate");
    }

    public void Dispose() => _context.Dispose();
}
