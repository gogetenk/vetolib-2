using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.OwnerPortalLogin;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class OwnerPortalLoginHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid FixedClinicId2 = new("22222222-2222-2222-2222-222222222222");

    private readonly IOwnerPortalJwtService _jwtService = Substitute.For<IOwnerPortalJwtService>();
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
    public async Task Handle_ValidCredentials_ReturnsTokenWithLinkedClinics()
    {
        using var context = BuildContext();
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(account);
        await context.SaveChangesAsync();

        var linkedClinics = new[] { FixedClinicId, FixedClinicId2 };
        _ownerAccountLinker.GetLinkedClinicIdsAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(linkedClinics);
        _jwtService.GenerateOwnerPortalToken(Arg.Any<OwnerAccount>(), Arg.Any<Guid[]>())
            .Returns("owner-portal-jwt");
        _jwtService.GenerateRefreshToken().Returns("refresh-token");

        var handler = new OwnerPortalLoginHandler(context, _jwtService, _ownerAccountLinker);
        var command = new OwnerPortalLoginCommand("fatima@example.com", "SecureP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("owner-portal-jwt");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.Account.Email.Should().Be("fatima@example.com");
        result.Value.LinkedClinicIds.Should().BeEquivalentTo(linkedClinics);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsError()
    {
        using var context = BuildContext();
        var handler = new OwnerPortalLoginHandler(context, _jwtService, _ownerAccountLinker);
        var command = new OwnerPortalLoginCommand("unknown@example.com", "SecureP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_CREDENTIALS"));
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsError()
    {
        using var context = BuildContext();
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new OwnerPortalLoginHandler(context, _jwtService, _ownerAccountLinker);
        var command = new OwnerPortalLoginCommand("fatima@example.com", "WrongP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_CREDENTIALS"));
    }

    [Fact]
    public async Task Handle_EmailIsCaseInsensitive()
    {
        using var context = BuildContext();
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(account);
        await context.SaveChangesAsync();

        _ownerAccountLinker.GetLinkedClinicIdsAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());
        _jwtService.GenerateOwnerPortalToken(Arg.Any<OwnerAccount>(), Arg.Any<Guid[]>())
            .Returns("jwt");
        _jwtService.GenerateRefreshToken().Returns("refresh");

        var handler = new OwnerPortalLoginHandler(context, _jwtService, _ownerAccountLinker);
        var command = new OwnerPortalLoginCommand("FATIMA@EXAMPLE.COM", "SecureP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
