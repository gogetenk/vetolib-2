using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Stock.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetPrescriptionPreflight;

internal class GetPrescriptionPreflightHandler : IRequestHandler<GetPrescriptionPreflightQuery, Result<PrescriptionPreflightResult>>
{
    private readonly ISender _sender;

    public GetPrescriptionPreflightHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Result<PrescriptionPreflightResult>> Handle(GetPrescriptionPreflightQuery query, CancellationToken ct)
    {
        // Run interaction check and stock availability check in parallel
        var interactionTask = _sender.Send(new CheckInteractionsQuery(
            query.PatientId,
            query.DrugCatalogEntryId,
            query.DosageAmount,
            query.ClinicId), ct);

        var stockTask = _sender.Send(new CheckStockAvailabilityQuery(
            query.DrugCatalogEntryId,
            query.ClinicId), ct);

        await Task.WhenAll(interactionTask, stockTask);

        var interactionResult = await interactionTask;
        var stockResult = await stockTask;

        var interactionAlerts = interactionResult.IsSuccess
            ? interactionResult.Value.Alerts
            : [];

        StockAvailabilityResult? stockAvailability = stockResult.IsSuccess ? stockResult.Value : null;

        var stockDto = stockAvailability is not null
            ? new PrescriptionStockAvailability(
                stockAvailability.Available,
                stockAvailability.Quantity,
                stockAvailability.Unit,
                stockAvailability.IsLowStock,
                stockAvailability.IsExpiringSoon)
            : new PrescriptionStockAvailability(false, 0, string.Empty, true, false);

        List<SafeAlternativeDto> safeAlternatives = [];

        // If out of stock, check each alternative for interaction safety
        if (stockAvailability is not null && !stockAvailability.Available)
        {
            foreach (var alternative in stockAvailability.Alternatives)
            {
                var altInteractionResult = await _sender.Send(new CheckInteractionsQuery(
                    query.PatientId,
                    alternative.DrugCatalogEntryId,
                    query.DosageAmount,
                    query.ClinicId), ct);

                if (!altInteractionResult.IsSuccess)
                    continue;

                var altAlerts = altInteractionResult.Value.Alerts;
                var hasCriticalAlert = altAlerts.Any(a => a.Severity == InteractionSeverity.Critical);

                if (!hasCriticalAlert)
                {
                    // Get the drug name from catalog
                    var catalogResult = await _sender.Send(
                        new GetDrugCatalogEntryByIdQuery(alternative.DrugCatalogEntryId), ct);

                    var drugName = catalogResult.IsSuccess
                        ? catalogResult.Value.DisplayName
                        : alternative.Name;

                    safeAlternatives.Add(new SafeAlternativeDto(
                        alternative.DrugCatalogEntryId,
                        drugName,
                        alternative.Quantity,
                        alternative.Unit,
                        altAlerts));
                }
            }
        }

        return Result<PrescriptionPreflightResult>.Success(new PrescriptionPreflightResult(
            interactionAlerts,
            stockDto,
            safeAlternatives));
    }
}
