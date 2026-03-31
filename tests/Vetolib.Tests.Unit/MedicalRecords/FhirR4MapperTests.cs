using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class FhirR4MapperTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void MapPatient_SetsResourceTypeAndBasicFields()
    {
        var patient = CreatePatient();

        var result = FhirR4Mapper.MapPatient(patient);

        result["resourceType"]!.GetValue<string>().Should().Be("Patient");
        result["id"]!.GetValue<string>().Should().Be(patient.Id.ToString());
        result["gender"]!.GetValue<string>().Should().Be("male");
        result["birthDate"]!.GetValue<string>().Should().Be("2020-05-10");
    }

    [Fact]
    public void MapPatient_SetsAnimalName()
    {
        var patient = CreatePatient();

        var result = FhirR4Mapper.MapPatient(patient);

        var name = result["name"]!.AsArray()[0]!;
        name["text"]!.GetValue<string>().Should().Be("Buddy");
        name["given"]!.AsArray()[0]!.GetValue<string>().Should().Be("Buddy");
    }

    [Fact]
    public void MapPatient_IncludesPatientAnimalExtension()
    {
        var patient = CreatePatient();

        var result = FhirR4Mapper.MapPatient(patient);

        var extensions = result["extension"]!.AsArray();
        extensions.Should().HaveCount(1);

        var animalExt = extensions[0]!;
        animalExt["url"]!.GetValue<string>().Should().Be("http://hl7.org/fhir/StructureDefinition/patient-animal");

        var subExtensions = animalExt["extension"]!.AsArray();
        subExtensions.Should().HaveCount(2);

        // Species
        var speciesExt = subExtensions[0]!;
        speciesExt["url"]!.GetValue<string>().Should().Be("species");
        speciesExt["valueCodeableConcept"]!["text"]!.GetValue<string>().Should().Be("Dog");
        speciesExt["valueCodeableConcept"]!["coding"]!.AsArray()[0]!["system"]!.GetValue<string>()
            .Should().Be("http://snomed.info/sct");

        // Breed
        var breedExt = subExtensions[1]!;
        breedExt["url"]!.GetValue<string>().Should().Be("breed");
        breedExt["valueCodeableConcept"]!["text"]!.GetValue<string>().Should().Be("Golden Retriever");
    }

    [Fact]
    public void MapPatient_WithMicrochip_IncludesIdentifier()
    {
        var patient = CreatePatient(microchip: "123456789012345");

        var result = FhirR4Mapper.MapPatient(patient);

        var identifiers = result["identifier"]!.AsArray();
        identifiers.Should().HaveCount(1);
        identifiers[0]!["system"]!.GetValue<string>().Should().Be("urn:iso:std:iso:11784");
        identifiers[0]!["value"]!.GetValue<string>().Should().Be("123456789012345");
    }

    [Fact]
    public void MapPatient_WithoutMicrochip_NoIdentifier()
    {
        var patient = CreatePatient();

        var result = FhirR4Mapper.MapPatient(patient);

        result["identifier"].Should().BeNull();
    }

    [Theory]
    [InlineData(Sex.Male, "male")]
    [InlineData(Sex.Female, "female")]
    [InlineData(Sex.NeuteredMale, "male")]
    [InlineData(Sex.SpayedFemale, "female")]
    [InlineData(Sex.Unknown, "unknown")]
    public void MapSexToFhirGender_MapsCorrectly(Sex sex, string expectedGender)
    {
        FhirR4Mapper.MapSexToFhirGender(sex).Should().Be(expectedGender);
    }

    [Theory]
    [InlineData(Species.Dog, "448771007")]
    [InlineData(Species.Cat, "448169003")]
    [InlineData(Species.Horse, "388445009")]
    [InlineData(Species.Camel, "422856003")]
    [InlineData(Species.Falcon, "8960001")]
    [InlineData(Species.Exotic, "387961004")]
    public void MapSpeciesToSnomedCode_MapsCorrectly(Species species, string expectedCode)
    {
        FhirR4Mapper.MapSpeciesToSnomedCode(species).Should().Be(expectedCode);
    }

    [Fact]
    public void MapOwnerToRelatedPerson_SetsResourceTypeAndPatientReference()
    {
        var owner = CreateOwner();
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapOwnerToRelatedPerson(owner, patientId);

        result["resourceType"]!.GetValue<string>().Should().Be("RelatedPerson");
        result["id"]!.GetValue<string>().Should().Be(owner.Id.ToString());
        result["patient"]!["reference"]!.GetValue<string>().Should().Be($"Patient/{patientId}");
    }

    [Fact]
    public void MapOwnerToRelatedPerson_SetsNameAndRelationship()
    {
        var owner = CreateOwner();
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapOwnerToRelatedPerson(owner, patientId);

        var name = result["name"]!.AsArray()[0]!;
        name["family"]!.GetValue<string>().Should().Be("Al-Rashid");
        name["given"]!.AsArray()[0]!.GetValue<string>().Should().Be("Ahmed");

        var relationship = result["relationship"]!.AsArray()[0]!;
        relationship["coding"]!.AsArray()[0]!["code"]!.GetValue<string>().Should().Be("O");
    }

    [Fact]
    public void MapOwnerToRelatedPerson_IncludesTelecom()
    {
        var owner = CreateOwner();
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapOwnerToRelatedPerson(owner, patientId);

        var telecom = result["telecom"]!.AsArray();
        telecom.Should().HaveCount(2);
        telecom[0]!["system"]!.GetValue<string>().Should().Be("email");
        telecom[0]!["value"]!.GetValue<string>().Should().Be("ahmed@email.ae");
        telecom[1]!["system"]!.GetValue<string>().Should().Be("phone");
        telecom[1]!["value"]!.GetValue<string>().Should().Be("+971 50 123 4567");
    }

    [Fact]
    public void MapMedicalRecordToEncounter_SetsCorrectFields()
    {
        var record = CreateMedicalRecord();
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapMedicalRecordToEncounter(record, patientId);

        result["resourceType"]!.GetValue<string>().Should().Be("Encounter");
        result["id"]!.GetValue<string>().Should().Be(record.Id.ToString());
        result["status"]!.GetValue<string>().Should().Be("finished");
        result["subject"]!["reference"]!.GetValue<string>().Should().Be($"Patient/{patientId}");

        // Participant (vet)
        result["participant"]!.AsArray()[0]!["individual"]!["display"]!.GetValue<string>()
            .Should().Be("Dr. Al-Rashidi");

        // Reason (diagnosis)
        result["reasonCode"]!.AsArray()[0]!["text"]!.GetValue<string>()
            .Should().Be("Annual checkup");

        // Narrative text includes both diagnosis and treatment
        result["text"]!["div"]!.GetValue<string>().Should().Contain("Annual checkup");
        result["text"]!["div"]!.GetValue<string>().Should().Contain("Vaccination administered");
    }

    [Fact]
    public void MapPrescriptionToMedicationRequest_SetsCorrectFields()
    {
        var prescription = CreatePrescription();
        var patientId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();

        var result = FhirR4Mapper.MapPrescriptionToMedicationRequest(prescription, patientId, encounterId);

        result["resourceType"]!.GetValue<string>().Should().Be("MedicationRequest");
        result["id"]!.GetValue<string>().Should().Be(prescription.Id.ToString());
        result["status"]!.GetValue<string>().Should().Be("active");
        result["intent"]!.GetValue<string>().Should().Be("order");
        result["medicationCodeableConcept"]!["text"]!.GetValue<string>().Should().Be("Amoxicillin");
        result["subject"]!["reference"]!.GetValue<string>().Should().Be($"Patient/{patientId}");
        result["encounter"]!["reference"]!.GetValue<string>().Should().Be($"Encounter/{encounterId}");
        result["dosageInstruction"]!.AsArray()[0]!["text"]!.GetValue<string>().Should().Be("250mg twice daily");
        result["requester"]!["display"]!.GetValue<string>().Should().Be("LIC-12345");
    }

    [Fact]
    public void MapWeightEntryToObservation_SetsCorrectFields()
    {
        var weight = CreateWeightEntry(25.5m);
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapWeightEntryToObservation(weight, patientId);

        result["resourceType"]!.GetValue<string>().Should().Be("Observation");
        result["id"]!.GetValue<string>().Should().Be(weight.Id.ToString());
        result["status"]!.GetValue<string>().Should().Be("final");
        result["subject"]!["reference"]!.GetValue<string>().Should().Be($"Patient/{patientId}");

        // LOINC code for body weight
        var coding = result["code"]!["coding"]!.AsArray()[0]!;
        coding["system"]!.GetValue<string>().Should().Be("http://loinc.org");
        coding["code"]!.GetValue<string>().Should().Be("29463-7");

        // Value quantity
        var quantity = result["valueQuantity"]!;
        quantity["value"]!.GetValue<decimal>().Should().Be(25.5m);
        quantity["unit"]!.GetValue<string>().Should().Be("kg");
        quantity["system"]!.GetValue<string>().Should().Be("http://unitsofmeasure.org");
    }

    [Fact]
    public void MapWeightEntryToObservation_WithNote_IncludesNote()
    {
        var weight = CreateWeightEntry(30.0m, "Post-surgery weight");
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapWeightEntryToObservation(weight, patientId);

        result["note"]!.AsArray()[0]!["text"]!.GetValue<string>().Should().Be("Post-surgery weight");
    }

    [Fact]
    public void MapWeightEntryToObservation_WithoutNote_NoNoteField()
    {
        var weight = CreateWeightEntry(30.0m);
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapWeightEntryToObservation(weight, patientId);

        result["note"].Should().BeNull();
    }

    [Fact]
    public void BuildBundle_ProducesValidFhirBundle()
    {
        var patient = CreatePatient(microchip: "123456789012345");
        var owner = CreateOwner();
        var records = new List<MedicalRecord> { CreateMedicalRecord() };
        var weights = new List<WeightEntry> { CreateWeightEntry(25.5m) };

        var json = FhirR4Mapper.BuildBundle(patient, owner, records, weights);

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("resourceType").GetString().Should().Be("Bundle");
        root.GetProperty("type").GetString().Should().Be("collection");
        root.TryGetProperty("timestamp", out _).Should().BeTrue();

        var entries = root.GetProperty("entry");
        // Patient + RelatedPerson + Encounter + Observation = 4 (no prescriptions on the record)
        entries.GetArrayLength().Should().Be(4);

        // First entry is the Patient
        entries[0].GetProperty("resource").GetProperty("resourceType").GetString().Should().Be("Patient");
        entries[0].GetProperty("fullUrl").GetString().Should().Contain(patient.Id.ToString());
    }

    [Fact]
    public void BuildBundle_WithoutOwner_SkipsRelatedPerson()
    {
        var patient = CreatePatient();
        var json = FhirR4Mapper.BuildBundle(patient, null, [], []);

        var doc = JsonDocument.Parse(json);
        var entries = doc.RootElement.GetProperty("entry");

        // Only Patient
        entries.GetArrayLength().Should().Be(1);
        entries[0].GetProperty("resource").GetProperty("resourceType").GetString().Should().Be("Patient");
    }

    [Fact]
    public void BuildBundle_IsValidJson()
    {
        var patient = CreatePatient(microchip: "123456789012345");
        var owner = CreateOwner();
        var records = new List<MedicalRecord> { CreateMedicalRecord() };
        var weights = new List<WeightEntry> { CreateWeightEntry(10.0m), CreateWeightEntry(12.0m) };

        var json = FhirR4Mapper.BuildBundle(patient, owner, records, weights);

        // Should not throw
        var parsed = JsonDocument.Parse(json);
        parsed.Should().NotBeNull();
    }

    [Fact]
    public void MapMedicalRecordToEncounter_EscapesHtmlInNarrative()
    {
        var record = CreateMedicalRecord(diagnosis: "Test <script>alert('xss')</script>");
        var patientId = Guid.NewGuid();

        var result = FhirR4Mapper.MapMedicalRecordToEncounter(record, patientId);

        var div = result["text"]!["div"]!.GetValue<string>();
        div.Should().NotContain("<script>");
        div.Should().Contain("&lt;script&gt;");
    }

    // --- Helpers ---

    private static Patient CreatePatient(string? microchip = null)
    {
        var result = Patient.Create(ClinicId, "Buddy", Species.Dog, "Golden Retriever",
            new DateOnly(2020, 5, 10), Sex.Male, microchip);
        return result.Value;
    }

    private static Owner CreateOwner()
    {
        var result = Owner.Create(ClinicId, "Ahmed", "Al-Rashid", "ahmed@email.ae", "+971 50 123 4567");
        return result.Value;
    }

    private static MedicalRecord CreateMedicalRecord(string diagnosis = "Annual checkup", string treatment = "Vaccination administered")
    {
        var result = MedicalRecord.Create(ClinicId, Guid.NewGuid(), diagnosis, treatment,
            "Dr. Al-Rashidi", DateTime.UtcNow.AddDays(-5));
        return result.Value;
    }

    private static Prescription CreatePrescription()
    {
        var result = Prescription.Create(ClinicId, Guid.NewGuid(), "Amoxicillin", "250mg twice daily", "LIC-12345");
        return result.Value;
    }

    private static WeightEntry CreateWeightEntry(decimal weightKg, string? note = null)
    {
        var result = WeightEntry.Create(ClinicId, Guid.NewGuid(), weightKg, "Dr. Al-Rashidi", note);
        return result.Value;
    }
}
