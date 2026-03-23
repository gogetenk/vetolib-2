using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application;

internal class CountryTaxResolver : ICountryTaxResolver
{
    private static readonly Dictionary<string, Dictionary<TaxCategory, decimal>> TaxRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AE"] = new()
        {
            [TaxCategory.Standard] = 0.05m,
            [TaxCategory.Reduced] = 0.05m,
            [TaxCategory.SuperReduced] = 0.05m,
            [TaxCategory.Zero] = 0m,
            [TaxCategory.Exempt] = 0m
        },
        ["FR"] = new()
        {
            [TaxCategory.Standard] = 0.20m,
            [TaxCategory.Reduced] = 0.10m,
            [TaxCategory.SuperReduced] = 0.055m,
            [TaxCategory.Zero] = 0m,
            [TaxCategory.Exempt] = 0m
        },
        ["PL"] = new()
        {
            [TaxCategory.Standard] = 0.23m,
            [TaxCategory.Reduced] = 0.08m,
            [TaxCategory.SuperReduced] = 0.05m,
            [TaxCategory.Zero] = 0m,
            [TaxCategory.Exempt] = 0m
        }
    };

    private static readonly Dictionary<string, string> TaxSchemeIds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AE"] = "UAE-VAT",
        ["FR"] = "FR-TVA",
        ["PL"] = "PL-VAT"
    };

    public decimal GetTaxRate(string countryCode, TaxCategory category)
    {
        if (TaxRates.TryGetValue(countryCode, out var rates) &&
            rates.TryGetValue(category, out var rate))
        {
            return rate;
        }

        // Default: UAE standard rate for unknown countries
        return 0.05m;
    }

    public string GetTaxSchemeId(string countryCode)
    {
        return TaxSchemeIds.TryGetValue(countryCode, out var schemeId)
            ? schemeId
            : "UNKNOWN";
    }
}
