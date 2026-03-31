using FluentAssertions;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Application.Rules;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class HealthAlertRulesTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("22222222-2222-2222-2222-222222222222");

    private static WeightEntryDto W(decimal weightKg, DateTime recordedAt)
        => new(Guid.NewGuid(), PatientId, weightKg, recordedAt, "Dr. Test", null);

    private static PatientAlertContext CreatePatient(
        Species species = Species.Dog,
        string breed = "Labrador Retriever",
        int ageYears = 5,
        decimal? weightKg = 30m,
        IReadOnlyList<MedicalRecordSummaryDto>? records = null,
        IReadOnlyList<WeightEntryDto>? weightHistory = null)
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-ageYears));
        return new PatientAlertContext(
            ClinicId, PatientId, "TestPet", species, breed, birthDate,
            weightKg,
            records ?? Array.Empty<MedicalRecordSummaryDto>(),
            weightHistory ?? Array.Empty<WeightEntryDto>());
    }

    private static MedicalRecordSummaryDto Record(string diagnosis)
        => new(Guid.NewGuid(), diagnosis, "Dr. Test", DateTime.UtcNow.AddDays(-30));

    private static MedicalRecordSummaryDto Record(string diagnosis, DateTime examinedAt)
        => new(Guid.NewGuid(), diagnosis, "Dr. Test", examinedAt);

    private static List<HealthAlert> NoExistingAlerts => [];

    private static List<HealthAlert> ExistingAlertForRule(string ruleId)
    {
        var alert = HealthAlert.Create(
            ClinicId, PatientId, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium, "Existing", "Existing alert", null, ruleId, 50);
        return [alert.Value];
    }

    // ── CatRenalScreeningRule ──────────────────────────────────────────────────

    [Fact]
    public void CatRenal_OldCat_NoRecords_GeneratesAlert()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Persian", 8);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("CAT_RENAL_SCREENING");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High); // Persian is high risk
    }

    [Fact]
    public void CatRenal_YoungCat_NoAlert()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Domestic Shorthair", 4);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CatRenal_HighRiskBreed_AlertsAtAge5()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Persian", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void CatRenal_NonHighRiskBreed_NoAlertAtAge5()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Domestic Shorthair", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CatRenal_Dog_NoAlert()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 10);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CatRenal_RecentRenalRecord_NoAlert()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Persian", 8, records: [Record("renal panel normal")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CatRenal_DuplicateExisting_NoAlert()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Persian", 8);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("CAT_RENAL_SCREENING"));

        alerts.Should().BeEmpty();
    }

    // ── CardiacBreedRule ──────────────────────────────────────────────────────

    [Fact]
    public void Cardiac_DobermanAge5_GeneratesAlert()
    {
        var rule = new CardiacBreedRule();
        var patient = CreatePatient(Species.Dog, "Doberman", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("CARDIAC_BREED_SCREENING");
    }

    [Fact]
    public void Cardiac_MaineCoonAge4_GeneratesAlert()
    {
        var rule = new CardiacBreedRule();
        var patient = CreatePatient(Species.Cat, "Maine Coon", 4);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void Cardiac_DobermanAge1_TooYoung()
    {
        var rule = new CardiacBreedRule();
        var patient = CreatePatient(Species.Dog, "Doberman", 1);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Cardiac_NonRiskBreed_NoAlert()
    {
        var rule = new CardiacBreedRule();
        var patient = CreatePatient(Species.Dog, "Beagle", 8);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Cardiac_RecentCardiacRecord_NoAlert()
    {
        var rule = new CardiacBreedRule();
        var patient = CreatePatient(Species.Dog, "Doberman", 5, records: [Record("cardiac exam normal")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── SeniorWellnessRule ──────────────────────────────────────────────────────

    [Fact]
    public void Senior_DogAge8_GeneratesAlert()
    {
        var rule = new SeniorWellnessRule();
        var patient = CreatePatient(Species.Dog, "Mixed", 8);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("SENIOR_WELLNESS");
    }

    [Fact]
    public void Senior_CatAge10_GeneratesAlert()
    {
        var rule = new SeniorWellnessRule();
        var patient = CreatePatient(Species.Cat, "Domestic", 10);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void Senior_DogAge5_TooYoung()
    {
        var rule = new SeniorWellnessRule();
        var patient = CreatePatient(Species.Dog, "Mixed", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Senior_DogAge12_HighSeverity()
    {
        var rule = new SeniorWellnessRule();
        var patient = CreatePatient(Species.Dog, "Mixed", 12);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void Senior_RecentWellnessExam_NoAlert()
    {
        var rule = new SeniorWellnessRule();
        var patient = CreatePatient(Species.Dog, "Mixed", 8, records: [Record("senior wellness exam")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── WeightTrendRule ──────────────────────────────────────────────────────

    [Fact]
    public void Weight_SignificantLoss_GeneratesAlert()
    {
        var rule = new WeightTrendRule();
        var history = new List<WeightEntryDto>
        {
            W(25m, DateTime.UtcNow),
            W(30m, DateTime.UtcNow.AddMonths(-4))
        };
        var patient = CreatePatient(weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("WEIGHT_TREND");
        alerts[0].Title.Should().Contain("loss");
    }

    [Fact]
    public void Weight_SignificantGain_GeneratesAlert()
    {
        var rule = new WeightTrendRule();
        var history = new List<WeightEntryDto>
        {
            W(35m, DateTime.UtcNow),
            W(30m, DateTime.UtcNow.AddMonths(-4))
        };
        var patient = CreatePatient(weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Title.Should().Contain("gain");
    }

    [Fact]
    public void Weight_SmallChange_NoAlert()
    {
        var rule = new WeightTrendRule();
        var history = new List<WeightEntryDto>
        {
            W(30.5m, DateTime.UtcNow),
            W(30m, DateTime.UtcNow.AddMonths(-4))
        };
        var patient = CreatePatient(weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Weight_OnlyOneEntry_NoAlert()
    {
        var rule = new WeightTrendRule();
        var history = new List<WeightEntryDto> { W(30m, DateTime.UtcNow) };
        var patient = CreatePatient(weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Weight_NoOldEntry_NoAlert()
    {
        var rule = new WeightTrendRule();
        var history = new List<WeightEntryDto>
        {
            W(25m, DateTime.UtcNow),
            W(30m, DateTime.UtcNow.AddDays(-10)) // Too recent
        };
        var patient = CreatePatient(weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Weight_Over20Percent_HighSeverity()
    {
        var rule = new WeightTrendRule();
        var history = new List<WeightEntryDto>
        {
            W(20m, DateTime.UtcNow),
            W(30m, DateTime.UtcNow.AddMonths(-4))
        };
        var patient = CreatePatient(weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    // ── VaccinationOverdueRule ──────────────────────────────────────────────────

    [Fact]
    public void Vaccination_DogNoRecords_GeneratesAlert()
    {
        var rule = new VaccinationOverdueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("VACCINATION_OVERDUE");
    }

    [Fact]
    public void Vaccination_CatNoRecords_GeneratesAlert()
    {
        var rule = new VaccinationOverdueRule();
        var patient = CreatePatient(Species.Cat, "Domestic", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void Vaccination_Puppy_TooYoung()
    {
        var rule = new VaccinationOverdueRule();
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var patient = new PatientAlertContext(
            ClinicId, PatientId, "Puppy", Species.Dog, "Lab", birthDate,
            5m, [], []);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Vaccination_Horse_DoesNotApply()
    {
        var rule = new VaccinationOverdueRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Vaccination_RecentVaccine_NoAlert()
    {
        var rule = new VaccinationOverdueRule();
        var patient = CreatePatient(Species.Dog, "Lab", 3, records: [Record("annual vaccination DHPP booster")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── BrachycephalicAirwayRule ──────────────────────────────────────────────

    [Fact]
    public void Brachycephalic_FrenchBulldog_GeneratesAlert()
    {
        var rule = new BrachycephalicAirwayRule();
        var patient = CreatePatient(Species.Dog, "French Bulldog", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("BRACHYCEPHALIC_AIRWAY");
    }

    [Fact]
    public void Brachycephalic_Persian_Cat_GeneratesAlert()
    {
        var rule = new BrachycephalicAirwayRule();
        var patient = CreatePatient(Species.Cat, "Persian", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void Brachycephalic_NonBrachyBreed_NoAlert()
    {
        var rule = new BrachycephalicAirwayRule();
        var patient = CreatePatient(Species.Dog, "German Shepherd", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Brachycephalic_Puppy_TooYoung()
    {
        var rule = new BrachycephalicAirwayRule();
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));
        var patient = new PatientAlertContext(
            ClinicId, PatientId, "Pup", Species.Dog, "French Bulldog", birthDate,
            5m, [], []);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Brachycephalic_RecentBOAS_NoAlert()
    {
        var rule = new BrachycephalicAirwayRule();
        var patient = CreatePatient(Species.Dog, "French Bulldog", 3, records: [Record("BOAS grade 1")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── HipDysplasiaRule ──────────────────────────────────────────────────────

    [Fact]
    public void HipDysplasia_GermanShepherd_GeneratesAlert()
    {
        var rule = new HipDysplasiaRule();
        var patient = CreatePatient(Species.Dog, "German Shepherd", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("HIP_DYSPLASIA_SCREENING");
    }

    [Fact]
    public void HipDysplasia_Cat_DoesNotApply()
    {
        var rule = new HipDysplasiaRule();
        var patient = CreatePatient(Species.Cat, "Maine Coon", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HipDysplasia_NonRiskBreed_NoAlert()
    {
        var rule = new HipDysplasiaRule();
        var patient = CreatePatient(Species.Dog, "Chihuahua", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HipDysplasia_Age5Plus_HighSeverity()
    {
        var rule = new HipDysplasiaRule();
        var patient = CreatePatient(Species.Dog, "German Shepherd", 6);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void HipDysplasia_RecentOFA_NoAlert()
    {
        var rule = new HipDysplasiaRule();
        var patient = CreatePatient(Species.Dog, "German Shepherd", 3, records: [Record("OFA hip evaluation good")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── DiabetesRiskRule ──────────────────────────────────────────────────────

    [Fact]
    public void Diabetes_Samoyed_GeneratesAlert()
    {
        var rule = new DiabetesRiskRule();
        var patient = CreatePatient(Species.Dog, "Samoyed", 6);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("DIABETES_RISK");
    }

    [Fact]
    public void Diabetes_BurmeseCat_GeneratesAlert()
    {
        var rule = new DiabetesRiskRule();
        var patient = CreatePatient(Species.Cat, "Burmese", 7);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void Diabetes_TooYoung_NoAlert()
    {
        var rule = new DiabetesRiskRule();
        var patient = CreatePatient(Species.Dog, "Samoyed", 3);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Diabetes_NonRiskBreed_NoAlert()
    {
        var rule = new DiabetesRiskRule();
        var patient = CreatePatient(Species.Dog, "Beagle", 8);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Diabetes_RecentGlucose_NoAlert()
    {
        var rule = new DiabetesRiskRule();
        var patient = CreatePatient(Species.Dog, "Samoyed", 7, records: [Record("glucose 95 mg/dL normal")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── DentalProphylaxisRule ──────────────────────────────────────────────────

    [Fact]
    public void Dental_DogAge3_GeneratesAlert()
    {
        var rule = new DentalProphylaxisRule();
        var patient = CreatePatient(Species.Dog, "Poodle", 3);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("DENTAL_PROPHYLAXIS");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Low);
    }

    [Fact]
    public void Dental_DogAge7_MediumSeverity()
    {
        var rule = new DentalProphylaxisRule();
        var patient = CreatePatient(Species.Dog, "Poodle", 7);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
    }

    [Fact]
    public void Dental_Puppy_NoAlert()
    {
        var rule = new DentalProphylaxisRule();
        var patient = CreatePatient(Species.Dog, "Poodle", 1);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Dental_Horse_DoesNotApply()
    {
        var rule = new DentalProphylaxisRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Dental_RecentDentalRecord_NoAlert()
    {
        var rule = new DentalProphylaxisRule();
        var patient = CreatePatient(Species.Dog, "Poodle", 5, records: [Record("dental prophylaxis performed")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── ArthritisFollowUpRule ──────────────────────────────────────────────────

    [Fact]
    public void Arthritis_WithDiagnosis_GeneratesAlert()
    {
        var rule = new ArthritisFollowUpRule();
        var patient = CreatePatient(Species.Dog, "Lab", 8, records: [Record("arthritis bilateral stifle")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("ARTHRITIS_FOLLOWUP");
        alerts[0].AlertType.Should().Be(HealthAlertType.ChronicDiseaseFollowUp);
    }

    [Fact]
    public void Arthritis_WithDJDDiagnosis_GeneratesAlert()
    {
        var rule = new ArthritisFollowUpRule();
        var patient = CreatePatient(Species.Dog, "Lab", 10, records: [Record("DJD lumbar")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High); // Age >= 10
    }

    [Fact]
    public void Arthritis_NoDiagnosis_NoAlert()
    {
        var rule = new ArthritisFollowUpRule();
        var patient = CreatePatient(Species.Dog, "Lab", 8, records: [Record("routine checkup healthy")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void Arthritis_Dedup_NoAlert()
    {
        var rule = new ArthritisFollowUpRule();
        var patient = CreatePatient(Species.Dog, "Lab", 8, records: [Record("arthritis bilateral")]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("ARTHRITIS_FOLLOWUP"));

        alerts.Should().BeEmpty();
    }

    // ── Dedup across all rules (generic) ──────────────────────────────────────

    [Fact]
    public void DismissedAlert_DoesNotBlockNewAlert()
    {
        var rule = new CatRenalScreeningRule();
        var patient = CreatePatient(Species.Cat, "Persian", 8);

        var existingAlert = HealthAlert.Create(
            ClinicId, PatientId, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium, "Old", "Old alert", null, "CAT_RENAL_SCREENING", 50).Value;
        existingAlert.Dismiss("no longer relevant", "Dr. Test");

        var alerts = rule.Evaluate(patient, [existingAlert]);

        alerts.Should().HaveCount(1); // Dismissed alert does NOT block new one
    }

    // ── FalconMoltWeightLossRule ──────────────────────────────────────────────

    [Fact]
    public void FalconMoltWeight_NonFalcon_NoAlert()
    {
        var rule = new FalconMoltWeightLossRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconMoltWeight_FalconDuringMolt_WithWeightLoss_GeneratesAlert()
    {
        var rule = new FalconMoltWeightLossRule();
        var now = DateTime.UtcNow;

        // Only triggers during Aug-Oct
        if (now.Month < 8 || now.Month > 10)
        {
            // Outside molt season, should not fire
            var history = new List<WeightEntryDto>
            {
                W(0.8m, now),
                W(1.0m, now.AddMonths(-3))
            };
            var patient = CreatePatient(Species.Falcon, "Peregrine", 3, 0.8m, weightHistory: history);
            var alerts = rule.Evaluate(patient, NoExistingAlerts);
            alerts.Should().BeEmpty();
        }
        else
        {
            // During molt season with >10% loss
            var preMolt = new DateTime(now.Year, 7, 15); // July baseline
            var history = new List<WeightEntryDto>
            {
                W(0.8m, now),
                W(1.0m, preMolt)
            };
            var patient = CreatePatient(Species.Falcon, "Peregrine", 3, 0.8m, weightHistory: history);
            var alerts = rule.Evaluate(patient, NoExistingAlerts);
            alerts.Should().HaveCount(1);
            alerts[0].RuleId.Should().Be("FALCON_MOLT_WEIGHT_LOSS");
        }
    }

    [Fact]
    public void FalconMoltWeight_SmallLoss_NoAlert()
    {
        var rule = new FalconMoltWeightLossRule();
        var now = DateTime.UtcNow;

        // Even during molt, <10% loss should not trigger
        var history = new List<WeightEntryDto>
        {
            W(0.95m, now),
            W(1.0m, now.AddMonths(-3))
        };
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3, 0.95m, weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty(); // 5% loss < 10% threshold (or outside season)
    }

    [Fact]
    public void FalconMoltWeight_NoWeightHistory_NoAlert()
    {
        var rule = new FalconMoltWeightLossRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3, 1.0m);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconMoltWeight_Dedup_NoAlert()
    {
        var rule = new FalconMoltWeightLossRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3, 0.8m);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("FALCON_MOLT_WEIGHT_LOSS"));

        alerts.Should().BeEmpty();
    }

    // ── FalconAspergillosisRiskRule ──────────────────────────────────────────

    [Fact]
    public void FalconAspergillosis_NonFalcon_NoAlert()
    {
        var rule = new FalconAspergillosisRiskRule();
        var patient = CreatePatient(Species.Bird, "Parrot", 5,
            records: [Record("respiratory distress")],
            weightHistory: [W(0.3m, DateTime.UtcNow), W(0.4m, DateTime.UtcNow.AddMonths(-2))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconAspergillosis_WeightLossAndRespiratory_GeneratesAlert()
    {
        var rule = new FalconAspergillosisRiskRule();
        var history = new List<WeightEntryDto>
        {
            W(0.8m, DateTime.UtcNow),
            W(1.0m, DateTime.UtcNow.AddMonths(-1))
        };
        var patient = CreatePatient(Species.Falcon, "Saker", 4, 0.8m,
            records: [Record("respiratory distress, dyspnea observed")],
            weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("FALCON_ASPERGILLOSIS_RISK");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void FalconAspergillosis_WeightLossOnly_NoAlert()
    {
        var rule = new FalconAspergillosisRiskRule();
        var history = new List<WeightEntryDto>
        {
            W(0.8m, DateTime.UtcNow),
            W(1.0m, DateTime.UtcNow.AddMonths(-1))
        };
        var patient = CreatePatient(Species.Falcon, "Saker", 4, 0.8m,
            records: [Record("routine checkup")],
            weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty(); // No respiratory signs
    }

    [Fact]
    public void FalconAspergillosis_RespiratoryOnly_NoAlert()
    {
        var rule = new FalconAspergillosisRiskRule();
        var patient = CreatePatient(Species.Falcon, "Saker", 4, 1.0m,
            records: [Record("mild wheezing")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty(); // No weight loss (only 1 weight entry or no loss)
    }

    [Fact]
    public void FalconAspergillosis_AlreadyDiagnosed_NoAlert()
    {
        var rule = new FalconAspergillosisRiskRule();
        var history = new List<WeightEntryDto>
        {
            W(0.8m, DateTime.UtcNow),
            W(1.0m, DateTime.UtcNow.AddMonths(-1))
        };
        var patient = CreatePatient(Species.Falcon, "Saker", 4, 0.8m,
            records: [Record("aspergillosis under treatment, dyspnea")],
            weightHistory: history);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty(); // Already diagnosed
    }

    // ── FalconBumblefootRule ────────────────────────────────────────────────

    [Fact]
    public void FalconBumblefoot_NonFalcon_NoAlert()
    {
        var rule = new FalconBumblefootRule();
        var patient = CreatePatient(Species.Bird, "Eagle", 5,
            records: [Record("foot swelling observed")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconBumblefoot_WithFootSymptoms_GeneratesAlert()
    {
        var rule = new FalconBumblefootRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("foot swelling on left foot")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("FALCON_BUMBLEFOOT");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
    }

    [Fact]
    public void FalconBumblefoot_NoFootSymptoms_NoAlert()
    {
        var rule = new FalconBumblefootRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("routine checkup healthy")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconBumblefoot_Dedup_NoAlert()
    {
        var rule = new FalconBumblefootRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("bumblefoot grade II")]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("FALCON_BUMBLEFOOT"));

        alerts.Should().BeEmpty();
    }

    // ── FalconTrichomoniasisRule ────────────────────────────────────────────

    [Fact]
    public void FalconTrich_NonFalcon_NoAlert()
    {
        var rule = new FalconTrichomoniasisRule();
        var patient = CreatePatient(Species.Bird, "Pigeon", 2,
            records: [Record("crop lesion")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconTrich_WithCropSymptoms_GeneratesAlert()
    {
        var rule = new FalconTrichomoniasisRule();
        var patient = CreatePatient(Species.Falcon, "Saker", 2,
            records: [Record("crop lesion visible, regurgitation")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("FALCON_TRICHOMONIASIS");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void FalconTrich_NoSymptoms_NoAlert()
    {
        var rule = new FalconTrichomoniasisRule();
        var patient = CreatePatient(Species.Falcon, "Saker", 2,
            records: [Record("routine exam")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconTrich_AlreadyTreated_NoAlert()
    {
        var rule = new FalconTrichomoniasisRule();
        var patient = CreatePatient(Species.Falcon, "Saker", 2,
            records: [Record("trichomoniasis treated, crop lesion")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── FalconMoltAnomalyRule ──────────────────────────────────────────────

    [Fact]
    public void FalconMoltAnomaly_NonFalcon_NoAlert()
    {
        var rule = new FalconMoltAnomalyRule();
        var patient = CreatePatient(Species.Bird, "Parrot", 3,
            records: [Record("feather loss observed")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconMoltAnomaly_StressBarAnyTime_GeneratesAlert()
    {
        var rule = new FalconMoltAnomalyRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("stress bar visible on flight feathers")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("FALCON_MOLT_ANOMALY");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
    }

    [Fact]
    public void FalconMoltAnomaly_NormalMoltInSeason_NoAlert()
    {
        var rule = new FalconMoltAnomalyRule();
        var now = DateTime.UtcNow;

        // During Jun-Oct, a normal molt record (no anomaly keywords) should not alert
        if (now.Month >= 6 && now.Month <= 10)
        {
            var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
                records: [Record("normal molt in progress")]);
            var alerts = rule.Evaluate(patient, NoExistingAlerts);
            alerts.Should().BeEmpty();
        }
        else
        {
            // Outside season, molt-related record triggers alert
            var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
                records: [Record("molt observed, feather loss")]);
            var alerts = rule.Evaluate(patient, NoExistingAlerts);
            alerts.Should().HaveCount(1);
        }
    }

    [Fact]
    public void FalconMoltAnomaly_NoMoltRecords_NoAlert()
    {
        var rule = new FalconMoltAnomalyRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("routine checkup")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── FalconHealthCertificateRule ────────────────────────────────────────

    [Fact]
    public void FalconCert_NonFalcon_NoAlert()
    {
        var rule = new FalconHealthCertificateRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 3);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconCert_NoCertificate_GeneratesAlert()
    {
        var rule = new FalconHealthCertificateRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("FALCON_HEALTH_CERTIFICATE");
    }

    [Fact]
    public void FalconCert_WithCertificate_NoAlert()
    {
        var rule = new FalconHealthCertificateRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 2,
            records: [Record("annual health certificate issued")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconCert_TooYoung_NoAlert()
    {
        var rule = new FalconHealthCertificateRule();
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var patient = new PatientAlertContext(
            ClinicId, PatientId, "YoungFalcon", Species.Falcon, "Peregrine", birthDate,
            0.5m, [], []);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconCert_Dedup_NoAlert()
    {
        var rule = new FalconHealthCertificateRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 2);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("FALCON_HEALTH_CERTIFICATE"));

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconCert_HuntingSeason_HighSeverity()
    {
        var rule = new FalconHealthCertificateRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 2);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (alerts.Count > 0)
        {
            var now = DateTime.UtcNow;
            var isHuntingSeason = now.Month >= 10 || now.Month <= 3;
            var expectedSeverity = isHuntingSeason
                ? HealthAlertSeverity.High
                : HealthAlertSeverity.Medium;
            alerts[0].Severity.Should().Be(expectedSeverity);
        }
    }

    // ── FalconPostHuntRecoveryRule ────────────────────────────────────────

    [Fact]
    public void FalconPostHunt_NonFalcon_NoAlert()
    {
        var rule = new FalconPostHuntRecoveryRule();
        var patient = CreatePatient(Species.Bird, "Eagle", 3);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconPostHunt_DuringAprilMay_GeneratesAlert()
    {
        var rule = new FalconPostHuntRecoveryRule();
        var now = DateTime.UtcNow;
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (now.Month >= 4 && now.Month <= 5)
        {
            alerts.Should().HaveCount(1);
            alerts[0].RuleId.Should().Be("FALCON_POST_HUNT_RECOVERY");
            alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
        }
        else
        {
            alerts.Should().BeEmpty(); // Outside April-May window
        }
    }

    [Fact]
    public void FalconPostHunt_WithHuntingInjury_HighSeverity()
    {
        var rule = new FalconPostHuntRecoveryRule();
        var now = DateTime.UtcNow;
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("hunting injury to right wing")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (now.Month >= 4 && now.Month <= 5)
        {
            alerts.Should().HaveCount(1);
            alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
        }
        else
        {
            alerts.Should().BeEmpty();
        }
    }

    [Fact]
    public void FalconPostHunt_WithRecoveryExam_NoAlert()
    {
        var rule = new FalconPostHuntRecoveryRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("post-season exam completed, all clear")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void FalconPostHunt_Dedup_NoAlert()
    {
        var rule = new FalconPostHuntRecoveryRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("FALCON_POST_HUNT_RECOVERY"));

        alerts.Should().BeEmpty();
    }

    // ── All falcon rules skip non-falcon species ─────────────────────────────

    [Theory]
    [InlineData(nameof(FalconMoltWeightLossRule))]
    [InlineData(nameof(FalconAspergillosisRiskRule))]
    [InlineData(nameof(FalconBumblefootRule))]
    [InlineData(nameof(FalconTrichomoniasisRule))]
    [InlineData(nameof(FalconMoltAnomalyRule))]
    [InlineData(nameof(FalconHealthCertificateRule))]
    [InlineData(nameof(FalconPostHuntRecoveryRule))]
    public void AllFalconRules_DogPatient_NoAlert(string ruleTypeName)
    {
        var rule = CreateFalconRule(ruleTypeName);
        var patient = CreatePatient(Species.Dog, "Labrador", 5,
            records: [Record("foot swelling, crop lesion, stress bar, respiratory distress")],
            weightHistory: [W(25m, DateTime.UtcNow), W(30m, DateTime.UtcNow.AddMonths(-3))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    // ── VaccinationDueRule ──────────────────────────────────────────────────

    [Fact]
    public void VaccinationDue_LastVaccine12MonthsAgo_GeneratesAlert()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 3,
            records: [Record("rabies vaccination", DateTime.UtcNow.AddMonths(-12))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("VACCINATION_DUE");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
        alerts[0].AlertType.Should().Be(HealthAlertType.VaccinationDue);
    }

    [Fact]
    public void VaccinationDue_LastVaccine11MonthsAgo_GeneratesAlert()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 3,
            records: [Record("DHPP booster", DateTime.UtcNow.AddMonths(-11))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void VaccinationDue_LastVaccine5MonthsAgo_NoAlert()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 3,
            records: [Record("rabies vaccination", DateTime.UtcNow.AddMonths(-5))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void VaccinationDue_Cat_LastVaccine14MonthsAgo_GeneratesAlert()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Cat, "Domestic Shorthair", 4,
            records: [Record("FVRCP vaccination", DateTime.UtcNow.AddMonths(-14))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void VaccinationDue_NoVaccinationRecords_NoAlert()
    {
        // No vaccination records => handled by VaccinationOverdueRule, not this rule
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 3,
            records: [Record("dental cleaning")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void VaccinationDue_Falcon_DoesNotApply()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Falcon, "Peregrine", 3,
            records: [Record("vaccination", DateTime.UtcNow.AddMonths(-12))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void VaccinationDue_Horse_DoesNotApply()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5,
            records: [Record("vaccination", DateTime.UtcNow.AddMonths(-12))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void VaccinationDue_Puppy_TooYoung()
    {
        var rule = new VaccinationDueRule();
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var patient = new PatientAlertContext(
            ClinicId, PatientId, "Puppy", Species.Dog, "Lab", birthDate,
            5m, [Record("vaccination", DateTime.UtcNow.AddMonths(-12))], []);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void VaccinationDue_DuplicateAlert_NoAlert()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 3,
            records: [Record("rabies vaccination", DateTime.UtcNow.AddMonths(-12))]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("VACCINATION_DUE"));

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void VaccinationDue_MultipleVaccineRecords_UsesLatest()
    {
        var rule = new VaccinationDueRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5,
            records:
            [
                Record("rabies vaccination", DateTime.UtcNow.AddMonths(-18)),
                Record("DHPP booster", DateTime.UtcNow.AddMonths(-5)),
                Record("dental cleaning", DateTime.UtcNow.AddDays(-10))
            ]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        // Most recent vaccination is 5 months ago => no alert
        alerts.Should().BeEmpty();
    }

    // ── CamelTrypanosomaRule ────────────────────────────────────────────────

    [Fact]
    public void CamelTrypanosoma_NonCamel_NoAlert()
    {
        var rule = new CamelTrypanosomaRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelTrypanosoma_WeightLossAndAnemia_GeneratesAlert()
    {
        var rule = new CamelTrypanosomaRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 6,
            records: [Record("pale mucous membranes, lethargy observed")],
            weightHistory: [W(380m, DateTime.UtcNow), W(420m, DateTime.UtcNow.AddMonths(-3))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("CAMEL_TRYPANOSOMA");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void CamelTrypanosoma_WeightLossOnly_NoAlert()
    {
        var rule = new CamelTrypanosomaRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 6,
            records: [Record("routine checkup")],
            weightHistory: [W(380m, DateTime.UtcNow), W(420m, DateTime.UtcNow.AddMonths(-3))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelTrypanosoma_AnemiaOnly_NoAlert()
    {
        var rule = new CamelTrypanosomaRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 6,
            records: [Record("anemia signs noted")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelTrypanosoma_AlreadyDiagnosed_NoAlert()
    {
        var rule = new CamelTrypanosomaRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 6,
            records: [Record("trypanosoma evansi confirmed, surra treatment started")],
            weightHistory: [W(380m, DateTime.UtcNow), W(420m, DateTime.UtcNow.AddMonths(-3))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelTrypanosoma_Dedup_NoAlert()
    {
        var rule = new CamelTrypanosomaRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 6,
            records: [Record("anemia, pale mucous")],
            weightHistory: [W(380m, DateTime.UtcNow), W(420m, DateTime.UtcNow.AddMonths(-3))]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("CAMEL_TRYPANOSOMA"));

        alerts.Should().BeEmpty();
    }

    // ── CamelHeatStressRule ──────────────────────────────────────────────────

    [Fact]
    public void CamelHeatStress_NonCamel_NoAlert()
    {
        var rule = new CamelHeatStressRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelHeatStress_DuringSummer_NoShadeNotes_GeneratesAlert()
    {
        var rule = new CamelHeatStressRule();
        var now = DateTime.UtcNow;
        var patient = CreatePatient(Species.Camel, "Dromedary", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (now.Month >= 6 && now.Month <= 9)
        {
            alerts.Should().HaveCount(1);
            alerts[0].RuleId.Should().Be("CAMEL_HEAT_STRESS");
        }
        else
        {
            alerts.Should().BeEmpty(); // Outside summer window
        }
    }

    [Fact]
    public void CamelHeatStress_WithShadeNotes_NoAlert()
    {
        var rule = new CamelHeatStressRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 5,
            records: [Record("shade and cooling misting system in place")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelHeatStress_Dedup_NoAlert()
    {
        var rule = new CamelHeatStressRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 5);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("CAMEL_HEAT_STRESS"));

        alerts.Should().BeEmpty();
    }

    // ── CamelFootRotRule ─────────────────────────────────────────────────────

    [Fact]
    public void CamelFootRot_NonCamel_NoAlert()
    {
        var rule = new CamelFootRotRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelFootRot_WithFootSymptoms_GeneratesAlert()
    {
        var rule = new CamelFootRotRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 4,
            records: [Record("foot swelling observed on left front pad")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("CAMEL_FOOT_ROT");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
    }

    [Fact]
    public void CamelFootRot_NoFootSymptoms_NoAlert()
    {
        var rule = new CamelFootRotRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 4,
            records: [Record("routine checkup, all normal")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelFootRot_AlreadyTreated_NoAlert()
    {
        var rule = new CamelFootRotRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 4,
            records: [Record("foot rot treatment completed, foot swelling resolved")]);

        // "foot rot treatment" triggers the exclusion even though "foot swelling" matches
        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelFootRot_Dedup_NoAlert()
    {
        var rule = new CamelFootRotRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 4,
            records: [Record("lameness detected")]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("CAMEL_FOOT_ROT"));

        alerts.Should().BeEmpty();
    }

    // ── CamelMERSScreeningRule ──────────────────────────────────────────────

    [Fact]
    public void CamelMERS_NonCamel_NoAlert()
    {
        var rule = new CamelMERSScreeningRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelMERS_RespiratorySymptoms_GeneratesAlert()
    {
        var rule = new CamelMERSScreeningRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 5,
            records: [Record("nasal discharge and cough observed")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("CAMEL_MERS_SCREENING");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
        alerts[0].AlertType.Should().Be(HealthAlertType.CamelMERSScreening);
    }

    [Fact]
    public void CamelMERS_NoRespiratorySymptoms_NoAlert()
    {
        var rule = new CamelMERSScreeningRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 5,
            records: [Record("routine dental check")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelMERS_AlreadyCleared_NoAlert()
    {
        var rule = new CamelMERSScreeningRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 5,
            records: [Record("nasal discharge, MERS negative confirmed")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelMERS_Dedup_NoAlert()
    {
        var rule = new CamelMERSScreeningRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 5,
            records: [Record("cough and dyspnea")]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("CAMEL_MERS_SCREENING"));

        alerts.Should().BeEmpty();
    }

    // ── CamelRacingFitnessRule ──────────────────────────────────────────────

    [Fact]
    public void CamelRacingFitness_NonCamel_NoAlert()
    {
        var rule = new CamelRacingFitnessRule();
        var patient = CreatePatient(Species.Dog, "Labrador", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelRacingFitness_Underweight_GeneratesAlert()
    {
        var rule = new CamelRacingFitnessRule();
        var now = DateTime.UtcNow;
        var patient = CreatePatient(Species.Camel, "Dromedary", 4, weightKg: 300m);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (now.Month <= 4 || now.Month >= 10)
        {
            alerts.Should().HaveCount(1);
            alerts[0].RuleId.Should().Be("CAMEL_RACING_FITNESS");
        }
        else
        {
            alerts.Should().BeEmpty(); // Outside racing season
        }
    }

    [Fact]
    public void CamelRacingFitness_RecentIllness_HighSeverity()
    {
        var rule = new CamelRacingFitnessRule();
        var now = DateTime.UtcNow;
        var patient = CreatePatient(Species.Camel, "Dromedary", 4, weightKg: 450m,
            records: [Record("fever and diarrhea treated")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (now.Month <= 4 || now.Month >= 10)
        {
            alerts.Should().HaveCount(1);
            alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
        }
        else
        {
            alerts.Should().BeEmpty();
        }
    }

    [Fact]
    public void CamelRacingFitness_NoVaccination_GeneratesAlert()
    {
        var rule = new CamelRacingFitnessRule();
        var now = DateTime.UtcNow;
        var patient = CreatePatient(Species.Camel, "Dromedary", 4, weightKg: 450m,
            records: [Record("routine dental check")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        if (now.Month <= 4 || now.Month >= 10)
        {
            alerts.Should().HaveCount(1);
        }
        else
        {
            alerts.Should().BeEmpty();
        }
    }

    [Fact]
    public void CamelRacingFitness_AllGood_NoAlert()
    {
        var rule = new CamelRacingFitnessRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 4, weightKg: 450m,
            records: [Record("vaccination booster administered")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelRacingFitness_TooYoung_NoAlert()
    {
        var rule = new CamelRacingFitnessRule();
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-18));
        var patient = new PatientAlertContext(
            ClinicId, PatientId, "YoungCamel", Species.Camel, "Dromedary", birthDate,
            300m, [], []);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void CamelRacingFitness_Dedup_NoAlert()
    {
        var rule = new CamelRacingFitnessRule();
        var patient = CreatePatient(Species.Camel, "Dromedary", 4, weightKg: 300m);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("CAMEL_RACING_FITNESS"));

        alerts.Should().BeEmpty();
    }

    // ── All camel rules skip non-camel species ──────────────────────────────

    [Theory]
    [InlineData(nameof(CamelTrypanosomaRule))]
    [InlineData(nameof(CamelHeatStressRule))]
    [InlineData(nameof(CamelFootRotRule))]
    [InlineData(nameof(CamelMERSScreeningRule))]
    [InlineData(nameof(CamelRacingFitnessRule))]
    public void AllCamelRules_DogPatient_NoAlert(string ruleTypeName)
    {
        var rule = CreateCamelRule(ruleTypeName);
        var patient = CreatePatient(Species.Dog, "Labrador", 5,
            records: [Record("foot swelling, nasal discharge, anemia, fever, diarrhea")],
            weightHistory: [W(25m, DateTime.UtcNow), W(30m, DateTime.UtcNow.AddMonths(-3))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    private static IHealthAlertRule CreateFalconRule(string name) => name switch
    {
        nameof(FalconMoltWeightLossRule) => new FalconMoltWeightLossRule(),
        nameof(FalconAspergillosisRiskRule) => new FalconAspergillosisRiskRule(),
        nameof(FalconBumblefootRule) => new FalconBumblefootRule(),
        nameof(FalconTrichomoniasisRule) => new FalconTrichomoniasisRule(),
        nameof(FalconMoltAnomalyRule) => new FalconMoltAnomalyRule(),
        nameof(FalconHealthCertificateRule) => new FalconHealthCertificateRule(),
        nameof(FalconPostHuntRecoveryRule) => new FalconPostHuntRecoveryRule(),
        _ => throw new ArgumentException($"Unknown rule: {name}")
    };

    private static IHealthAlertRule CreateCamelRule(string name) => name switch
    {
        nameof(CamelTrypanosomaRule) => new CamelTrypanosomaRule(),
        nameof(CamelHeatStressRule) => new CamelHeatStressRule(),
        nameof(CamelFootRotRule) => new CamelFootRotRule(),
        nameof(CamelMERSScreeningRule) => new CamelMERSScreeningRule(),
        nameof(CamelRacingFitnessRule) => new CamelRacingFitnessRule(),
        _ => throw new ArgumentException($"Unknown rule: {name}")
    };

    private static IHealthAlertRule CreateHorseRule(string name) => name switch
    {
        nameof(HorseColicRiskRule) => new HorseColicRiskRule(),
        nameof(HorseLaminitisRule) => new HorseLaminitisRule(),
        nameof(HorseEquineInfluenzaRule) => new HorseEquineInfluenzaRule(),
        nameof(HorseDentalCheckRule) => new HorseDentalCheckRule(),
        nameof(HorseDewormerRotationRule) => new HorseDewormerRotationRule(),
        _ => throw new ArgumentException($"Unknown rule: {name}")
    };

    // ── HorseColicRiskRule ──────────────────────────────────────────────────

    [Fact]
    public void HorseColic_ColicSigns_GeneratesAlert()
    {
        var rule = new HorseColicRiskRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8, weightKg: 480m,
            records: [Record("abdominal pain, rolling, pawing")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("HORSE_COLIC_RISK");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
    }

    [Fact]
    public void HorseColic_ColicSignsAndWeightLoss_HighSeverity()
    {
        var rule = new HorseColicRiskRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8, weightKg: 450m,
            records: [Record("decreased appetite, lethargy")],
            weightHistory: [W(450m, DateTime.UtcNow), W(500m, DateTime.UtcNow.AddMonths(-1))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void HorseColic_NoSigns_NoAlert()
    {
        var rule = new HorseColicRiskRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8, weightKg: 500m,
            records: [Record("routine checkup")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseColic_AlreadyTreated_NoAlert()
    {
        var rule = new HorseColicRiskRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8,
            records: [Record("colic treatment administered")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseColic_Dedup_NoAlert()
    {
        var rule = new HorseColicRiskRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8,
            records: [Record("abdominal pain")]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("HORSE_COLIC_RISK"));

        alerts.Should().BeEmpty();
    }

    // ── HorseLaminitisRule ──────────────────────────────────────────────────

    [Fact]
    public void HorseLaminitis_OverweightArabian_GeneratesAlert()
    {
        var rule = new HorseLaminitisRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 10, weightKg: 600m);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        // Always triggers for overweight high-risk breed regardless of season
        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("HORSE_LAMINITIS");
    }

    [Fact]
    public void HorseLaminitis_NormalWeight_NoAlert()
    {
        var rule = new HorseLaminitisRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 10, weightKg: 450m);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseLaminitis_AlreadyManaged_NoAlert()
    {
        var rule = new HorseLaminitisRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 10, weightKg: 600m,
            records: [Record("laminitis management ongoing")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseLaminitis_OverweightNonRiskBreed_NoSpring_NoAlert()
    {
        // This test verifies that non-risk breed without spring doesn't trigger
        // We can't control DateTime.UtcNow.Month, so we test the breed filtering
        var rule = new HorseLaminitisRule();
        var patient = CreatePatient(Species.Horse, "Thoroughbred", 10, weightKg: 600m);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        // May or may not alert depending on current month (spring vs not)
        // If it's spring, even non-risk breeds trigger. If not spring, only risk breeds.
        if (DateTime.UtcNow.Month is >= 3 and <= 5)
            alerts.Should().HaveCount(1);
        else
            alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseLaminitis_Dedup_NoAlert()
    {
        var rule = new HorseLaminitisRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 10, weightKg: 600m);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("HORSE_LAMINITIS"));

        alerts.Should().BeEmpty();
    }

    // ── HorseEquineInfluenzaRule ─────────────────────────────────────────────

    [Fact]
    public void HorseInfluenza_RespiratoryAndFever_GeneratesAlert()
    {
        var rule = new HorseEquineInfluenzaRule();
        var patient = CreatePatient(Species.Horse, "Thoroughbred", 5,
            records: [Record("cough, nasal discharge, fever")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("HORSE_EQUINE_INFLUENZA");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void HorseInfluenza_RespiratoryOnly_NoFever_NoAlert()
    {
        var rule = new HorseEquineInfluenzaRule();
        var patient = CreatePatient(Species.Horse, "Thoroughbred", 5,
            records: [Record("cough, nasal discharge")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseInfluenza_FeverOnly_NoRespiratory_NoAlert()
    {
        var rule = new HorseEquineInfluenzaRule();
        var patient = CreatePatient(Species.Horse, "Thoroughbred", 5,
            records: [Record("fever, lethargy")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseInfluenza_AlreadyDiagnosed_NoAlert()
    {
        var rule = new HorseEquineInfluenzaRule();
        var patient = CreatePatient(Species.Horse, "Thoroughbred", 5,
            records: [Record("cough, fever, influenza confirmed")]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseInfluenza_Dedup_NoAlert()
    {
        var rule = new HorseEquineInfluenzaRule();
        var patient = CreatePatient(Species.Horse, "Thoroughbred", 5,
            records: [Record("cough, fever")]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("HORSE_EQUINE_INFLUENZA"));

        alerts.Should().BeEmpty();
    }

    // ── HorseDentalCheckRule ─────────────────────────────────────────────────

    [Fact]
    public void HorseDental_NoDentalRecords_GeneratesAlert()
    {
        var rule = new HorseDentalCheckRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("HORSE_DENTAL_CHECK");
        alerts[0].Severity.Should().Be(HealthAlertSeverity.Medium);
    }

    [Fact]
    public void HorseDental_SeniorHorse_HighSeverity()
    {
        var rule = new HorseDentalCheckRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 18);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public void HorseDental_RecentDental_NoAlert()
    {
        var rule = new HorseDentalCheckRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5,
            records: [Record("dental float performed", DateTime.UtcNow.AddMonths(-6))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseDental_OldDental_GeneratesAlert()
    {
        var rule = new HorseDentalCheckRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5,
            records: [Record("dental float performed", DateTime.UtcNow.AddMonths(-14))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
    }

    [Fact]
    public void HorseDental_Foal_NoAlert()
    {
        var rule = new HorseDentalCheckRule();
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));
        var patient = new PatientAlertContext(
            ClinicId, PatientId, "Foal", Species.Horse, "Arabian", birthDate,
            150m, [], []);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseDental_Dedup_NoAlert()
    {
        var rule = new HorseDentalCheckRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 5);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("HORSE_DENTAL_CHECK"));

        alerts.Should().BeEmpty();
    }

    // ── HorseDewormerRotationRule ────────────────────────────────────────────

    [Fact]
    public void HorseDewormer_SameDewormer3Times_GeneratesAlert()
    {
        var rule = new HorseDewormerRotationRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8,
            records:
            [
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-1)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-4)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-7))
            ]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().HaveCount(1);
        alerts[0].RuleId.Should().Be("HORSE_DEWORMER_ROTATION");
    }

    [Fact]
    public void HorseDewormer_DifferentDewormers_NoAlert()
    {
        var rule = new HorseDewormerRotationRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8,
            records:
            [
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-1)),
                Record("fenbendazole administered", DateTime.UtcNow.AddMonths(-4)),
                Record("pyrantel administered", DateTime.UtcNow.AddMonths(-7))
            ]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseDewormer_TooFewRecords_NoAlert()
    {
        var rule = new HorseDewormerRotationRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8,
            records:
            [
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-1)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-4))
            ]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }

    [Fact]
    public void HorseDewormer_Dedup_NoAlert()
    {
        var rule = new HorseDewormerRotationRule();
        var patient = CreatePatient(Species.Horse, "Arabian", 8,
            records:
            [
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-1)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-4)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-7))
            ]);

        var alerts = rule.Evaluate(patient, ExistingAlertForRule("HORSE_DEWORMER_ROTATION"));

        alerts.Should().BeEmpty();
    }

    // ── All horse rules skip non-horse species ──────────────────────────────

    [Theory]
    [InlineData(nameof(HorseColicRiskRule))]
    [InlineData(nameof(HorseLaminitisRule))]
    [InlineData(nameof(HorseEquineInfluenzaRule))]
    [InlineData(nameof(HorseDentalCheckRule))]
    [InlineData(nameof(HorseDewormerRotationRule))]
    public void AllHorseRules_DogPatient_NoAlert(string ruleTypeName)
    {
        var rule = CreateHorseRule(ruleTypeName);
        var patient = CreatePatient(Species.Dog, "Labrador", 5, weightKg: 600m,
            records:
            [
                Record("cough, fever, abdominal pain, decreased appetite, ivermectin administered", DateTime.UtcNow.AddMonths(-1)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-4)),
                Record("ivermectin administered", DateTime.UtcNow.AddMonths(-7))
            ],
            weightHistory: [W(25m, DateTime.UtcNow), W(30m, DateTime.UtcNow.AddMonths(-1))]);

        var alerts = rule.Evaluate(patient, NoExistingAlerts);

        alerts.Should().BeEmpty();
    }
}
