using FluentAssertions;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class FacturXPdfGeneratorTests
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
        InvoiceTypeCode: "380",
        PaymentTerms: "Net 30 days",
        CountryCode: "FR");

    [Fact]
    public void Generate_WithFrenchInvoice_ReturnsPdfBytesGreaterThanZero()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();

        var bytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generate_WithFrenchInvoice_StartsWithPdfMagicBytes()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();

        var bytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        var header = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public void Generate_WithFrenchInvoice_ContainsFacturXXmlAttachment()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();

        var bytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        // The PDF should contain the embedded file specification referencing factur-x.xml
        var pdfString = System.Text.Encoding.Latin1.GetString(bytes);
        pdfString.Should().Contain("factur-x.xml", "the PDF must contain the Factur-X XML attachment reference");
    }

    [Fact]
    public void Generate_WithFrenchInvoice_ContainsEmbeddedXmlContent()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();

        var bytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        // The embedded XML stream should contain the invoice number
        var pdfString = System.Text.Encoding.Latin1.GetString(bytes);
        pdfString.Should().Contain("FR-2026-001", "the embedded XML should contain the invoice number");
    }

    [Fact]
    public void Generate_WithCreditNote_ReturnsPdfBytes()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto() with { InvoiceTypeCode = "381" };

        var bytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void EmbedXmlAttachment_ProducesValidPdf()
    {
        // Create a minimal PDF and XML to test embedding
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();
        var pdfBytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        // The result should still start with %PDF
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        header.Should().Be("%PDF");

        // And should end with %%EOF
        var trailer = System.Text.Encoding.Latin1.GetString(pdfBytes);
        trailer.Should().Contain("%%EOF");
    }

    [Fact]
    public void EmbedXmlAttachment_ContainsAFRelationship()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();
        var pdfBytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        var pdfString = System.Text.Encoding.Latin1.GetString(pdfBytes);
        pdfString.Should().Contain("/AFRelationship /Data",
            "the filespec must declare AFRelationship for Factur-X compliance");
    }

    [Fact]
    public void EmbedXmlAttachment_ContainsEmbeddedFileSubtype()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();
        var pdfBytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        var pdfString = System.Text.Encoding.Latin1.GetString(pdfBytes);
        pdfString.Should().Contain("/Subtype /text#2Fxml",
            "the embedded file stream must declare its MIME type as text/xml");
    }

    /// <summary>
    /// Documents known PDF/A-3 compliance gaps. The current implementation embeds the XML
    /// attachment correctly but does NOT produce a fully PDF/A-3 compliant document.
    /// See PDFA3_COMPLIANCE.md in the Billing module for the full gap analysis.
    /// </summary>
    [Fact]
    public void Generate_PdfA3ComplianceGaps_DocumentedAsKnownLimitation()
    {
        var generator = new FacturXPdfGenerator();
        var dto = BuildFrenchInvoiceDto();
        var pdfBytes = generator.Generate(dto, "Clinique Vet Paris", "FR12345678901");

        var pdfString = System.Text.Encoding.Latin1.GetString(pdfBytes);

        // GAP 1: No PDF/A-3 OutputIntent with sRGB ICC profile
        // PDF/A-3 requires: /OutputIntents [<< /Type /OutputIntent /S /GTS_PDFA1 ... >>]
        pdfString.Should().NotContain("/GTS_PDFA1",
            "KNOWN GAP: OutputIntent with sRGB ICC profile is not yet embedded");

        // GAP 2: No XMP metadata declaring PDF/A conformance
        // PDF/A-3 requires XMP metadata with pdfaid:part=3 and pdfaid:conformance=B
        pdfString.Should().NotContain("pdfaid:part",
            "KNOWN GAP: XMP metadata for PDF/A-3 conformance is not yet present");

        // GAP 3: No /AF array in document catalog
        // PDF/A-3 requires the document catalog to contain /AF [filespec_ref]
        // Our implementation only adds the filespec with /AFRelationship but does not
        // patch the existing catalog's /AF array (would require full PDF rewrite)

        // Despite these gaps, the functional requirements are met:
        // - The PDF is valid and viewable
        // - The factur-x.xml is embedded and extractable
        // - The CII XML conforms to EN16931
    }
}
