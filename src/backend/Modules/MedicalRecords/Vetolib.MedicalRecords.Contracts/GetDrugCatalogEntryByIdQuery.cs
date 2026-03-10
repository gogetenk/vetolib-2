using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record GetDrugCatalogEntryByIdQuery(Guid Id) : IRequest<Result<DrugCatalogEntryDto>>;
