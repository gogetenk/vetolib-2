using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.GetOnboardingState;

internal record GetOnboardingStateQuery(Guid UserId) : IRequest<Result<OnboardingStateDto>>;
