using System.Text.Json.Serialization;

namespace Vetolib.Billing.Contracts.EInvoicing;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EInvoicingPlatformStatus
{
    Submitted,
    Accepted,
    Rejected,
    Refused,
    Received
}
