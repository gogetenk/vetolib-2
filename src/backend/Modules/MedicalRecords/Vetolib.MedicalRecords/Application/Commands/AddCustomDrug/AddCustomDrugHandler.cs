using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.AddCustomDrug;

internal class AddCustomDrugHandler : IRequestHandler<AddCustomDrugCommand, Result<DrugCatalogEntryDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public AddCustomDrugHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DrugCatalogEntryDto>> Handle(AddCustomDrugCommand cmd, CancellationToken ct)
    {
        var result = DrugCatalogEntry.Create(
            cmd.InnName,
            cmd.DisplayName,
            cmd.Category,
            clinicId: cmd.ClinicId);

        if (!result.IsSuccess)
            return Result<DrugCatalogEntryDto>.Invalid(result.ValidationErrors.ToList());

        var entry = result.Value;
        _context.DrugCatalogEntries.Add(entry);
        await _context.SaveChangesAsync(ct);

        return Result<DrugCatalogEntryDto>.Success(entry.ToDto());
    }
}
