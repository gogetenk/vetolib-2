using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.CompleteOnboardingStep;

internal class CompleteOnboardingStepHandler : IRequestHandler<CompleteOnboardingStepCommand, Result>
{
    private readonly AuthDbContext _context;

    public CompleteOnboardingStepHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CompleteOnboardingStepCommand command, CancellationToken ct)
    {
        var state = await _context.OnboardingStates
            .FirstOrDefaultAsync(o => o.UserId == command.UserId, ct);

        if (state is null)
            return Result.NotFound("Onboarding state not found. Call GET /api/onboarding first.");

        var roleSteps = OnboardingSteps.GetStepsForRole(state.Role);

        if (!roleSteps.Contains(command.StepId))
            return Result.Invalid(new ValidationError(nameof(command.StepId),
                $"Step '{command.StepId}' is not valid for role '{state.Role}'"));

        var completeResult = state.CompleteStep(command.StepId);
        if (!completeResult.IsSuccess)
            return completeResult;

        // Mark fully completed if all steps done
        if (!state.CompletedAt.HasValue && state.IsFullyCompleted(roleSteps))
            state.MarkCompleted();

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
