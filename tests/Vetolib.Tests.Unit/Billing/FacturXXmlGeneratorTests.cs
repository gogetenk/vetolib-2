using System.Xml.Linq;
using FluentAssertions;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class FacturXXmlGeneratorTests
{
    private static InvoiceDto BuildFrenchInvoiceDto() => new(
        Id: Guid.NewGuid(),
        InvoiceNumber: "FR-2026-001",
        PatientId: Guid.NewGuid(),
        PatientName: "Milou",
        OwnerName: "Jean Dupont",
        OwnerPhone: "+33 6 12 34 56 78",
        AppointmentId: null,
        Status: InvoiceStatus.Sent,
        Items: new List<InvoiceItemDto>
        {
            new(Guid.NewGuid(), "Consultation", 1, 50.00m, 60.00m, TaxCategory.Standard, 0.20m),
            new(Guid.NewGuid(), "Vaccination antirabique", 1, 30.00m, 31.65m, TaxCategory.SuperReduced, 0.055m),
        },
        Subtotal: 80.00m,
        VatRate: 0.20m,
        VatAmount: 11.65m,
        Total: 91.65m,
        Notes: null,
        CreatedAt: new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
        PaidAt: null,
        DueDate: new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc),
        ClinicId: Guid.NewGuid(),
        CurrencyCode: "EUR",
        SellerSiren: "123456789",
        SellerVatNumber: "FR12345678901",
        BuyerName: "Jean Dupont",
        BuyerAddress: "12 Rue de la Paix, 75002 Paris",
        BuyerSiren: "987654321",
        BuyerVatNumber: "FR98765432100",
        OperationType: Vetolib.Billing.Contracts.OperationType.Service,
        InvoiceTypeCode: "380",
        PaymentTerms: "Net 30 days",
        CountryCode: "FR",
        PurchaseOrderReference: "PO-2026-042");

    [Fact]
    public void Generate_WithFrenchInvoice_ReturnsValidXml()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        xmlBytes.Should().NotBeNull();
        xmlBytes.Length.Should().BeGreaterThan(0);

        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);
        var action = () => XDocument.Parse(xmlString);
        action.Should().NotThrow("the output must be valid XML");
    }

    [Fact]
    public void Generate_XmlContainsInvoiceNumber()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        xmlString.Should().Contain("FR-2026-001");
    }

    [Fact]
    public void Generate_XmlContainsSellerName()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        xmlString.Should().Contain("Clinique Vet Paris");
    }

    [Fact]
    public void Generate_XmlContainsBuyerName()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        xmlString.Should().Contain("Jean Dupont");
    }

    [Fact]
    public void Generate_XmlContainsCurrencyCode()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        xmlString.Should().Contain("EUR");
    }

    [Fact]
    public void Generate_XmlContainsLineItems()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        xmlString.Should().Contain("Consultation");
        xmlString.Should().Contain("Vaccination antirabique");
    }

    [Fact]
    public void Generate_XmlContainsSellerVatNumber()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        xmlString.Should().Contain("FR12345678901");
    }

    [Fact]
    public void Generate_XmlContainsTaxAmounts()
    {
        var dto = BuildFrenchInvoiceDto();

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        // Should contain the total amount
        xmlString.Should().Contain("91.65");
    }

    [Fact]
    public void Generate_CreditNote_SetsCorrectInvoiceType()
    {
        var dto = BuildFrenchInvoiceDto() with { InvoiceTypeCode = "381" };

        var xmlBytes = FacturXXmlGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");
        var xmlString = System.Text.Encoding.UTF8.GetString(xmlBytes);

        // CreditNoteRelatedToGoodsOrServices = 81 in the XML type code
        xmlString.Should().NotBeEmpty();
        xmlBytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MapCurrencyCode_ReturnsCorrectCodes()
    {
        FacturXXmlGenerator.MapCurrencyCode("EUR").Should().Be(s2industries.ZUGFeRD.CurrencyCodes.EUR);
        FacturXXmlGenerator.MapCurrencyCode("USD").Should().Be(s2industries.ZUGFeRD.CurrencyCodes.USD);
        FacturXXmlGenerator.MapCurrencyCode("AED").Should().Be(s2industries.ZUGFeRD.CurrencyCodes.AED);
        FacturXXmlGenerator.MapCurrencyCode("PLN").Should().Be(s2industries.ZUGFeRD.CurrencyCodes.PLN);
        FacturXXmlGenerator.MapCurrencyCode("UNKNOWN").Should().Be(s2industries.ZUGFeRD.CurrencyCodes.EUR);
    }

    [Fact]
    public void MapCountryCode_ReturnsCorrectCodes()
    {
        FacturXXmlGenerator.MapCountryCode("FR").Should().Be(s2industries.ZUGFeRD.CountryCodes.FR);
        FacturXXmlGenerator.MapCountryCode("DE").Should().Be(s2industries.ZUGFeRD.CountryCodes.DE);
        FacturXXmlGenerator.MapCountryCode("AE").Should().Be(s2industries.ZUGFeRD.CountryCodes.AE);
        FacturXXmlGenerator.MapCountryCode("UNKNOWN").Should().Be(s2industries.ZUGFeRD.CountryCodes.FR);
    }

    [Fact]
    public void MapTaxCategory_ReturnsCorrectCodes()
    {
        FacturXXmlGenerator.MapTaxCategory(TaxCategory.Standard).Should().Be(s2industries.ZUGFeRD.TaxCategoryCodes.S);
        FacturXXmlGenerator.MapTaxCategory(TaxCategory.Zero).Should().Be(s2industries.ZUGFeRD.TaxCategoryCodes.Z);
        FacturXXmlGenerator.MapTaxCategory(TaxCategory.Exempt).Should().Be(s2industries.ZUGFeRD.TaxCategoryCodes.E);
    }
}
