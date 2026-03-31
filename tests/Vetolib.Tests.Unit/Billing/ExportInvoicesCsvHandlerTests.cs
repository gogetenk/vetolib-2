using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application.Queries.ExportInvoicesCsv;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class ExportInvoicesCsvHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly BillingDbContext _context;
    private readonly ExportInvoicesCsvHandler _handler;

    public ExportInvoicesCsvHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);
        _handler = new ExportInvoicesCsvHandler(_context);
    }

    [Fact]
    public async Task Handle_FromAfterTo_ReturnsInvalid()
    {
        // Arrange
        var query = new ExportInvoicesCsvQuery(
            new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_FromEqualsTo_ReturnsInvalid()
    {
        // Arrange
        var sameDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var query = new ExportInvoicesCsvQuery(sameDate, sameDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_ValidRange_NoInvoices_ReturnsSuccessWithHeaderOnly()
    {
        // Arrange
        var query = new ExportInvoicesCsvQuery(
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CsvBytes.Should().NotBeEmpty();
        result.Value.FileName.Should().Be("invoices-2026-01-01-to-2026-03-31.csv");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
