namespace Vetolib.Billing.Contracts;

public interface ICountryTaxResolver
{
    decimal GetTaxRate(string countryCode, TaxCategory category);
    string GetTaxSchemeId(string countryCode);
}
