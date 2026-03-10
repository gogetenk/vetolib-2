using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record AddCustomDrugCommand(
    string InnName,
    string DisplayName,
    DrugCategory Category,
    Guid ClinicId) : IRequest<Result<DrugCatalogEntryDto>>;
