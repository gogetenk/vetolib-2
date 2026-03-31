using System.Text.Json;
using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;

/// <summary>
/// Parses a FHIR R4 JSON Bundle into intermediate DTOs for import.
/// Reverse of FhirR4Mapper — built with System.Text.Json, no Hl7.Fhir.R4 dependency.
/// </summary>
internal static class FhirR4BundleParser
{
    private const string MicrochipIdentifierSystem = "urn:iso:std:iso:11784";
    private const string SnomedSystemUrl = "http://snomed.info/sct";
    private const string LoincBodyWeightCode = "29463-7";

    internal record ParsedFhirBundle(
        FhirPatientData Patient,
        IReadOnlyList<FhirEncounterData> Encounters,
        IReadOnlyList<FhirMedicationRequestData> MedicationRequests,
        IReadOnlyList<FhirWeightObservationData> WeightObservations);

    internal record FhirPatientData(
        string Name,
        Species Species,
        string Breed,
        DateOnly BirthDate,
        Sex Sex,
        string? MicrochipNumber);

    internal record FhirEncounterData(
        string FhirId,
        string Diagnosis,
        string Treatment,
        string VetName,
        DateTime ExaminedAt);

    internal record FhirMedicationRequestData(
        string EncounterFhirId,
        string Medication,
        string Dosage,
        string VetLicenseNumber);

    internal record FhirWeightObservationData(
        decimal WeightKg,
        DateTime RecordedAt,
        string? Note);

