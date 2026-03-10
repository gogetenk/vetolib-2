namespace Vetolib.Auth.Contracts;

public record OnboardingStateDto(
    bool WelcomeBannerVisible,
    bool ChecklistVisible,
    List<OnboardingStepDto> Steps,
    OnboardingProgressDto Progress,
    bool IsCompleted);

public record OnboardingStepDto(
    string StepId,
    string Label,
    bool IsCompleted,
    string LinkTo);

public record OnboardingProgressDto(
    int Completed,
    int Total);
