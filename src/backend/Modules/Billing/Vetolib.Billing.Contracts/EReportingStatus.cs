using System.Text.Json.Serialization;

namespace Vetolib.Billing.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EReportingStatus
{
    Draft,
    Submitted,
    Accepted,
    Rejected
}
