using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.DeleteTemplate;

internal class DeleteTemplateValidator : AbstractValidator<DeleteTemplateCommand>
{
    public DeleteTemplateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template Id is required.");
    }
}
