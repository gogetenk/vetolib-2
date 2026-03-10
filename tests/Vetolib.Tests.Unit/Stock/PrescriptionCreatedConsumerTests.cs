using Ardalis.Result;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Stock.Application.Commands.DecrementStockForPrescription;
using Vetolib.Stock.Consumers;
using Vetolib.Stock.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

[Collection("StockTests")]
public class PrescriptionCreatedConsumerTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DrugCatalogEntryId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid PrescriptionId = Guid.NewGuid();

    private readonly ISender _sender;
    private readonly IPublisher _publisher;
    private readonly PrescriptionCreatedConsumer _consumer;

    public PrescriptionCreatedConsumerTests()
    {
        _sender = Substitute.For<ISender>();
        _publisher = Substitute.For<IPublisher>();
        _consumer = new PrescriptionCreatedConsumer(_sender, _publisher);
    }

    private void SetupStockAvailable(int quantity = 20)
        => _sender.Send(
                Arg.Is<CheckStockAvailabilityQuery>(q => q.DrugCatalogEntryId == DrugCatalogEntryId),
                Arg.Any<CancellationToken>())
            .Returns(Result<StockAvailabilityResult>.Success(
                new StockAvailabilityResult(
                    Available: quantity > 0,
                    Quantity: quantity,
                    Unit: "tablet",
                    IsLowStock: false,
                    IsExpiringSoon: false,
                    Alternatives: [])));

    private void SetupDecrementByDrugSuccess()
        => _sender.Send(
                Arg.Any<DecrementStockByDrugCatalogEntryCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

    [Fact]
    public async Task Handle_WhenStockDecrementConfirmedAndDrugLinked_ChecksAvailabilityFirst()
    {
        SetupStockAvailable(20);
        SetupDecrementByDrugSuccess();

        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: DrugCatalogEntryId,
            Quantity: 3,
            StockDecrementConfirmed: true);

        await _consumer.Handle(evt, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<CheckStockAvailabilityQuery>(q =>
                q.DrugCatalogEntryId == DrugCatalogEntryId &&
                q.ClinicId == ClinicId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockDecrementConfirmedAndAvailable_SendsDecrementByDrugCommand()
    {
        SetupStockAvailable(20);
        SetupDecrementByDrugSuccess();

        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: DrugCatalogEntryId,
            Quantity: 3,
            StockDecrementConfirmed: true);

        await _consumer.Handle(evt, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<DecrementStockByDrugCatalogEntryCommand>(c =>
                c.DrugCatalogEntryId == DrugCatalogEntryId &&
                c.Quantity == 3 &&
                c.PrescriptionId == PrescriptionId &&
                c.ClinicId == ClinicId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockDecrementNotConfirmed_DoesNothing()
    {
        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: DrugCatalogEntryId,
            Quantity: 3,
            StockDecrementConfirmed: false);

        await _consumer.Handle(evt, CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<CheckStockAvailabilityQuery>(),
            Arg.Any<CancellationToken>());
        await _sender.DidNotReceive().Send(
            Arg.Any<DecrementStockByDrugCatalogEntryCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDrugCatalogEntryIdIsNull_DoesNothing()
    {
        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: null,
            Quantity: 3,
            StockDecrementConfirmed: true);

        await _consumer.Handle(evt, CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<CheckStockAvailabilityQuery>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuantityIsNull_DoesNothing()
    {
        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: DrugCatalogEntryId,
            Quantity: null,
            StockDecrementConfirmed: true);

        await _consumer.Handle(evt, CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<CheckStockAvailabilityQuery>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockInsufficient_PublishesStockInsufficientEvent()
    {
        // Available=true but quantity=1 < requested 5
        SetupStockAvailable(1);

        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: DrugCatalogEntryId,
            Quantity: 5,
            StockDecrementConfirmed: true);

        await _consumer.Handle(evt, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<StockInsufficientForPrescriptionEvent>(e =>
                e.PrescriptionId == PrescriptionId &&
                e.DrugCatalogEntryId == DrugCatalogEntryId &&
                e.RequestedQuantity == 5 &&
                e.AvailableQuantity == 1),
            Arg.Any<CancellationToken>());

        await _sender.DidNotReceive().Send(
            Arg.Any<DecrementStockByDrugCatalogEntryCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockAvailable_DoesNotPublishInsufficientEvent()
    {
        SetupStockAvailable(20);
        SetupDecrementByDrugSuccess();

        var evt = new PrescriptionCreatedEvent(
            Id: PrescriptionId,
            ClinicId: ClinicId,
            DrugCatalogEntryId: DrugCatalogEntryId,
            Quantity: 3,
            StockDecrementConfirmed: true);

        await _consumer.Handle(evt, CancellationToken.None);

        await _publisher.DidNotReceive().Publish(
            Arg.Any<StockInsufficientForPrescriptionEvent>(),
            Arg.Any<CancellationToken>());
    }
}
