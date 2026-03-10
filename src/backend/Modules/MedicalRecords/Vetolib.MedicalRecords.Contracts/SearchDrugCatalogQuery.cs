using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record SearchDrugCatalogQuery(string? SearchTerm, int Limit = 20) : IRequest<Result<List<DrugCatalogEntryDto>>>;
