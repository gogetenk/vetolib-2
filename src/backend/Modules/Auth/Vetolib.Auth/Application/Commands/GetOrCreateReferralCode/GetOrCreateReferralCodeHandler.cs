using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.GetOrCreateReferralCode;

internal class GetOrCreateReferralCodeHandler : IRequestHandler<GetOrCreateReferralCodeCommand, Result<ReferralCodeDto>>
{
    private readonly AuthDbContext _context;

    public GetOrCreateReferralCodeHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReferralCodeDto>> Handle(GetOrCreateReferralCodeCommand cmd, CancellationToken ct)
    {
        // Check if user exists
        var userExists = await _context.Users
            .IgnoreQueryFilters() // Referral codes are global, not per-clinic
            .AnyAsync(u => u.Id == cmd.UserId, ct);

        if (!userExists)
            return Result<ReferralCodeDto>.NotFound("USER_NOT_FOUND");

        // Try to find existing referral code
        var existing = await _context.ReferralCodes
            .FirstOrDefaultAsync(r => r.OwnerUserId == cmd.UserId, ct);

        if (existing is not null)
            return Result<ReferralCodeDto>.Success(ToDto(existing));

        // Create a new referral code
        var code = ReferralCode.GenerateCode();

        // Ensure uniqueness (retry if collision)
        var attempts = 0;
        while (await _context.ReferralCodes.AnyAsync(r => r.Code == code, ct))
        {
            code = ReferralCode.GenerateCode();
            attempts++;
            if (attempts > 5)
                return Result<ReferralCodeDto>.Error("REFERRAL_CODE_GENERATION_FAILED:Could not generate a unique code");
        }

        var referralCodeResult = ReferralCode.Create(cmd.UserId, code);
        if (!referralCodeResult.IsSuccess)
            return referralCodeResult.Map(_ => (ReferralCodeDto)null!);

        _context.ReferralCodes.Add(referralCodeResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<ReferralCodeDto>.Success(ToDto(referralCodeResult.Value));
    }

    private static ReferralCodeDto ToDto(ReferralCode rc) =>
        new(rc.Id, rc.Code, rc.UsageCount, rc.CreatedAt);
}
