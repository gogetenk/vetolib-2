using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Microsoft.Extensions.Options;
using Vetolib.Billing.Application;
using Vetolib.Billing.Application.Commands.CreateInvoice;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class CreateInvoiceHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly BillingDbContext _context;
    private readonly CreateInvoiceHandler _handler;

    public CreateInvoiceHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);
        _handler = new CreateInvoiceHandler(_context, Options.Create(new BillingOptions()));
    }

    private CreateInvoiceCommand BuildCommand(
        Guid? clinicId = null,
        Guid? animalId = null,
        string description = "Consultation vétérinaire",
        decimal unitPrice = 150m)
        => new(
            ClinicId: clinicId ?? ClinicId,
            AnimalId: animalId ?? AnimalId,
            ItemDescription: description,
            ItemUnitPrice: unitPrice);

    [Fact]
    public async Task Handle_HappyPath_ReturnsSuccessWithDraftStatus()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(InvoiceStatus.Draft);
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesInvoiceWithCorrectPatientId()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(AnimalId);
    }

    [Fact]
    public async Task Handle_HappyPath_GeneratesSequentialInvoiceNumber()
    {
        var cmd1 = BuildCommand();
        var cmd2 = BuildCommand();

        var result1 = await _handler.Handle(cmd1, CancellationToken.None);
        var result2 = await _handler.Handle(cmd2, CancellationToken.None);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        var year = DateTime.UtcNow.Year;
        result1.Value.InvoiceNumber.Should().Be($"INV-{year}-001");
        result2.Value.InvoiceNumber.Should().Be($"INV-{year}-002");
    }

    [Fact]
    public async Task Handle_WithEmptyAnimalId_ReturnsInvalid()
    {
        var cmd = BuildCommand(animalId: Guid.Empty);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithEmptyClinicId_ReturnsInvalid()
    {
        var cmd = BuildCommand(clinicId: Guid.Empty);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithNegativeUnitPrice_ReturnsInvalid()
    {
        var cmd = BuildCommand(unitPrice: -50m);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_HappyPath_InvoicePersistedInDatabase()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var saved = await _context.Invoices.FindAsync(result.Value.Id);
        saved.Should().NotBeNull();
    }

    public void Dispose() => _context.Dispose();
}
