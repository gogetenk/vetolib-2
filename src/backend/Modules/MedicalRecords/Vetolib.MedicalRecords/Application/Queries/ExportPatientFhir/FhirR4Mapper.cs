using System.Text.Json;
using System.Text.Json.Nodes;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;

/// <summary>
/// Maps Vetolib domain entities to FHIR R4 JSON resources.
/// Built with System.Text.Json — no Hl7.Fhir.R4 dependency.
/// </summary>
internal static class FhirR4Mapper
{
    private const string FhirPatientAnimalExtensionUrl = "http://hl7.org/fhir/StructureDefinition/patient-animal";
    private const string MicrochipIdentifierSystem = "urn:iso:std:iso:11784";
    private const string LoincBodyWeightCode = "29463-7";
    private const string LoincSystemUrl = "http://loinc.org";
    private const string UnitsOfMeasureSystem = "http://unitsofmeasure.org";
    private const string SnomedSystemUrl = "http://snomed.info/sct";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Creates a FHIR R4 Bundle of type "document" containing all resources for a patient export.
    /// </summary>
    public static string BuildBundle(
        Patient patient,
        Owner? owner,
        IReadOnlyList<MedicalRecord> medicalRecords,
        IReadOnlyList<WeightEntry> weightEntries)
    {
        var bundle = new JsonObject
        {
            ["resourceType"] = "Bundle",
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "collection",
            ["timestamp"] = DateTime.UtcNow.ToString("O"),
            ["entry"] = BuildEntries(patient, owner, medicalRecords, weightEntries)
        };

        return bundle.ToJsonString(SerializerOptions);
    }

    private static JsonArray BuildEntries(
        Patient patient,
        Owner? owner,
        IReadOnlyList<MedicalRecord> medicalRecords,
        IReadOnlyList<WeightEntry> weightEntries)
    {
        var entries = new JsonArray();

        // Patient resource
        entries.Add(WrapEntry($"urn:uuid:{patient.Id}", MapPatient(patient)));

        // Owner as RelatedPerson
        if (owner is not null)
        {
            entries.Add(WrapEntry($"urn:uuid:{owner.Id}", MapOwnerToRelatedPerson(owner, patient.Id)));
        }

        // Medical records as Encounters
        foreach (var record in medicalRecords)
        {
            entries.Add(WrapEntry($"urn:uuid:{record.Id}", MapMedicalRecordToEncounter(record, patient.Id)));

            // Prescriptions as MedicationRequests
            foreach (var prescription in record.Prescriptions)
            {
                entries.Add(WrapEntry($"urn:uuid:{prescription.Id}", MapPrescriptionToMedicationRequest(prescription, patient.Id, record.Id)));
            }
        }

        // Weight entries as Observations
        foreach (var weight in weightEntries)
        {
            entries.Add(WrapEntry($"urn:uuid:{weight.Id}", MapWeightEntryToObservation(weight, patient.Id)));
        }

        return entries;
    }

    private static JsonObject WrapEntry(string fullUrl, JsonObject resource)
    {
        return new JsonObject
        {
            ["fullUrl"] = fullUrl,
            ["resource"] = resource
        };
    }

    internal static JsonObject MapPatient(Patient patient)
    {
        var resource = new JsonObject
        {
            ["resourceType"] = "Patient",
            ["id"] = patient.Id.ToString(),
            ["name"] = new JsonArray
            {
                new JsonObject
                {
                    ["text"] = patient.Name,
                    ["given"] = new JsonArray { JsonValue.Create(patient.Name) }
                }
            },
            ["gender"] = MapSexToFhirGender(patient.Sex),
            ["birthDate"] = patient.BirthDate.ToString("yyyy-MM-dd")
        };

        // patient-animal extension (species + breed)
        var animalExtension = BuildPatientAnimalExtension(patient.Species, patient.Breed);
        resource["extension"] = new JsonArray { animalExtension };

        // Microchip identifier
        if (!string.IsNullOrEmpty(patient.MicrochipNumber))
        {
            resource["identifier"] = new JsonArray
            {
                new JsonObject
                {
                    ["system"] = MicrochipIdentifierSystem,
                    ["value"] = patient.MicrochipNumber
                }
            };
        }

        return resource;
    }

    internal static JsonObject BuildPatientAnimalExtension(Species species, string breed)
    {
        var extensions = new JsonArray
        {
            new JsonObject
            {
                ["url"] = "species",
                ["valueCodeableConcept"] = new JsonObject
                {
                    ["coding"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["system"] = SnomedSystemUrl,
                            ["code"] = MapSpeciesToSnomedCode(species),
                            ["display"] = species.ToString()
                        }
                    },
                    ["text"] = species.ToString()
                }
            },
            new JsonObject
            {
                ["url"] = "breed",
                ["valueCodeableConcept"] = new JsonObject
                {
                    ["text"] = breed
                }
            }
        };

