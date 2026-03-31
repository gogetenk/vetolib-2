using FluentValidation;

namespace Vetolib.Auth.Application.Queries.SearchClinics;

internal class SearchClinicsValidator : AbstractValidator<SearchClinicsQuery>
{
    public SearchClinicsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Name)
            .MaximumLength(256)
            .When(x => x.Name is not null)
            .WithMessage("Name filter must not exceed 256 characters");

        RuleFor(x => x.City)
            .MaximumLength(256)
            .When(x => x.City is not null)
            .WithMessage("City filter must not exceed 256 characters");

        RuleFor(x => x.Species)
            .MaximumLength(100)
            .When(x => x.Species is not null)
            .WithMessage("Species filter must not exceed 100 characters");
    }
}
