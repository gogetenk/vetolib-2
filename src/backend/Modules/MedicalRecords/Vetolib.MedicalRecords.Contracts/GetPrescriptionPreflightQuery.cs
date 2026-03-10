using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record GetPrescriptionPreflightQuery(
    Guid PatientId,
    Guid DrugCatalogEntryId,
    decimal? DosageAmount,
    Guid ClinicId) : IRequest<Result<PrescriptionPreflightResult>>;

public record PrescriptionPreflightResult(
    List<InteractionAlert> InteractionAlerts,
    PrescriptionStockAvailability StockAvailability,
    List<SafeAlternativeDto> SafeAlternatives);

public record PrescriptionStockAvailability(
    bool Available,
    int Quantity,
    string Unit,
    bool IsLowStock,
    bool IsExpiringSoon);

public record SafeAlternativeDto(
    Guid DrugCatalogEntryId,
    string DrugName,
    int StockQuantity,
    string StockUnit,
    List<InteractionAlert> InteractionAlerts);
