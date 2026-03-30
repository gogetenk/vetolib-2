using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

/// <summary>
/// Seeds pre-built system templates for common consultation types.
/// System templates have IsSystemTemplate = true and are read-only.
/// They are seeded per-clinic when the first template list is requested,
/// or can be triggered at startup via DbInitializer.
///
/// IgnoreQueryFilters is used because the seed runs outside a tenant context.
/// </summary>
internal static class MedicalRecordTemplateSeedData
{
    public static async Task SeedAsync(MedicalRecordsDbContext context, Guid clinicId, CancellationToken cancellationToken = default)
    {
        var alreadySeeded = await context.MedicalRecordTemplates
            .IgnoreQueryFilters()
            .AnyAsync(t => t.ClinicId == clinicId && t.IsSystemTemplate, cancellationToken);

        if (alreadySeeded)
            return;

        var templates = BuildSystemTemplates(clinicId);
        context.MedicalRecordTemplates.AddRange(templates);
        await context.SaveChangesAsync(cancellationToken);
    }

    internal static List<MedicalRecordTemplate> BuildSystemTemplates(Guid clinicId)
    {
        var templates = new List<MedicalRecordTemplate>();

        AddTemplate(templates, clinicId, "Routine Checkup", TemplateCategory.Checkup,
            "General physical examination — no abnormalities detected",
            "No treatment required. Advised routine follow-up in 12 months.",
            "Weight, temperature, heart rate, respiratory rate all within normal limits. Teeth, ears, eyes examined. Skin and coat condition normal.",
            species: null, sortOrder: 1);

        AddTemplate(templates, clinicId, "Vaccination Visit", TemplateCategory.Vaccination,
            "Vaccination administered as per schedule",
            "Vaccine administered subcutaneously. Observed for 15 minutes post-injection — no adverse reaction.",
            "Pre-vaccination assessment: patient alert, no fever, no signs of illness. Vaccination record updated. Next booster due in [interval].",
            species: null, sortOrder: 2);

        AddTemplate(templates, clinicId, "Dental Cleaning", TemplateCategory.Dental,
            "Dental prophylaxis — calculus and plaque buildup on premolars and molars",
            "Full mouth scaling and polishing under general anaesthesia. No extractions required.",
            "Pre-anaesthetic blood panel: normal. Grade II periodontal disease. Recommend dental diet and home brushing. Follow-up in 6 months.",
            species: null, sortOrder: 3);

        AddTemplate(templates, clinicId, "Wound Treatment", TemplateCategory.Emergency,
            "Laceration / abrasion wound — location and dimensions noted",
            "Wound cleaned and debrided. Sutured with [material]. Prescribed antibiotics and pain management.",
            "Wound measurements recorded. No signs of deep tissue involvement. Elizabethan collar fitted. Suture removal in 10–14 days.",
            species: null, sortOrder: 4);

        AddTemplate(templates, clinicId, "Post-Surgery Follow-up", TemplateCategory.Surgery,
            "Post-operative follow-up — surgical site healing appropriately",
            "Sutures intact, no signs of infection. Continue current medication protocol.",
            "Incision site clean and dry. No discharge, swelling within expected range. Patient weight stable. Appetite and activity level returning to normal.",
            species: null, sortOrder: 5);

        return templates;
    }

    private static void AddTemplate(
        List<MedicalRecordTemplate> templates,
        Guid clinicId,
        string name,
        TemplateCategory category,
        string diagnosis,
        string treatment,
        string notes,
        Species? species,
        int sortOrder)
    {
        var result = MedicalRecordTemplate.Create(
            clinicId, name, category, diagnosis, treatment, notes, species, isSystemTemplate: true, sortOrder);

        if (result.IsSuccess)
            templates.Add(result.Value);
    }
}
