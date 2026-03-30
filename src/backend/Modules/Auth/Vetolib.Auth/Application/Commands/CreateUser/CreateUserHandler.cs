using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.CreateUser;

internal class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly AuthDbContext _context;

    public CreateUserHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // Check if email already exists globally (cross-tenant uniqueness)
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);

        if (existingUser is not null)
            return Result<UserDto>.Error("EMAIL_EXISTS:This email is already in use");

        // Create user via domain factory
        var userResult = User.Create(cmd.ClinicId, cmd.Email, cmd.Password, cmd.Role, cmd.VetLicenseNumber);

        if (!userResult.IsSuccess)
        {
            // Check for VET_LICENSE_REQUIRED
            var vetLicenseError = userResult.ValidationErrors
                .FirstOrDefault(e => e.Identifier == "vetLicenseNumber");
            if (vetLicenseError is not null)
            {
                return Result<UserDto>.Error($"VET_LICENSE_REQUIRED:{vetLicenseError.ErrorMessage}");
            }

            // Return validation errors
            return Result<UserDto>.Invalid(userResult.ValidationErrors.ToList());
        }

        _context.Users.Add(userResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<UserDto>.Success(userResult.Value.ToDto());
    }
}
