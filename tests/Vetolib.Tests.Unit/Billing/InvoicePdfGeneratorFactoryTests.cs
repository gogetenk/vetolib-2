using FluentAssertions;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class InvoicePdfGeneratorFactoryTests
{
    private readonly InvoicePdfGeneratorFactory _factory = new();

    [Theory]
    [InlineData("AE")]
    [InlineData("US")]
    [InlineData("DE")]
    [InlineData("PL")]
    [InlineData("GB")]
    public void GetGenerator_NonFacturXCountry_ReturnsStandardPdfGenerator(string countryCode)
    {
        var generator = _factory.GetGenerator(countryCode);

        generator.Should().BeOfType<StandardPdfGenerator>();
    }

    [Theory]
    [InlineData("FR")]
    [InlineData("fr")]
    public void GetGenerator_France_ReturnsFacturXPdfGenerator(string countryCode)
    {
        var generator = _factory.GetGenerator(countryCode);

        generator.Should().BeOfType<FacturXPdfGenerator>();
    }

    [Fact]
    public void GetGenerator_StandardGenerator_ImplementsInterface()
    {
        var generator = _factory.GetGenerator("AE");

        generator.Should().BeAssignableTo<IInvoicePdfGenerator>();
    }

    [Fact]
    public void GetGenerator_FacturXGenerator_ImplementsInterface()
    {
        var generator = _factory.GetGenerator("FR");

        generator.Should().BeAssignableTo<IInvoicePdfGenerator>();
    }

    [Fact]
    public void StandardPdfGenerator_ViaFactory_ProducesValidPdf()
    {
        var dto = BuildUaeInvoiceDto();
        var generator = _factory.GetGenerator("AE");

        var bytes = generator.Generate(dto, "Desert Paws Veterinary Clinic", "100XXXXXXXXX");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
        var header = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public void FacturXPdfGenerator_ViaFactory_ProducesValidPdf()
    {
        var dto = BuildFrenchInvoiceDto();
        var generator = _factory.GetGenerator("FR");

        var bytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
        var header = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
        header.Should().Be("%PDF");
    }

    private static InvoiceDto BuildUaeInvoiceDto() => new(
        Id: Guid.NewGuid(),
        InvoiceNumber: "INV-2026-042",
        PatientId: Guid.NewGuid(),
        PatientName: "Max",
        OwnerName: "Ahmed Al-Mansoori",
        OwnerPhone: "+971 50 123 4567",
        AppointmentId: null,
        Status: InvoiceStatus.Sent,
        Items: new List<InvoiceItemDto>
        {
            new(Guid.NewGuid(), "Consultation", 1, 150.00m, 157.50m, TaxCategory.Standard, 0.05m),
        },
        Subtotal: 150.00m,
        VatRate: 0.05m,
        VatAmount: 7.50m,
        Total: 157.50m,
        Notes: null,
        CreatedAt: new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
        PaidAt: null,
        DueDate: new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
        ClinicId: Guid.NewGuid(),
        CurrencyCode: "AED",
        CountryCode: "AE");

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
        },
        Subtotal: 50.00m,
        VatRate: 0.20m,
        VatAmount: 10.00m,
        Total: 60.00m,
        Notes: null,
        CreatedAt: new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
        PaidAt: null,
        DueDate: new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc),
        ClinicId: Guid.NewGuid(),
        CurrencyCode: "EUR",
        SellerSiren: "123456789",
        SellerVatNumber: "FR12345678901",
        BuyerName: "Jean Dupont",
        InvoiceTypeCode: "380",
        CountryCode: "FR");
}
