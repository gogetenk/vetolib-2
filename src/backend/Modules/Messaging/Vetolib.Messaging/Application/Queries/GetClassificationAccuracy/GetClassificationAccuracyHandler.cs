using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetClassificationAccuracy;

internal class GetClassificationAccuracyHandler : IRequestHandler<GetClassificationAccuracyQuery, Result<ClassificationAccuracyDto>>
{
    private readonly MessagingDbContext _context;

    public GetClassificationAccuracyHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClassificationAccuracyDto>> Handle(
        GetClassificationAccuracyQuery request,
        CancellationToken ct)
    {
        // Get all classified messages (those that have AI classification)
        var classifiedMessages = await _context.Messages
            .Where(m => m.ClassifiedUrgency != null)
            .Select(m => new
            {
                m.ClassificationFeedbackCorrect,
                m.OriginalAiCategory,
                m.ClassifiedCategory,
                HasOverride = m.OverriddenByUserId != null
            })
            .ToListAsync(ct);

        var totalClassified = classifiedMessages.Count;

        if (totalClassified == 0)
        {
            return Result<ClassificationAccuracyDto>.Success(new ClassificationAccuracyDto(
                AccuracyRate: 0,
                TotalClassified: 0,
                TotalCorrected: 0,
                MostCommonCorrections: []));
        }

        // Messages with explicit feedback
        var withFeedback = classifiedMessages
            .Where(m => m.ClassificationFeedbackCorrect.HasValue)
            .ToList();

        // Messages that were overridden (vet corrected the classification)
        var overridden = classifiedMessages
            .Where(m => m.HasOverride && m.OriginalAiCategory.HasValue)
            .ToList();

        // Total corrected = overrides + explicit "incorrect" feedback (avoid double-counting)
        var totalCorrected = overridden.Count +
            withFeedback.Count(m => m.ClassificationFeedbackCorrect == false && !m.HasOverride);

        // Accuracy: messages confirmed correct / (confirmed correct + corrected)
        var confirmedCorrect = withFeedback.Count(m => m.ClassificationFeedbackCorrect == true);
        var totalEvaluated = confirmedCorrect + totalCorrected;
        var accuracyRate = totalEvaluated > 0
            ? (double)confirmedCorrect / totalEvaluated
            : 0;

        // Most common corrections (from overrides where original category differs)
        var corrections = overridden
            .Where(m => m.OriginalAiCategory != m.ClassifiedCategory)
            .GroupBy(m => new { From = m.OriginalAiCategory!.Value, To = m.ClassifiedCategory!.Value })
            .Select(g => new CategoryCorrectionDto(g.Key.From, g.Key.To, g.Count()))
            .OrderByDescending(c => c.Count)
            .Take(10)
            .ToList()
            .AsReadOnly();

        return Result<ClassificationAccuracyDto>.Success(new ClassificationAccuracyDto(
            AccuracyRate: Math.Round(accuracyRate, 4),
            TotalClassified: totalClassified,
            TotalCorrected: totalCorrected,
            MostCommonCorrections: corrections));
    }
}
