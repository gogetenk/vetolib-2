using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetDrugAlternatives;

internal class GetDrugAlternativesHandler : IRequestHandler<GetDrugAlternativesQuery, Result<List<AlternativeDrugDto>>>
{
    private const int MaxResults = 10;
    private readonly MedicalRecordsDbContext _context;

    public GetDrugAlternativesHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AlternativeDrugDto>>> Handle(
        GetDrugAlternativesQuery request,
        CancellationToken cancellationToken)
    {
        // Load the source drug with its interactions
        var sourceDrug = await _context.DrugCatalogEntries
            .AsNoTracking()
            .Include(d => d.Interactions)
            .FirstOrDefaultAsync(d => d.Id == request.DrugId && d.IsActive, cancellationToken);

        if (sourceDrug is null)
            return Result<List<AlternativeDrugDto>>.NotFound("Drug not found");

        // Collect DrugCatalogEntryIds from patient's current prescriptions (if patientId provided)
        var patientDrugIds = new HashSet<Guid>();
        if (request.PatientId.HasValue)
        {
            patientDrugIds = (await _context.Prescriptions
                .AsNoTracking()
                .Where(p => _context.MedicalRecords
                    .Any(mr => mr.Id == p.MedicalRecordId && mr.PatientId == request.PatientId.Value))
                .Where(p => p.DrugCatalogEntryId.HasValue)
                .Select(p => p.DrugCatalogEntryId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken))
                .ToHashSet();
        }

        // Load interactions for all drugs the patient is currently taking
        var patientDrugInteractions = new HashSet<Guid>();
        if (patientDrugIds.Count > 0)
        {
            var interactions = await _context.DrugCatalogEntries
                .AsNoTracking()
                .Include(d => d.Interactions)
                .Where(d => patientDrugIds.Contains(d.Id))
                .SelectMany(d => d.Interactions)
                .Select(i => i.OtherDrugId)
                .ToListAsync(cancellationToken);

            patientDrugInteractions = interactions.ToHashSet();
        }

        // Find candidate alternatives: same category, active, not the source drug
        var candidates = await _context.DrugCatalogEntries
            .AsNoTracking()
            .Include(d => d.Interactions)
            .Where(d => d.IsActive && d.Id != request.DrugId)
            .Where(d => d.Category == sourceDrug.Category || d.InnName == sourceDrug.InnName)
            .ToListAsync(cancellationToken);

        // Filter out drugs with known interactions with patient's current prescriptions
        var alternatives = new List<(DrugCatalogEntry Drug, int SortPriority, bool HasInteraction)>();

        foreach (var candidate in candidates)
        {
            var hasInteractionWithPatient = patientDrugInteractions.Contains(candidate.Id)
                || candidate.Interactions.Any(i => patientDrugIds.Contains(i.OtherDrugId));

            // Exclude drugs with interactions when patient context is provided
            if (request.PatientId.HasValue && hasInteractionWithPatient)
                continue;

            // Sort priority: 0 = same active ingredient, 1 = same category
            var sortPriority = candidate.InnName.Equals(sourceDrug.InnName, StringComparison.OrdinalIgnoreCase)
                ? 0
                : 1;

            alternatives.Add((candidate, sortPriority, hasInteractionWithPatient));
        }

        // Sort: same active ingredient first, then same category, then alphabetically
        var result = alternatives
            .OrderBy(a => a.SortPriority)
            .ThenBy(a => a.Drug.DisplayName)
            .Take(MaxResults)
            .Select(a => ToAlternativeDto(a.Drug, sourceDrug, a.HasInteraction))
            .ToList();

        return Result<List<AlternativeDrugDto>>.Success(result);
    }

    internal static AlternativeDrugDto ToAlternativeDto(
        DrugCatalogEntry candidate,
        DrugCatalogEntry sourceDrug,
        bool hasInteraction)
    {
        // A drug is considered generic if its DisplayName matches or starts with its INN name
        var isGeneric = candidate.DisplayName.StartsWith(candidate.InnName, StringComparison.OrdinalIgnoreCase);

        // Derive formulation type from display name patterns
        var formulationType = DeriveFormulationType(candidate.DisplayName);

        // Price indicator: generics are typically lower, same INN = same, branded = higher
        var priceIndicator = DeterminePriceIndicator(candidate, sourceDrug, isGeneric);

        return new AlternativeDrugDto(
            DrugId: candidate.Id,
            Name: candidate.DisplayName,
            ActiveIngredient: candidate.InnName,
            Category: candidate.Category,
            FormulationType: formulationType,
            IsGeneric: isGeneric,
            PriceIndicator: priceIndicator,
            HasInteraction: hasInteraction);
    }

    internal static string DeriveFormulationType(string displayName)
    {
        var lower = displayName.ToLowerInvariant();

        if (lower.Contains("mg/ml") || lower.Contains("injectable") || lower.Contains("injection"))
            return "Injectable";
        if (lower.Contains("oral") || lower.Contains("suspension") || lower.Contains("syrup"))
            return "Oral Liquid";
        if (lower.Contains("tablet") || lower.Contains("tab") || lower.Contains("mg"))
            return "Tablet";
        if (lower.Contains("cream") || lower.Contains("ointment") || lower.Contains("topical"))
            return "Topical";
        if (lower.Contains("spray") || lower.Contains("inhaler"))
            return "Inhalation";
        if (lower.Contains("patch"))
            return "Transdermal";
        if (lower.Contains("drop") || lower.Contains("ophthalmic"))
            return "Ophthalmic";

        return "Other";
    }

    internal static PriceIndicator DeterminePriceIndicator(
        DrugCatalogEntry candidate,
        DrugCatalogEntry sourceDrug,
        bool isGeneric)
    {
        // Same active ingredient: generic = lower price, branded = higher
        if (candidate.InnName.Equals(sourceDrug.InnName, StringComparison.OrdinalIgnoreCase))
        {
            return isGeneric ? PriceIndicator.Lower : PriceIndicator.Same;
        }

        // Different active ingredient in same category: default to Same
        return PriceIndicator.Same;
    }
}
