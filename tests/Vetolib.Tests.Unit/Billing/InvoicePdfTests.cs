using FluentAssertions;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class InvoicePdfTests
{
    private static InvoiceDto BuildSentInvoiceDto() => new(
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
            new(Guid.NewGuid(), "Vaccination", 1, 200.00m, 210.00m, TaxCategory.Standard, 0.05m),
        },
        Subtotal: 350.00m,
        VatRate: 0.05m,
        VatAmount: 17.50m,
        Total: 367.50m,
        Notes: null,
        CreatedAt: new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
        PaidAt: null,
        DueDate: new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
        ClinicId: Guid.NewGuid(),
        CurrencyCode: "AED",
        CountryCode: "AE");

    [Fact]
    public void Generate_WithSentInvoice_ReturnsBytesGreaterThanZero()
    {
        var dto = BuildSentInvoiceDto();

        var bytes = InvoicePdfGenerator.Generate(dto, "Desert Paws Veterinary Clinic", "100XXXXXXXXX");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generate_WithPaidInvoice_ReturnsBytesGreaterThanZero()
    {
        var dto = BuildSentInvoiceDto() with
        {
            Status = InvoiceStatus.Paid,
            PaidAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
        };

        var bytes = InvoicePdfGenerator.Generate(dto, "Desert Paws Veterinary Clinic", "100XXXXXXXXX");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generate_PdfStartsWithPdfMagicBytes()
    {
        var dto = BuildSentInvoiceDto();

        var bytes = InvoicePdfGenerator.Generate(dto, "Desert Paws Veterinary Clinic", "100XXXXXXXXX");

        // PDF files begin with %PDF
        var header = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public void Generate_WithNotes_ReturnsBytesGreaterThanZero()
    {
        var dto = BuildSentInvoiceDto() with { Notes = "First visit discount applied." };

        var bytes = InvoicePdfGenerator.Generate(dto, "Desert Paws Veterinary Clinic", "100XXXXXXXXX");

        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generate_WithEmptyTrn_DoesNotThrow()
    {
        var dto = BuildSentInvoiceDto();

        var bytes = InvoicePdfGenerator.Generate(dto, "Happy Paws Clinic", string.Empty);

        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generate_WithMixedTaxRates_ReturnsBytesGreaterThanZero()
    {
        var dto = BuildSentInvoiceDto() with
        {
            Items = new List<InvoiceItemDto>
            {
                new(Guid.NewGuid(), "Consultation", 1, 150.00m, 180.00m, TaxCategory.Standard, 0.20m),
                new(Guid.NewGuid(), "Medication", 1, 50.00m, 52.75m, TaxCategory.SuperReduced, 0.055m),
                new(Guid.NewGuid(), "Exempt service", 1, 100.00m, 100.00m, TaxCategory.Exempt, 0m),
            },
            Subtotal = 300.00m,
            VatAmount = 32.75m,
            Total = 332.75m,
            CountryCode = "FR"
        };

        var bytes = InvoicePdfGenerator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }
}
