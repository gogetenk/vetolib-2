namespace Vetolib.Breeding.Contracts;

public record RecordDeliveryRequest(
    DateOnly DeliveryDate,
    PregnancyOutcome Outcome,
    int OffspringCount,
    string? Notes);
