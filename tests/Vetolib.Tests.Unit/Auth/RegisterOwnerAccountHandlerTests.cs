using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.RegisterOwnerAccount;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RegisterOwnerAccountHandlerTests
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
    public async Task Handle_ValidRequest_CreatesAccountAndAutoLinks()
    {
        using var context = BuildContext();
        _ownerAccountLinker.LinkOwnersByEmailOrPhoneAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { FixedClinicId });

        var handler = new RegisterOwnerAccountHandler(context, _ownerAccountLinker);
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "Fatima Al Rashid", "SecureP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("fatima@example.com");
        result.Value.FullName.Should().Be("Fatima Al Rashid");

        // Verify auto-link was called
        await _ownerAccountLinker.Received(1).LinkOwnersByEmailOrPhoneAsync(
            Arg.Any<Guid>(), "fatima@example.com", "+971501234567", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        using var context = BuildContext();
        var existing = OwnerAccount.Create("fatima@example.com", "+971509999999", "Existing User", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(existing);
        await context.SaveChangesAsync();

        var handler = new RegisterOwnerAccountHandler(context, _ownerAccountLinker);
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "Another User", "SecureP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain(e => e.Contains("EMAIL_TAKEN"));
    }

    [Fact]
    public async Task Handle_DuplicatePhone_ReturnsConflict()
    {
        using var context = BuildContext();
        var existing = OwnerAccount.Create("existing@example.com", "+971501234567", "Existing User", "SecureP@ss1").Value;
        context.OwnerAccounts.Add(existing);
        await context.SaveChangesAsync();

        var handler = new RegisterOwnerAccountHandler(context, _ownerAccountLinker);
        var command = new RegisterOwnerAccountCommand("new@example.com", "+971501234567", "New User", "SecureP@ss1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain(e => e.Contains("PHONE_TAKEN"));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsInvalid()
    {
        using var context = BuildContext();
        var handler = new RegisterOwnerAccountHandler(context, _ownerAccountLinker);
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "Fatima", "weak");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }
}
