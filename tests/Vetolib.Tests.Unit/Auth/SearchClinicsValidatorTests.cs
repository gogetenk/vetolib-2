using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Auth.Application.Queries.SearchClinics;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class SearchClinicsValidatorTests
{
    private readonly SearchClinicsValidator _validator = new();

    [Fact]
    public void ValidQuery_NoErrors()
    {
        var query = new SearchClinicsQuery("Desert", "Dubai", "Dog", 1, 20);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AllNullFilters_NoErrors()
    {
        var query = new SearchClinicsQuery(null, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PageLessThan1_HasError()
    {
        var query = new SearchClinicsQuery(null, null, null, Page: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void PageSizeLessThan1_HasError()
    {
        var query = new SearchClinicsQuery(null, null, null, PageSize: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void PageSizeOver100_HasError()
    {
        var query = new SearchClinicsQuery(null, null, null, PageSize: 101);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void NameTooLong_HasError()
    {
        var query = new SearchClinicsQuery(new string('a', 257), null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CityTooLong_HasError()
    {
        var query = new SearchClinicsQuery(null, new string('a', 257), null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void SpeciesTooLong_HasError()
    {
        var query = new SearchClinicsQuery(null, null, new string('a', 101));
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Species);
    }
}
