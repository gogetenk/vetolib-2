namespace Vetolib.Messaging.Contracts;

public record ClassifyMessageOverrideRequest(
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category
);
