using FluentValidation;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.AddCustomDrug;

internal class AddCustomDrugValidator : AbstractValidator<AddCustomDrugCommand>
{
    public AddCustomDrugValidator()
    {
        RuleFor(x => x.InnName).NotEmpty().WithMessage("INN name is required");
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Display name is required");
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
    }
}
