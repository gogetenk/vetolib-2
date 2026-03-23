using System.Text.Json.Serialization;

namespace Vetolib.Billing.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    Service,
    Goods,
    Mixed
}
