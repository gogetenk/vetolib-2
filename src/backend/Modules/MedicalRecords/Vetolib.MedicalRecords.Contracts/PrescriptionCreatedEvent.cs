using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record PrescriptionCreatedEvent(
    Guid Id,
    Guid ClinicId,
    Guid? DrugCatalogEntryId,
    int? Quantity,
    bool StockDecrementConfirmed) : INotification;
