using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.UpdateTemplate;

internal class UpdateTemplateValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ContentEn)
            .NotEmpty().WithMessage("English content is required.")
            .MaximumLength(2000).WithMessage("English content must not exceed 2000 characters.");

        RuleFor(x => x.ContentAr)
            .NotEmpty().WithMessage("Arabic content is required.")
            .MaximumLength(2000).WithMessage("Arabic content must not exceed 2000 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.")
            .When(x => x.Category is not null);
    }
}
