using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

/// <summary>
/// Weight-based dosage guideline for a specific species and drug.
/// </summary>
internal class DosageGuideline : BaseEntity
{
    public Guid DrugCatalogEntryId { get; private set; }
    public Species Species { get; private set; }
    public decimal MinDosePerKg { get; private set; }
    public decimal MaxDosePerKg { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string Route { get; private set; } = string.Empty;

    private DosageGuideline() { } // EF Core

    public static DosageGuideline Create(
        Guid drugCatalogEntryId,
        Species species,
        decimal minDosePerKg,
        decimal maxDosePerKg,
        string unit,
        string route)
    {
        return new DosageGuideline
        {
            DrugCatalogEntryId = drugCatalogEntryId,
            Species = species,
            MinDosePerKg = minDosePerKg,
            MaxDosePerKg = maxDosePerKg,
            Unit = unit.Trim(),
            Route = route.Trim()
        };
    }

    public DosageGuidelineDto ToDto() =>
        new(Species, MinDosePerKg, MaxDosePerKg, Unit, Route);
}
