using FluentAssertions;
using Vetolib.Billing.Application;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class CountryTaxResolverTests
{
    private readonly CountryTaxResolver _resolver = new();

    // UAE rates
    [Fact]
    public void GetTaxRate_UAE_Standard_Returns5Percent()
        => _resolver.GetTaxRate("AE", TaxCategory.Standard).Should().Be(0.05m);

    [Fact]
    public void GetTaxRate_UAE_Zero_Returns0()
        => _resolver.GetTaxRate("AE", TaxCategory.Zero).Should().Be(0m);

    [Fact]
    public void GetTaxRate_UAE_Exempt_Returns0()
        => _resolver.GetTaxRate("AE", TaxCategory.Exempt).Should().Be(0m);

    // France rates
    [Fact]
    public void GetTaxRate_France_Standard_Returns20Percent()
        => _resolver.GetTaxRate("FR", TaxCategory.Standard).Should().Be(0.20m);

    [Fact]
    public void GetTaxRate_France_Reduced_Returns10Percent()
        => _resolver.GetTaxRate("FR", TaxCategory.Reduced).Should().Be(0.10m);

    [Fact]
    public void GetTaxRate_France_SuperReduced_Returns5Point5Percent()
        => _resolver.GetTaxRate("FR", TaxCategory.SuperReduced).Should().Be(0.055m);

    [Fact]
    public void GetTaxRate_France_Zero_Returns0()
        => _resolver.GetTaxRate("FR", TaxCategory.Zero).Should().Be(0m);

    // Poland rates
    [Fact]
    public void GetTaxRate_Poland_Standard_Returns23Percent()
        => _resolver.GetTaxRate("PL", TaxCategory.Standard).Should().Be(0.23m);

    [Fact]
    public void GetTaxRate_Poland_Reduced_Returns8Percent()
        => _resolver.GetTaxRate("PL", TaxCategory.Reduced).Should().Be(0.08m);

    [Fact]
    public void GetTaxRate_Poland_SuperReduced_Returns5Percent()
        => _resolver.GetTaxRate("PL", TaxCategory.SuperReduced).Should().Be(0.05m);

    // Unknown country defaults to UAE standard
    [Fact]
    public void GetTaxRate_UnknownCountry_DefaultsTo5Percent()
        => _resolver.GetTaxRate("XX", TaxCategory.Standard).Should().Be(0.05m);

    // Case insensitive
    [Fact]
    public void GetTaxRate_CaseInsensitive_Works()
        => _resolver.GetTaxRate("fr", TaxCategory.Standard).Should().Be(0.20m);

    // Tax scheme IDs
    [Fact]
    public void GetTaxSchemeId_UAE_ReturnsUAEVAT()
        => _resolver.GetTaxSchemeId("AE").Should().Be("UAE-VAT");

    [Fact]
    public void GetTaxSchemeId_France_ReturnsFRTVA()
        => _resolver.GetTaxSchemeId("FR").Should().Be("FR-TVA");

    [Fact]
    public void GetTaxSchemeId_Poland_ReturnsPLVAT()
        => _resolver.GetTaxSchemeId("PL").Should().Be("PL-VAT");

    [Fact]
    public void GetTaxSchemeId_Unknown_ReturnsUnknown()
        => _resolver.GetTaxSchemeId("XX").Should().Be("UNKNOWN");

    // Decimal rounding edge cases
    [Fact]
    public void InvoiceItem_FranceSuperReduced_RoundsCorrectly()
    {
        // 99.99 * 0.055 = 5.49945 → rounded to 5.50
        var result = InvoiceItem_TaxCalc(99.99m, 0.055m);
        result.taxAmount.Should().Be(5.50m);
        result.total.Should().Be(105.49m);
    }

    [Fact]
    public void InvoiceItem_ZeroRate_NoTax()
    {
        var result = InvoiceItem_TaxCalc(250m, 0m);
        result.taxAmount.Should().Be(0m);
        result.total.Should().Be(250m);
    }

    private static (decimal taxAmount, decimal total) InvoiceItem_TaxCalc(decimal unitPrice, decimal taxRate)
    {
        var taxAmount = Math.Round(unitPrice * taxRate, 2);
        return (taxAmount, unitPrice + taxAmount);
    }
}
