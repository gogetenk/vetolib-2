using System.Text.Json.Serialization;

namespace Vetolib.MedicalRecords.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Sex { Male, Female, NeuteredMale, SpayedFemale, Unknown }