    internal static Result<ParsedFhirBundle> Parse(string json)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            return Result<ParsedFhirBundle>.Invalid(
                new ValidationError("fhirBundle", $"Invalid JSON: {ex.Message}"));
        }

        var root = doc.RootElement;

        if (!root.TryGetProperty("resourceType", out var resourceType) ||
            resourceType.GetString() != "Bundle")
        {
            return Result<ParsedFhirBundle>.Invalid(
                new ValidationError("fhirBundle", "Root resource must be a FHIR Bundle (resourceType: 'Bundle')"));
        }

        if (!root.TryGetProperty("entry", out var entries) ||
            entries.ValueKind != JsonValueKind.Array ||
            entries.GetArrayLength() == 0)
        {
            return Result<ParsedFhirBundle>.Invalid(
                new ValidationError("fhirBundle", "Bundle must contain at least one entry"));
        }

        FhirPatientData? patient = null;
        var encounters = new List<FhirEncounterData>();
        var medicationRequests = new List<FhirMedicationRequestData>();
        var weightObservations = new List<FhirWeightObservationData>();
        var warnings = new List<string>();

        foreach (var entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty("resource", out var resource))
                continue;

            if (!resource.TryGetProperty("resourceType", out var resType))
                continue;

            var type = resType.GetString();

            switch (type)
            {
                case "Patient":
                    var patientResult = ParsePatientResource(resource);
                    if (patientResult.IsSuccess)
                        patient = patientResult.Value;
                    else
                        return Result<ParsedFhirBundle>.Invalid(patientResult.ValidationErrors.ToList());
                    break;

                case "Encounter":
                    var encounterResult = ParseEncounterResource(resource);
                    if (encounterResult.IsSuccess)
                        encounters.Add(encounterResult.Value);
                    break;

                case "MedicationRequest":
                    var medResult = ParseMedicationRequestResource(resource);
                    if (medResult.IsSuccess)
                        medicationRequests.Add(medResult.Value);
                    break;

                case "Observation":
                    var obsResult = ParseWeightObservation(resource);
                    if (obsResult.IsSuccess)
                        weightObservations.Add(obsResult.Value);
                    break;
            }
        }

        if (patient is null)
        {
            return Result<ParsedFhirBundle>.Invalid(
                new ValidationError("fhirBundle", "Bundle must contain at least one Patient resource"));
        }

        return Result<ParsedFhirBundle>.Success(
            new ParsedFhirBundle(patient, encounters, medicationRequests, weightObservations));
    }

    internal static Result<FhirPatientData> ParsePatientResource(JsonElement resource)
    {
        // Name
        var name = "Unknown";
        if (resource.TryGetProperty("name", out var names) &&
            names.ValueKind == JsonValueKind.Array &&
            names.GetArrayLength() > 0)
        {
            var nameObj = names[0];
            if (nameObj.TryGetProperty("text", out var text))
                name = text.GetString() ?? "Unknown";
            else if (nameObj.TryGetProperty("given", out var given) &&
                     given.ValueKind == JsonValueKind.Array &&
                     given.GetArrayLength() > 0)
                name = given[0].GetString() ?? "Unknown";
        }

        // Species + Breed from patient-animal extension
        var species = Species.Exotic; // default
        var breed = "Unknown";

        if (resource.TryGetProperty("extension", out var extensions) &&
            extensions.ValueKind == JsonValueKind.Array)
        {
            foreach (var ext in extensions.EnumerateArray())
            {
                if (ext.TryGetProperty("url", out var url) &&
                    url.GetString() == "http://hl7.org/fhir/StructureDefinition/patient-animal" &&
                    ext.TryGetProperty("extension", out var subExtensions))
                {
                    foreach (var subExt in subExtensions.EnumerateArray())
                    {
                        var subUrl = subExt.TryGetProperty("url", out var u) ? u.GetString() : null;

                        if (subUrl == "species" &&
                            subExt.TryGetProperty("valueCodeableConcept", out var speciesConcept))
                        {
                            species = ParseSpeciesFromCodeableConcept(speciesConcept);
                        }
                        else if (subUrl == "breed" &&
                                 subExt.TryGetProperty("valueCodeableConcept", out var breedConcept))
                        {
                            if (breedConcept.TryGetProperty("text", out var breedText))
                                breed = breedText.GetString() ?? "Unknown";
                        }
                    }
                }
            }
        }

        // Gender → Sex
        var sex = Sex.Unknown;
        if (resource.TryGetProperty("gender", out var gender))
        {
            sex = MapFhirGenderToSex(gender.GetString());
        }

        // BirthDate
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow); // fallback
        if (resource.TryGetProperty("birthDate", out var bd))
        {
            if (DateOnly.TryParse(bd.GetString(), out var parsed))
                birthDate = parsed;
        }

        // Microchip from identifier
        string? microchip = null;
        if (resource.TryGetProperty("identifier", out var identifiers) &&
            identifiers.ValueKind == JsonValueKind.Array)
        {
            foreach (var id in identifiers.EnumerateArray())
            {
                if (id.TryGetProperty("system", out var sys) &&
                    sys.GetString() == MicrochipIdentifierSystem &&
                    id.TryGetProperty("value", out var val))
                {
                    microchip = val.GetString();
                    break;
                }
            }
        }

        return Result<FhirPatientData>.Success(
            new FhirPatientData(name, species, breed, birthDate, sex, microchip));
    }

    internal static Result<FhirEncounterData> ParseEncounterResource(JsonElement resource)
    {
        var fhirId = resource.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";

        // Diagnosis from reasonCode
        var diagnosis = "Imported encounter";
        if (resource.TryGetProperty("reasonCode", out var reasonCodes) &&
            reasonCodes.ValueKind == JsonValueKind.Array &&
            reasonCodes.GetArrayLength() > 0)
        {
            var first = reasonCodes[0];
            if (first.TryGetProperty("text", out var reasonText))
                diagnosis = reasonText.GetString() ?? diagnosis;
        }

        // Treatment from text.div (best effort: strip HTML)
        var treatment = "Imported treatment";
        if (resource.TryGetProperty("text", out var textObj) &&
            textObj.TryGetProperty("div", out var div))
        {
            var rawHtml = div.GetString() ?? "";
            // Extract treatment from the div: look for "Treatment: ..."
            var treatmentIdx = rawHtml.IndexOf("Treatment:", StringComparison.OrdinalIgnoreCase);
            if (treatmentIdx >= 0)
            {
                var afterTreatment = rawHtml[(treatmentIdx + "Treatment:".Length)..];
                treatment = StripHtml(afterTreatment).Trim();
                if (string.IsNullOrWhiteSpace(treatment))
                    treatment = "Imported treatment";
            }
        }

        // VetName from participant
        var vetName = "Imported vet";
        if (resource.TryGetProperty("participant", out var participants) &&
            participants.ValueKind == JsonValueKind.Array &&
            participants.GetArrayLength() > 0)
        {
            var first = participants[0];
            if (first.TryGetProperty("individual", out var individual) &&
                individual.TryGetProperty("display", out var display))
            {
                vetName = display.GetString() ?? vetName;
            }
        }

        // ExaminedAt from period.start
        var examinedAt = DateTime.UtcNow;
        if (resource.TryGetProperty("period", out var period) &&
            period.TryGetProperty("start", out var start))
        {
            if (DateTime.TryParse(start.GetString(), out var parsed))
                examinedAt = parsed.ToUniversalTime();
        }

        return Result<FhirEncounterData>.Success(
            new FhirEncounterData(fhirId, diagnosis, treatment, vetName, examinedAt));
    }

    internal static Result<FhirMedicationRequestData> ParseMedicationRequestResource(JsonElement resource)
    {
        // Encounter reference
        var encounterFhirId = "";
        if (resource.TryGetProperty("encounter", out var encounter) &&
            encounter.TryGetProperty("reference", out var encRef))
        {
            var refStr = encRef.GetString() ?? "";
            // Format: "Encounter/{id}"
            if (refStr.StartsWith("Encounter/", StringComparison.OrdinalIgnoreCase))
                encounterFhirId = refStr["Encounter/".Length..];
        }

        // Medication
        var medication = "Unknown medication";
        if (resource.TryGetProperty("medicationCodeableConcept", out var medConcept) &&
            medConcept.TryGetProperty("text", out var medText))
        {
            medication = medText.GetString() ?? medication;
        }

        // Dosage
        var dosage = "See original prescription";
        if (resource.TryGetProperty("dosageInstruction", out var dosageInstructions) &&
            dosageInstructions.ValueKind == JsonValueKind.Array &&
            dosageInstructions.GetArrayLength() > 0)
        {
            var first = dosageInstructions[0];
            if (first.TryGetProperty("text", out var dosageText))
                dosage = dosageText.GetString() ?? dosage;
        }

        // Vet license number from requester
        var vetLicense = "IMPORTED";
        if (resource.TryGetProperty("requester", out var requester) &&
            requester.TryGetProperty("display", out var display))
        {
            vetLicense = display.GetString() ?? vetLicense;
        }

        return Result<FhirMedicationRequestData>.Success(
            new FhirMedicationRequestData(encounterFhirId, medication, dosage, vetLicense));
    }

    internal static Result<FhirWeightObservationData> ParseWeightObservation(JsonElement resource)
    {
        // Check if this is a body-weight observation
        if (resource.TryGetProperty("code", out var code) &&
            code.TryGetProperty("coding", out var codings) &&
            codings.ValueKind == JsonValueKind.Array)
        {
            var isBodyWeight = false;
            foreach (var coding in codings.EnumerateArray())
            {
                if (coding.TryGetProperty("code", out var c) &&
                    c.GetString() == LoincBodyWeightCode)
                {
                    isBodyWeight = true;
                    break;
                }
            }

            if (!isBodyWeight)
                return Result<FhirWeightObservationData>.Error("Not a body-weight observation");
        }
        else
        {
            return Result<FhirWeightObservationData>.Error("Not a body-weight observation");
        }

        // Weight from valueQuantity
        decimal weightKg = 0;
        if (resource.TryGetProperty("valueQuantity", out var quantity) &&
            quantity.TryGetProperty("value", out var val))
        {
            weightKg = val.GetDecimal();

            // Convert to kg if needed
            if (quantity.TryGetProperty("code", out var unitCode))
            {
                var unit = unitCode.GetString();
                if (unit == "g")
                    weightKg /= 1000m;
                else if (unit == "[lb_av]" || unit == "lb")
                    weightKg *= 0.453592m;
            }
        }

        if (weightKg <= 0)
            return Result<FhirWeightObservationData>.Error("Weight must be positive");

        // RecordedAt from effectiveDateTime
        var recordedAt = DateTime.UtcNow;
        if (resource.TryGetProperty("effectiveDateTime", out var effectiveDt))
        {
            if (DateTime.TryParse(effectiveDt.GetString(), out var parsed))
                recordedAt = parsed.ToUniversalTime();
        }

        // Note
        string? note = null;
        if (resource.TryGetProperty("note", out var notes) &&
            notes.ValueKind == JsonValueKind.Array &&
            notes.GetArrayLength() > 0)
        {
            var first = notes[0];
            if (first.TryGetProperty("text", out var noteText))
                note = noteText.GetString();
        }

        return Result<FhirWeightObservationData>.Success(
            new FhirWeightObservationData(weightKg, recordedAt, note));
    }

    internal static Species ParseSpeciesFromCodeableConcept(JsonElement concept)
    {
        // Try SNOMED code first
        if (concept.TryGetProperty("coding", out var codings) &&
            codings.ValueKind == JsonValueKind.Array)
        {
            foreach (var coding in codings.EnumerateArray())
            {
                if (coding.TryGetProperty("system", out var sys) &&
                    sys.GetString() == SnomedSystemUrl &&
                    coding.TryGetProperty("code", out var code))
                {
                    var mapped = MapSnomedCodeToSpecies(code.GetString());
                    if (mapped.HasValue)
                        return mapped.Value;
                }
            }
        }

        // Fallback: try text/display
        if (concept.TryGetProperty("text", out var text))
        {
            var mapped = MapDisplayToSpecies(text.GetString());
            if (mapped.HasValue)
                return mapped.Value;
        }

        return Species.Exotic;
    }

    internal static Species? MapSnomedCodeToSpecies(string? code)
    {
        return code switch
        {
            "448771007" => Species.Dog,
            "448169003" => Species.Cat,
            "387972009" => Species.Bird,
            "388814007" => Species.Rabbit,
            "388445009" => Species.Horse,
            "422856003" => Species.Camel,
            "8960001" => Species.Falcon,
            "107241004" => Species.Reptile,
            _ => null
        };
    }

    internal static Species? MapDisplayToSpecies(string? display)
    {
        if (string.IsNullOrWhiteSpace(display))
            return null;

        return display.Trim().ToLowerInvariant() switch
        {
            "dog" => Species.Dog,
            "cat" => Species.Cat,
            "bird" => Species.Bird,
            "rabbit" => Species.Rabbit,
            "horse" => Species.Horse,
            "camel" => Species.Camel,
            "falcon" => Species.Falcon,
            "reptile" => Species.Reptile,
            _ => null
        };
    }

    internal static Sex MapFhirGenderToSex(string? gender)
    {
        return gender switch
        {
            "male" => Sex.Male,
            "female" => Sex.Female,
            _ => Sex.Unknown
        };
    }

    private static string StripHtml(string html)
    {
        // Simple HTML tag removal for narrative text
        var result = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
        return System.Net.WebUtility.HtmlDecode(result);
    }
}
