using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.CompleteOnboardingStep;

internal record CompleteOnboardingStepCommand(Guid UserId, string StepId) : IRequest<Result>;