        return new JsonObject
        {
            ["url"] = FhirPatientAnimalExtensionUrl,
            ["extension"] = extensions
        };
    }

    internal static JsonObject MapOwnerToRelatedPerson(Owner owner, Guid patientId)
    {
        var resource = new JsonObject
        {
            ["resourceType"] = "RelatedPerson",
            ["id"] = owner.Id.ToString(),
            ["patient"] = new JsonObject
            {
                ["reference"] = $"Patient/{patientId}"
            },
            ["relationship"] = new JsonArray
            {
                new JsonObject
                {
                    ["coding"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["system"] = "http://terminology.hl7.org/CodeSystem/v3-RoleCode",
                            ["code"] = "O",
                            ["display"] = "Owner"
                        }
                    }
                }
            },
            ["name"] = new JsonArray
            {
                new JsonObject
                {
                    ["family"] = owner.LastName,
                    ["given"] = new JsonArray { JsonValue.Create(owner.FirstName) }
                }
            }
        };

        // Telecom (email + phone)
        var telecom = new JsonArray();

        if (!string.IsNullOrEmpty(owner.Email))
        {
            telecom.Add(new JsonObject
            {
                ["system"] = "email",
                ["value"] = owner.Email
            });
        }

        if (!string.IsNullOrEmpty(owner.Phone))
        {
            telecom.Add(new JsonObject
            {
                ["system"] = "phone",
                ["value"] = owner.Phone
            });
        }

        if (telecom.Count > 0)
            resource["telecom"] = telecom;

        return resource;
    }

    internal static JsonObject MapMedicalRecordToEncounter(MedicalRecord record, Guid patientId)
    {
        return new JsonObject
        {
            ["resourceType"] = "Encounter",
            ["id"] = record.Id.ToString(),
            ["status"] = "finished",
            ["class"] = new JsonObject
            {
                ["system"] = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                ["code"] = "AMB",
                ["display"] = "ambulatory"
            },
            ["subject"] = new JsonObject
            {
                ["reference"] = $"Patient/{patientId}"
            },
            ["participant"] = new JsonArray
            {
                new JsonObject
                {
                    ["individual"] = new JsonObject
                    {
                        ["display"] = record.VetName
                    }
                }
            },
            ["period"] = new JsonObject
            {
                ["start"] = record.ExaminedAt.ToString("O")
            },
            ["reasonCode"] = new JsonArray
            {
                new JsonObject
                {
                    ["text"] = record.Diagnosis
                }
            },
            ["text"] = new JsonObject
            {
                ["status"] = "generated",
                ["div"] = $"<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Diagnosis: {EscapeHtml(record.Diagnosis)}</p><p>Treatment: {EscapeHtml(record.Treatment)}</p></div>"
            }
        };
    }

    internal static JsonObject MapPrescriptionToMedicationRequest(Prescription prescription, Guid patientId, Guid encounterId)
    {
        return new JsonObject
        {
            ["resourceType"] = "MedicationRequest",
            ["id"] = prescription.Id.ToString(),
            ["status"] = "active",
            ["intent"] = "order",
            ["medicationCodeableConcept"] = new JsonObject
            {
                ["text"] = prescription.Medication
            },
            ["subject"] = new JsonObject
            {
                ["reference"] = $"Patient/{patientId}"
            },
            ["encounter"] = new JsonObject
            {
                ["reference"] = $"Encounter/{encounterId}"
            },
            ["dosageInstruction"] = new JsonArray
            {
                new JsonObject
                {
                    ["text"] = prescription.Dosage
                }
            },
            ["requester"] = new JsonObject
            {
                ["display"] = prescription.VetLicenseNumber
            }
        };
    }

    internal static JsonObject MapWeightEntryToObservation(WeightEntry weightEntry, Guid patientId)
    {
        var resource = new JsonObject
        {
            ["resourceType"] = "Observation",
            ["id"] = weightEntry.Id.ToString(),
            ["status"] = "final",
            ["code"] = new JsonObject
            {
                ["coding"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["system"] = LoincSystemUrl,
                        ["code"] = LoincBodyWeightCode,
                        ["display"] = "Body weight"
                    }
                }
            },
            ["subject"] = new JsonObject
            {
                ["reference"] = $"Patient/{patientId}"
            },
            ["effectiveDateTime"] = weightEntry.RecordedAt.ToString("O"),
            ["valueQuantity"] = new JsonObject
            {
                ["value"] = weightEntry.WeightKg,
                ["unit"] = "kg",
                ["system"] = UnitsOfMeasureSystem,
                ["code"] = "kg"
            }
        };

        if (!string.IsNullOrEmpty(weightEntry.Note))
        {
            resource["note"] = new JsonArray
            {
                new JsonObject
                {
                    ["text"] = weightEntry.Note
                }
            };
        }

        return resource;
    }

    internal static string MapSexToFhirGender(Sex sex)
    {
        return sex switch
        {
            Sex.Male => "male",
            Sex.Female => "female",
            Sex.NeuteredMale => "male",
            Sex.SpayedFemale => "female",
            _ => "unknown"
        };
    }

    internal static string MapSpeciesToSnomedCode(Species species)
    {
        return species switch
        {
            Species.Dog => "448771007",
            Species.Cat => "448169003",
            Species.Bird => "387972009",
            Species.Rabbit => "388814007",
            Species.Horse => "388445009",
            Species.Camel => "422856003",
            Species.Falcon => "8960001",
            Species.Reptile => "107241004",
            _ => "387961004" // animal (generic)
        };
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}
