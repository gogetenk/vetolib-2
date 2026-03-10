using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

/// <summary>
/// Owned entity: species-specific contraindication for a DrugCatalogEntry.
/// </summary>
internal class SpeciesContraindication : BaseEntity
{
    public Guid DrugCatalogEntryId { get; private set; }
    public Species Species { get; private set; }
    public InteractionSeverity Severity { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private SpeciesContraindication() { } // EF Core

    public static SpeciesContraindication Create(
        Guid drugCatalogEntryId,
        Species species,
        InteractionSeverity severity,
        string reason)
    {
        return new SpeciesContraindication
        {
            DrugCatalogEntryId = drugCatalogEntryId,
            Species = species,
            Severity = severity,
            Reason = reason.Trim()
        };
    }

    public SpeciesContraindicationDto ToDto() =>
        new(Species, Severity, Reason);
}
