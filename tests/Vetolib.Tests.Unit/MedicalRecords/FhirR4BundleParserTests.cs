using System.Text.Json;
using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class FhirR4BundleParserTests
{
    [Fact]
    public void Parse_InvalidJson_ReturnsInvalid()
    {
        var result = FhirR4BundleParser.Parse("not json at all");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Invalid JSON"));
    }

    [Fact]
    public void Parse_NotABundle_ReturnsInvalid()
    {
        var json = """{"resourceType": "Patient", "id": "123"}""";

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("FHIR Bundle"));
    }

    [Fact]
    public void Parse_EmptyEntries_ReturnsInvalid()
    {
        var json = """{"resourceType": "Bundle", "type": "collection", "entry": []}""";

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("at least one entry"));
    }

    [Fact]
    public void Parse_NoPatientResource_ReturnsInvalid()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "type": "collection",
            "entry": [
                {
                    "resource": {
                        "resourceType": "Encounter",
                        "id": "enc-1",
                        "status": "finished"
                    }
                }
            ]
        }
        """;

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage.Contains("Patient resource"));
    }

    [Fact]
    public void Parse_MinimalPatientBundle_Succeeds()
    {
        var json = BuildMinimalPatientBundle();

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patient.Name.Should().Be("Buddy");
        result.Value.Patient.Species.Should().Be(Species.Dog);
        result.Value.Patient.Breed.Should().Be("Golden Retriever");
        result.Value.Patient.Sex.Should().Be(Sex.Male);
        result.Value.Patient.BirthDate.Should().Be(new DateOnly(2020, 5, 10));
        result.Value.Patient.MicrochipNumber.Should().Be("123456789012345");
    }

    [Fact]
    public void Parse_FullBundle_ExtractsAllResources()
    {
        var json = BuildFullBundle();

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patient.Should().NotBeNull();
        result.Value.Encounters.Should().HaveCount(1);
        result.Value.MedicationRequests.Should().HaveCount(1);
        result.Value.WeightObservations.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_EncounterExtraction_ParsesFieldsCorrectly()
    {
        var json = BuildFullBundle();

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        var encounter = result.Value.Encounters[0];
        encounter.Diagnosis.Should().Be("Annual checkup");
        encounter.VetName.Should().Be("Dr. Al-Rashidi");
        encounter.Treatment.Should().Contain("Vaccination administered");
    }

    [Fact]
    public void Parse_MedicationRequestExtraction_ParsesFieldsCorrectly()
    {
        var json = BuildFullBundle();

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        var medReq = result.Value.MedicationRequests[0];
        medReq.Medication.Should().Be("Amoxicillin");
        medReq.Dosage.Should().Be("250mg twice daily");
        medReq.VetLicenseNumber.Should().Be("LIC-12345");
    }

    [Fact]
    public void Parse_WeightObservation_ParsesFieldsCorrectly()
    {
        var json = BuildFullBundle();

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        var weight = result.Value.WeightObservations[0];
        weight.WeightKg.Should().Be(25.5m);
        weight.Note.Should().Be("Post-surgery weight");
    }

    [Fact]
    public void Parse_PatientWithoutMicrochip_ReturnsNullMicrochip()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "type": "collection",
            "entry": [
                {
                    "resource": {
                        "resourceType": "Patient",
                        "id": "pat-1",
                        "name": [{"text": "Luna"}],
                        "gender": "female",
                        "birthDate": "2021-01-15",
                        "extension": [{
                            "url": "http://hl7.org/fhir/StructureDefinition/patient-animal",
                            "extension": [
                                {"url": "species", "valueCodeableConcept": {"text": "Cat"}},
                                {"url": "breed", "valueCodeableConcept": {"text": "Siamese"}}
                            ]
                        }]
                    }
                }
            ]
        }
        """;

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patient.MicrochipNumber.Should().BeNull();
        result.Value.Patient.Species.Should().Be(Species.Cat);
    }

    [Theory]
    [InlineData("448771007", Species.Dog)]
    [InlineData("448169003", Species.Cat)]
    [InlineData("388445009", Species.Horse)]
    [InlineData("422856003", Species.Camel)]
    [InlineData("8960001", Species.Falcon)]
    [InlineData("107241004", Species.Reptile)]
    [InlineData("387972009", Species.Bird)]
    [InlineData("388814007", Species.Rabbit)]
    [InlineData("999999999", null)]
    public void MapSnomedCodeToSpecies_MapsCorrectly(string code, Species? expected)
    {
        FhirR4BundleParser.MapSnomedCodeToSpecies(code).Should().Be(expected);
    }

    [Theory]
    [InlineData("Dog", Species.Dog)]
    [InlineData("cat", Species.Cat)]
    [InlineData("HORSE", Species.Horse)]
    [InlineData("unknown_animal", null)]
    [InlineData(null, null)]
    public void MapDisplayToSpecies_MapsCorrectly(string? display, Species? expected)
    {
        FhirR4BundleParser.MapDisplayToSpecies(display).Should().Be(expected);
    }

    [Theory]
    [InlineData("male", Sex.Male)]
    [InlineData("female", Sex.Female)]
    [InlineData("other", Sex.Unknown)]
    [InlineData("unknown", Sex.Unknown)]
    [InlineData(null, Sex.Unknown)]
    public void MapFhirGenderToSex_MapsCorrectly(string? gender, Sex expected)
    {
        FhirR4BundleParser.MapFhirGenderToSex(gender).Should().Be(expected);
    }

    [Fact]
    public void Parse_RoundTrip_ExportThenImport_PreservesPatientData()
    {
        // Export a patient to FHIR, then parse the bundle back
        var clinicId = Guid.NewGuid();
        var patient = CreateTestPatient(clinicId);
        var json = FhirR4Mapper.BuildBundle(patient, null, [], []);

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patient.Name.Should().Be("Buddy");
        result.Value.Patient.Species.Should().Be(Species.Dog);
        result.Value.Patient.Breed.Should().Be("Golden Retriever");
        result.Value.Patient.Sex.Should().Be(Sex.Male);
        result.Value.Patient.BirthDate.Should().Be(new DateOnly(2020, 5, 10));
        result.Value.Patient.MicrochipNumber.Should().Be("123456789012345");
    }

    [Fact]
    public void Parse_RoundTrip_WithMedicalRecords_PreservesData()
    {
        var clinicId = Guid.NewGuid();
        var patient = CreateTestPatient(clinicId);
        var record = CreateTestMedicalRecord(clinicId, patient.Id);
        var weight = CreateTestWeightEntry(clinicId, patient.Id);

        var json = FhirR4Mapper.BuildBundle(patient, null, [record], [weight]);

        var result = FhirR4BundleParser.Parse(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Encounters.Should().HaveCount(1);
        result.Value.Encounters[0].Diagnosis.Should().Be("Annual checkup");
        result.Value.Encounters[0].VetName.Should().Be("Dr. Al-Rashidi");
        result.Value.WeightObservations.Should().HaveCount(1);
        result.Value.WeightObservations[0].WeightKg.Should().Be(25.5m);
    }

    [Fact]
    public void ParseWeightObservation_NonBodyWeight_ReturnsError()
    {
        var json = """
        {
            "resourceType": "Observation",
            "code": {
                "coding": [{"system": "http://loinc.org", "code": "8310-5"}]
            },
            "valueQuantity": {"value": 37.5, "unit": "Cel", "code": "Cel"}
        }
        """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = FhirR4BundleParser.ParseWeightObservation(element);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ParseEncounterResource_MinimalEncounter_UsesDefaults()
    {
        var json = """
        {
            "resourceType": "Encounter",
            "id": "enc-1",
            "status": "finished"
        }
        """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = FhirR4BundleParser.ParseEncounterResource(element);

        result.IsSuccess.Should().BeTrue();
        result.Value.Diagnosis.Should().Be("Imported encounter");
        result.Value.VetName.Should().Be("Imported vet");
        result.Value.Treatment.Should().Be("Imported treatment");
    }

    [Fact]
    public void ParsePatientResource_SpeciesFromSnomedCode_ParsesCorrectly()
    {
        var json = """
        {
            "resourceType": "Patient",
            "name": [{"text": "Max"}],
            "gender": "male",
            "birthDate": "2019-03-01",
            "extension": [{
                "url": "http://hl7.org/fhir/StructureDefinition/patient-animal",
                "extension": [
                    {
                        "url": "species",
                        "valueCodeableConcept": {
                            "coding": [{"system": "http://snomed.info/sct", "code": "388445009", "display": "Horse"}],
                            "text": "Horse"
                        }
                    },
                    {"url": "breed", "valueCodeableConcept": {"text": "Arabian"}}
                ]
            }]
        }
        """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = FhirR4BundleParser.ParsePatientResource(element);

        result.IsSuccess.Should().BeTrue();
        result.Value.Species.Should().Be(Species.Horse);
        result.Value.Breed.Should().Be("Arabian");
    }

    // --- Helpers ---

    private static string BuildMinimalPatientBundle()
    {
        return """
        {
            "resourceType": "Bundle",
            "type": "collection",
            "entry": [
                {
                    "resource": {
                        "resourceType": "Patient",
                        "id": "pat-1",
                        "name": [{"text": "Buddy", "given": ["Buddy"]}],
                        "gender": "male",
                        "birthDate": "2020-05-10",
                        "extension": [{
                            "url": "http://hl7.org/fhir/StructureDefinition/patient-animal",
                            "extension": [
                                {
                                    "url": "species",
                                    "valueCodeableConcept": {
                                        "coding": [{"system": "http://snomed.info/sct", "code": "448771007", "display": "Dog"}],
                                        "text": "Dog"
                                    }
                                },
                                {"url": "breed", "valueCodeableConcept": {"text": "Golden Retriever"}}
                            ]
                        }],
                        "identifier": [{"system": "urn:iso:std:iso:11784", "value": "123456789012345"}]
                    }
                }
            ]
        }
        """;
    }

    private static string BuildFullBundle()
    {
        return """
        {
            "resourceType": "Bundle",
            "id": "test-bundle",
            "type": "collection",
            "timestamp": "2026-01-01T00:00:00Z",
            "entry": [
                {
                    "fullUrl": "urn:uuid:pat-1",
                    "resource": {
                        "resourceType": "Patient",
                        "id": "pat-1",
                        "name": [{"text": "Buddy", "given": ["Buddy"]}],
                        "gender": "male",
                        "birthDate": "2020-05-10",
                        "extension": [{
                            "url": "http://hl7.org/fhir/StructureDefinition/patient-animal",
                            "extension": [
                                {
                                    "url": "species",
                                    "valueCodeableConcept": {
                                        "coding": [{"system": "http://snomed.info/sct", "code": "448771007", "display": "Dog"}],
                                        "text": "Dog"
                                    }
                                },
                                {"url": "breed", "valueCodeableConcept": {"text": "Golden Retriever"}}
                            ]
                        }],
                        "identifier": [{"system": "urn:iso:std:iso:11784", "value": "123456789012345"}]
                    }
                },
                {
                    "fullUrl": "urn:uuid:enc-1",
                    "resource": {
                        "resourceType": "Encounter",
                        "id": "enc-1",
                        "status": "finished",
                        "class": {"system": "http://terminology.hl7.org/CodeSystem/v3-ActCode", "code": "AMB"},
                        "subject": {"reference": "Patient/pat-1"},
                        "participant": [{"individual": {"display": "Dr. Al-Rashidi"}}],
                        "period": {"start": "2025-12-01T10:00:00Z"},
                        "reasonCode": [{"text": "Annual checkup"}],
                        "text": {"status": "generated", "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Diagnosis: Annual checkup</p><p>Treatment: Vaccination administered</p></div>"}
                    }
                },
                {
                    "fullUrl": "urn:uuid:med-1",
                    "resource": {
                        "resourceType": "MedicationRequest",
                        "id": "med-1",
                        "status": "active",
                        "intent": "order",
                        "medicationCodeableConcept": {"text": "Amoxicillin"},
                        "subject": {"reference": "Patient/pat-1"},
                        "encounter": {"reference": "Encounter/enc-1"},
                        "dosageInstruction": [{"text": "250mg twice daily"}],
                        "requester": {"display": "LIC-12345"}
                    }
                },
                {
                    "fullUrl": "urn:uuid:obs-1",
                    "resource": {
                        "resourceType": "Observation",
                        "id": "obs-1",
                        "status": "final",
                        "code": {
                            "coding": [{"system": "http://loinc.org", "code": "29463-7", "display": "Body weight"}]
                        },
                        "subject": {"reference": "Patient/pat-1"},
                        "effectiveDateTime": "2025-12-01T10:00:00Z",
                        "valueQuantity": {"value": 25.5, "unit": "kg", "system": "http://unitsofmeasure.org", "code": "kg"},
                        "note": [{"text": "Post-surgery weight"}]
                    }
                }
            ]
        }
        """;
    }

    private static Patient CreateTestPatient(Guid clinicId)
    {
        return Patient.Create(
            clinicId, "Buddy", Species.Dog, "Golden Retriever",
            new DateOnly(2020, 5, 10), Sex.Male, "123456789012345").Value;
    }

    private static MedicalRecord CreateTestMedicalRecord(Guid clinicId, Guid patientId)
    {
        return MedicalRecord.Create(
            clinicId, patientId, "Annual checkup", "Vaccination administered",
            "Dr. Al-Rashidi", DateTime.UtcNow.AddDays(-5)).Value;
    }

    private static WeightEntry CreateTestWeightEntry(Guid clinicId, Guid patientId)
    {
        return WeightEntry.Create(
            clinicId, patientId, 25.5m, "Dr. Al-Rashidi", "Post-surgery weight").Value;
    }
}
