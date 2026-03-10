using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

/// <summary>
/// Drug catalog entry. ClinicId = null means global (visible to all clinics).
/// ClinicId = non-null means clinic-specific (isolated by tenant).
/// Does NOT implement IMultiTenant because ClinicId is nullable.
/// A custom query filter in MedicalRecordsDbContext handles: WHERE ClinicId = @current OR ClinicId IS NULL.
/// </summary>
internal class DrugCatalogEntry : BaseEntity
{
    /// <summary>
    /// NULL = global entry (seed data). Non-null = clinic-specific custom entry.
    /// </summary>
    public Guid? ClinicId { get; private set; }

    public string InnName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public DrugCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<SpeciesContraindication> _speciesContraindications = [];
    public IReadOnlyList<SpeciesContraindication> SpeciesContraindications => _speciesContraindications.AsReadOnly();

    private readonly List<DrugInteraction> _interactions = [];
    public IReadOnlyList<DrugInteraction> Interactions => _interactions.AsReadOnly();

    private readonly List<DosageGuideline> _dosageGuidelines = [];
    public IReadOnlyList<DosageGuideline> DosageGuidelines => _dosageGuidelines.AsReadOnly();

    private DrugCatalogEntry() { } // EF Core

    public static Result<DrugCatalogEntry> Create(
        string innName,
        string displayName,
        DrugCategory category,
        Guid? clinicId = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(innName))
            errors.Add(new ValidationError(nameof(innName), "INN name is required"));

        if (string.IsNullOrWhiteSpace(displayName))
            errors.Add(new ValidationError(nameof(displayName), "Display name is required"));

        if (errors.Count > 0)
            return Result<DrugCatalogEntry>.Invalid(errors);

        return Result<DrugCatalogEntry>.Success(new DrugCatalogEntry
        {
            InnName = innName.Trim(),
            DisplayName = displayName.Trim(),
            Category = category,
            ClinicId = clinicId,
            IsActive = true
        });
    }

    public void AddContraindication(Species species, InteractionSeverity severity, string reason)
    {
        _speciesContraindications.Add(
            SpeciesContraindication.Create(Id, species, severity, reason));
    }

    public void AddInteraction(Guid otherDrugId, string otherDrugName, InteractionSeverity severity, string description)
    {
        _interactions.Add(
            DrugInteraction.Create(Id, otherDrugId, otherDrugName, severity, description));
    }

    public void AddDosageGuideline(Species species, decimal minDosePerKg, decimal maxDosePerKg, string unit, string route)
    {
        _dosageGuidelines.Add(
            DosageGuideline.Create(Id, species, minDosePerKg, maxDosePerKg, unit, route));
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Error("Drug catalog entry is already inactive");
        IsActive = false;
        return Result.Success();
    }

    public DrugCatalogEntryDto ToDto() => new(
        Id,
        InnName,
        DisplayName,
        Category,
        IsActive,
        ClinicId,
        _speciesContraindications.Select(c => c.ToDto()).ToList(),
        _interactions.Select(i => i.ToDto()).ToList(),
        _dosageGuidelines.Select(d => d.ToDto()).ToList());
}
