using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Commands.RegisterOwnerAccount;

internal class RegisterOwnerAccountHandler : IRequestHandler<RegisterOwnerAccountCommand, Result<OwnerAccountDto>>
{
    private readonly AuthDbContext _context;
    private readonly IOwnerAccountLinker _ownerAccountLinker;

    public RegisterOwnerAccountHandler(AuthDbContext context, IOwnerAccountLinker ownerAccountLinker)
    {
        _context = context;
        _ownerAccountLinker = ownerAccountLinker;
    }

    public async Task<Result<OwnerAccountDto>> Handle(RegisterOwnerAccountCommand cmd, CancellationToken ct)
    {
        var normalizedEmail = cmd.Email.Trim().ToLowerInvariant();

        // Check if email already taken
        var emailExists = await _context.OwnerAccounts
            .AnyAsync(a => a.Email == normalizedEmail, ct);

        if (emailExists)
            return Result<OwnerAccountDto>.Conflict("EMAIL_TAKEN:An account with this email already exists");

        // Check if phone already taken
        var phoneExists = await _context.OwnerAccounts
            .AnyAsync(a => a.Phone == cmd.Phone.Trim(), ct);

        if (phoneExists)
            return Result<OwnerAccountDto>.Conflict("PHONE_TAKEN:An account with this phone number already exists");

        // Create the OwnerAccount
        var createResult = OwnerAccount.Create(cmd.Email, cmd.Phone, cmd.FullName, cmd.Password);
        if (!createResult.IsSuccess)
            return Result<OwnerAccountDto>.Invalid(createResult.ValidationErrors.ToList());

        var account = createResult.Value;
        _context.OwnerAccounts.Add(account);
        await _context.SaveChangesAsync(ct);

        // Auto-link owners by email/phone across all clinics
        await _ownerAccountLinker.LinkOwnersByEmailOrPhoneAsync(account.Id, account.Email, account.Phone, ct);

        return Result<OwnerAccountDto>.Success(account.ToDto());
    }
}
