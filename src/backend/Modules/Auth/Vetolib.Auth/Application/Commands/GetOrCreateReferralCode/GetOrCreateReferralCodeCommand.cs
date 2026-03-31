using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.GetOrCreateReferralCode;

internal record GetOrCreateReferralCodeCommand(Guid UserId) : IRequest<Result<ReferralCodeDto>>;
