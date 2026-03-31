using FluentAssertions;
using Vetolib.Billing.Application.Queries.ExportInvoicesCsv;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class InvoiceCsvGeneratorTests
{
    [Fact]
    public void Generate_WithEmptyList_ReturnsHeaderOnly()
    {
        // Arrange
        var invoices = new List<InvoiceDto>();

        // Act
        var csv = InvoiceCsvGenerator.Generate(invoices);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(1);
        lines[0].Should().Be("InvoiceNumber,Date,ClientName,ClientEmail,Description,Amount,TaxRate,TaxAmount,Total,Status,PaidAt,CurrencyCode");
    }

    [Fact]
    public void Generate_WithSingleInvoice_ReturnsCorrectCsv()
    {
        // Arrange
        var invoice = CreateInvoice(
            invoiceNumber: "INV-001",
            buyerName: "Ahmed Al Mansouri",
            status: InvoiceStatus.Paid,
            subtotal: 100.00m,
            vatRate: 0.05m,
            vatAmount: 5.00m,
            total: 105.00m,
            createdAt: new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            paidAt: new DateTime(2026, 1, 20, 14, 0, 0, DateTimeKind.Utc),
            items: [new InvoiceItemDto(Guid.NewGuid(), "Consultation", 1, 100.00m, 105.00m, TaxCategory.Standard, 0.05m)]);

        // Act
        var csv = InvoiceCsvGenerator.Generate([invoice]);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(2);
        lines[1].Should().Be("INV-001,2026-01-15,Ahmed Al Mansouri,,Consultation,100.00,0.0500,5.00,105.00,Paid,2026-01-20,AED");
    }

    [Fact]
    public void Generate_WithMultipleItems_JoinsDescriptions()
    {
        // Arrange
        var items = new List<InvoiceItemDto>
        {
            new(Guid.NewGuid(), "Consultation", 1, 80.00m, 84.00m, TaxCategory.Standard, 0.05m),
            new(Guid.NewGuid(), "Vaccination", 1, 20.00m, 21.00m, TaxCategory.Standard, 0.05m)
        };

        var invoice = CreateInvoice(
            invoiceNumber: "INV-002",
            buyerName: "Fatima Hassan",
            status: InvoiceStatus.Sent,
            subtotal: 100.00m,
            vatRate: 0.05m,
            vatAmount: 5.00m,
            total: 105.00m,
            createdAt: new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            paidAt: null,
            items: items);

        // Act
        var csv = InvoiceCsvGenerator.Generate([invoice]);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        // Description with semicolon should NOT need escaping (no comma)
        lines[1].Should().Contain("Consultation; Vaccination");
    }

    [Fact]
    public void Generate_WithCommaInBuyerName_EscapesField()
    {
        // Arrange
        var invoice = CreateInvoice(
            invoiceNumber: "INV-003",
            buyerName: "Al Mansouri, Ahmed",
            status: InvoiceStatus.Draft,
            subtotal: 50.00m,
            vatRate: 0.05m,
            vatAmount: 2.50m,
            total: 52.50m,
            createdAt: new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            paidAt: null,
            items: [new InvoiceItemDto(Guid.NewGuid(), "Checkup", 1, 50.00m, 52.50m, TaxCategory.Standard, 0.05m)]);

        // Act
        var csv = InvoiceCsvGenerator.Generate([invoice]);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[1].Should().Contain("\"Al Mansouri, Ahmed\"");
    }

    [Fact]
    public void Generate_WithQuotesInField_DoublesQuotes()
    {
        // Arrange
        var invoice = CreateInvoice(
            invoiceNumber: "INV-004",
            buyerName: "Ahmed \"The Doc\" Ali",
            status: InvoiceStatus.Draft,
            subtotal: 75.00m,
            vatRate: 0.05m,
            vatAmount: 3.75m,
            total: 78.75m,
            createdAt: new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc),
            paidAt: null,
            items: [new InvoiceItemDto(Guid.NewGuid(), "Surgery", 1, 75.00m, 78.75m, TaxCategory.Standard, 0.05m)]);

        // Act
        var csv = InvoiceCsvGenerator.Generate([invoice]);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[1].Should().Contain("\"Ahmed \"\"The Doc\"\" Ali\"");
    }

    [Fact]
    public void Generate_UnpaidInvoice_PaidAtIsEmpty()
    {
        // Arrange
        var invoice = CreateInvoice(
            invoiceNumber: "INV-005",
            buyerName: "Khalid Bin Zayed",
            status: InvoiceStatus.Sent,
            subtotal: 200.00m,
            vatRate: 0.05m,
            vatAmount: 10.00m,
            total: 210.00m,
            createdAt: new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc),
            paidAt: null,
            items: [new InvoiceItemDto(Guid.NewGuid(), "Dental cleaning", 1, 200.00m, 210.00m, TaxCategory.Standard, 0.05m)]);

        // Act
        var csv = InvoiceCsvGenerator.Generate([invoice]);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        // PaidAt should be empty (second to last field)
        lines[1].Should().EndWith(",Sent,,AED");
    }

    [Fact]
    public void Generate_MultipleInvoices_CorrectLineCount()
    {
        // Arrange
        var invoices = Enumerable.Range(1, 5).Select(i => CreateInvoice(
            invoiceNumber: $"INV-{i:D3}",
            buyerName: $"Client {i}",
            status: InvoiceStatus.Paid,
            subtotal: 100.00m * i,
            vatRate: 0.05m,
            vatAmount: 5.00m * i,
            total: 105.00m * i,
            createdAt: new DateTime(2026, 1, i, 10, 0, 0, DateTimeKind.Utc),
            paidAt: new DateTime(2026, 1, i + 5, 10, 0, 0, DateTimeKind.Utc),
            items: [new InvoiceItemDto(Guid.NewGuid(), "Service", 1, 100.00m * i, 105.00m * i, TaxCategory.Standard, 0.05m)]
        )).ToList();

        // Act
        var csv = InvoiceCsvGenerator.Generate(invoices);

        // Assert
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(6); // 1 header + 5 data rows
    }

    [Fact]
    public void EscapeCsvField_EmptyString_ReturnsEmpty()
    {
        InvoiceCsvGenerator.EscapeCsvField("").Should().BeEmpty();
    }

    [Fact]
    public void EscapeCsvField_NullString_ReturnsEmpty()
    {
        InvoiceCsvGenerator.EscapeCsvField(null!).Should().BeEmpty();
    }

    [Fact]
    public void EscapeCsvField_SimpleString_ReturnsUnchanged()
    {
        InvoiceCsvGenerator.EscapeCsvField("Hello World").Should().Be("Hello World");
    }

    [Fact]
    public void EscapeCsvField_StringWithNewline_WrapsInQuotes()
    {
        InvoiceCsvGenerator.EscapeCsvField("Line1\nLine2").Should().Be("\"Line1\nLine2\"");
    }

    private static InvoiceDto CreateInvoice(
        string invoiceNumber,
        string buyerName,
        InvoiceStatus status,
        decimal subtotal,
        decimal vatRate,
        decimal vatAmount,
        decimal total,
        DateTime createdAt,
        DateTime? paidAt,
        IReadOnlyList<InvoiceItemDto> items) =>
        new(
            Id: Guid.NewGuid(),
            InvoiceNumber: invoiceNumber,
            PatientId: Guid.NewGuid(),
            PatientName: null,
            OwnerName: null,
            OwnerPhone: null,
            AppointmentId: null,
            Status: status,
            Items: items,
            Subtotal: subtotal,
            VatRate: vatRate,
            VatAmount: vatAmount,
            Total: total,
            Notes: null,
            CreatedAt: createdAt,
            PaidAt: paidAt,
            DueDate: null,
            ClinicId: Guid.NewGuid(),
            CurrencyCode: "AED",
            BuyerName: buyerName);
}
