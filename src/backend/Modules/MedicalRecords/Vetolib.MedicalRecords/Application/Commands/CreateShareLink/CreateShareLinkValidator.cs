using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreateShareLink;

internal class CreateShareLinkValidator : AbstractValidator<CreateShareLinkCommand>
{
    public CreateShareLinkValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("PatientId is required");
        RuleFor(x => x.OwnerAccountId).NotEmpty().WithMessage("OwnerAccountId is required");
    }
}
