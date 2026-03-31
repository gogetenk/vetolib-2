using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.LinkOwnerByMicrochip;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class LinkOwnerByMicrochipHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IOwnerAccountLinker _ownerAccountLinker = Substitute.For<IOwnerAccountLinker>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    [Fact]
    public async Task Handle_ValidMicrochip_LinksOwner()
    {
        using var context = BuildContext();
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(account);
        await context.SaveChangesAsync();

        _ownerAccountLinker.LinkOwnerByMicrochipAsync(account.Id, "123456789012345", Arg.Any<CancellationToken>())
            .Returns(FixedClinicId);

        var handler = new LinkOwnerByMicrochipHandler(context, _ownerAccountLinker);
        var command = new LinkOwnerByMicrochipCommand(account.Id, "123456789012345");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AccountNotFound_ReturnsNotFound()
    {
        using var context = BuildContext();
        var handler = new LinkOwnerByMicrochipHandler(context, _ownerAccountLinker);
        var command = new LinkOwnerByMicrochipCommand(Guid.NewGuid(), "123456789012345");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_MicrochipNotFound_ReturnsNotFound()
    {
        using var context = BuildContext();
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(account);
        await context.SaveChangesAsync();

        _ownerAccountLinker.LinkOwnerByMicrochipAsync(account.Id, "999999999999999", Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var handler = new LinkOwnerByMicrochipHandler(context, _ownerAccountLinker);
        var command = new LinkOwnerByMicrochipCommand(account.Id, "999999999999999");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
