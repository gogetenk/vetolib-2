using s2industries.ZUGFeRD;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.GenerateInvoicePdf;

/// <summary>
/// Generates CII (Cross-Industry Invoice) XML conforming to EN16931 / Factur-X.
/// Uses the ZUGFeRD-csharp library (<see cref="InvoiceDescriptor"/>).
/// </summary>
internal static class FacturXXmlGenerator
{
    /// <summary>
    /// Builds an EN16931-compliant CII XML byte array from the given invoice DTO.
    /// </summary>
    public static byte[] Generate(InvoiceDto invoice, string clinicName, string taxRegistrationNumber)
    {
        var currencyCode = MapCurrencyCode(invoice.CurrencyCode);
        var invoiceTypeCode = MapInvoiceTypeCode(invoice.InvoiceTypeCode);

        var descriptor = InvoiceDescriptor.CreateInvoice(
            invoice.InvoiceNumber,
            invoice.CreatedAt,
            currencyCode,
            invoice.PurchaseOrderReference ?? string.Empty);

        descriptor.Type = invoiceTypeCode;
        descriptor.ReferenceOrderNo = invoice.PurchaseOrderReference ?? string.Empty;

        // Seller
        descriptor.SetSeller(
            name: clinicName,
            postcode: string.Empty,
            city: string.Empty,
            street: string.Empty,
            country: MapCountryCode(invoice.CountryCode));

        if (!string.IsNullOrWhiteSpace(invoice.SellerVatNumber))
            descriptor.AddSellerTaxRegistration(invoice.SellerVatNumber, TaxRegistrationSchemeID.VA);

        if (!string.IsNullOrWhiteSpace(invoice.SellerSiren))
            descriptor.AddSellerTaxRegistration(invoice.SellerSiren, TaxRegistrationSchemeID.FC);

        // Buyer
        descriptor.SetBuyer(
            name: !string.IsNullOrWhiteSpace(invoice.BuyerName) ? invoice.BuyerName : "Client",
            postcode: string.Empty,
            city: string.Empty,
            street: invoice.BuyerAddress ?? string.Empty,
            country: MapCountryCode(invoice.CountryCode));

        if (!string.IsNullOrWhiteSpace(invoice.BuyerVatNumber))
            descriptor.AddBuyerTaxRegistration(invoice.BuyerVatNumber, TaxRegistrationSchemeID.VA);

        if (!string.IsNullOrWhiteSpace(invoice.BuyerSiren))
            descriptor.AddBuyerTaxRegistration(invoice.BuyerSiren, TaxRegistrationSchemeID.FC);

        // Payment terms
        if (!string.IsNullOrWhiteSpace(invoice.PaymentTerms))
            descriptor.AddTradePaymentTerms(invoice.PaymentTerms, invoice.DueDate);
        else if (invoice.DueDate.HasValue)
            descriptor.AddTradePaymentTerms($"Due by {invoice.DueDate.Value:yyyy-MM-dd}", invoice.DueDate);

        // Line items
        var lineIndex = 1;
        foreach (var item in invoice.Items)
        {
            descriptor.AddTradeLineItem(
                lineID: lineIndex.ToString(),
                name: item.Description,
                unitCode: QuantityCodes.C62,
                netUnitPrice: item.UnitPriceExclTax,
                billedQuantity: item.Quantity,
                taxType: TaxTypes.VAT,
                categoryCode: MapTaxCategory(item.TaxCategory),
                taxPercent: item.TaxRate * 100m);

            lineIndex++;
        }

        // Tax breakdown — group by TaxRate
        var taxGroups = invoice.Items
            .GroupBy(i => new { i.TaxRate, i.TaxCategory })
            .ToList();

        foreach (var group in taxGroups)
        {
            var basisAmount = group.Sum(i => i.UnitPriceExclTax * i.Quantity);
            var taxPercent = group.Key.TaxRate * 100m;
            var taxAmount = basisAmount * group.Key.TaxRate;

            descriptor.AddApplicableTradeTax(
                basisAmount: basisAmount,
                percent: taxPercent,
                taxAmount: taxAmount,
                typeCode: TaxTypes.VAT,
                categoryCode: MapTaxCategory(group.Key.TaxCategory));
        }

        // Totals
        descriptor.SetTotals(
            lineTotalAmount: invoice.Subtotal,
            taxBasisAmount: invoice.Subtotal,
            taxTotalAmount: invoice.VatAmount,
            grandTotalAmount: invoice.Total,
            duePayableAmount: invoice.Total);

        // Serialize to XML
        using var stream = new MemoryStream();
        descriptor.Save(stream, ZUGFeRDVersion.Version23, Profile.Comfort);
        return stream.ToArray();
    }

    internal static CurrencyCodes MapCurrencyCode(string code) => code.ToUpperInvariant() switch
    {
        "EUR" => CurrencyCodes.EUR,
        "USD" => CurrencyCodes.USD,
        "GBP" => CurrencyCodes.GBP,
        "AED" => CurrencyCodes.AED,
        "PLN" => CurrencyCodes.PLN,
        "CHF" => CurrencyCodes.CHF,
        _ => CurrencyCodes.EUR
    };

    internal static InvoiceType MapInvoiceTypeCode(string typeCode) => typeCode switch
    {
        "381" => InvoiceType.CreditNoteRelatedToGoodsOrServices,
        "380" => InvoiceType.Invoice,
        "384" => InvoiceType.CreditNoteRelatedToFinancialAdjustments,
        _ => InvoiceType.Invoice
    };

    internal static CountryCodes MapCountryCode(string code) => code.ToUpperInvariant() switch
    {
        "FR" => CountryCodes.FR,
        "DE" => CountryCodes.DE,
        "AE" => CountryCodes.AE,
        "PL" => CountryCodes.PL,
        "GB" => CountryCodes.GB,
        "US" => CountryCodes.US,
        "BE" => CountryCodes.BE,
        "IT" => CountryCodes.IT,
        "ES" => CountryCodes.ES,
        _ => CountryCodes.FR
    };

    internal static TaxCategoryCodes MapTaxCategory(TaxCategory category) => category switch
    {
        TaxCategory.Standard => TaxCategoryCodes.S,
        TaxCategory.Reduced => TaxCategoryCodes.S,
        TaxCategory.SuperReduced => TaxCategoryCodes.S,
        TaxCategory.Zero => TaxCategoryCodes.Z,
        TaxCategory.Exempt => TaxCategoryCodes.E,
        _ => TaxCategoryCodes.S
    };
}
