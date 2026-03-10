using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record CheckInteractionsQuery(
    Guid PatientId,
    Guid DrugCatalogEntryId,
    decimal? DosageAmount,
    Guid ClinicId) : IRequest<Result<InteractionCheckResult>>;
