using System.Text.Json.Serialization;

namespace Vetolib.Billing.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaxCategory
{
    Standard,
    Reduced,
    SuperReduced,
    Zero,
    Exempt
}
