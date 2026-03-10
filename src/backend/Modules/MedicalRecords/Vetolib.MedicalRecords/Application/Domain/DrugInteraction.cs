using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

/// <summary>
/// Drug-drug interaction entry linked to a DrugCatalogEntry.
/// </summary>
internal class DrugInteraction : BaseEntity
{
    public Guid DrugCatalogEntryId { get; private set; }
    public Guid OtherDrugId { get; private set; }
    public string OtherDrugName { get; private set; } = string.Empty;
    public InteractionSeverity Severity { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private DrugInteraction() { } // EF Core

    public static DrugInteraction Create(
        Guid drugCatalogEntryId,
        Guid otherDrugId,
        string otherDrugName,
        InteractionSeverity severity,
        string description)
    {
        return new DrugInteraction
        {
            DrugCatalogEntryId = drugCatalogEntryId,
            OtherDrugId = otherDrugId,
            OtherDrugName = otherDrugName.Trim(),
            Severity = severity,
            Description = description.Trim()
        };
    }

    public DrugInteractionDto ToDto() =>
        new(OtherDrugId, OtherDrugName, Severity, Description);
}
