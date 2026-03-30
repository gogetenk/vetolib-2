using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.AcceptConsent;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class AcceptConsentHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly MessagingDbContext _context;
    private readonly AcceptConsentHandler _sut;

    public AcceptConsentHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new AcceptConsentHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenTokenExists_RecordsConsentAndReturnsSuccess()
    {
        var token = OwnerPortalToken.Create(ClinicId, OwnerId, "test-token", DateTime.UtcNow.AddDays(30)).Value;
        _context.OwnerPortalTokens.Add(token);
        await _context.SaveChangesAsync();

        var cmd = new AcceptConsentCommand(OwnerId, ClinicId, "v1.0");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _context.OwnerPortalTokens.FirstAsync();
        updated.ConsentVersion.Should().Be("v1.0");
        updated.ConsentAcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ReturnsNotFound()
    {
        var cmd = new AcceptConsentCommand(Guid.NewGuid(), ClinicId, "v1.0");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WithEmptyConsentVersion_ReturnsError()
    {
        var token = OwnerPortalToken.Create(ClinicId, OwnerId, "test-token", DateTime.UtcNow.AddDays(30)).Value;
        _context.OwnerPortalTokens.Add(token);
        await _context.SaveChangesAsync();

        var cmd = new AcceptConsentCommand(OwnerId, ClinicId, "");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    public void Dispose() => _context.Dispose();
}
