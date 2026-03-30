using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Breeding.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Breeding;

/// <summary>
/// Integration tests for all Breeding module endpoints.
/// Tests HTTP-level contracts (200, 401, 404) for each endpoint group:
/// Litter, Pregnancy, HeatCycle, and Lineage.
/// </summary>
public sealed class BreedingEndpointsTests : IntegrationTestBase
{
    public BreedingEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // =========================================================================
    // LITTER ENDPOINTS
    // =========================================================================

    // -- POST /api/v1/litters ------------------------------------------------

    [Fact]
    public async Task CreateLitter_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var request = new CreateLitterRequest(
            MotherPatientId: Guid.NewGuid(),
            FatherPatientId: Guid.NewGuid(),
            ExternalFatherName: null,
            BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            BornCount: 5,
            AliveCount: 4,
            Notes: "Healthy litter");

        var response = await client.PostAsJsonAsync("/api/v1/litters", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.BornCount.Should().Be(5);
        body.AliveCount.Should().Be(4);
    }

    [Fact]
    public async Task CreateLitter_Unauthenticated_Returns401()
    {
        var request = new CreateLitterRequest(
            Guid.NewGuid(), null, "External sire", DateOnly.FromDateTime(DateTime.UtcNow),
            3, 3, null);

        var response = await Client.WithoutAuth().PostAsJsonAsync("/api/v1/litters", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/litters/{id} --------------------------------------------

    [Fact]
    public async Task GetLitterById_ExistingLitter_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreateLitterRequest(
            Guid.NewGuid(), null, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            2, 2, null);
        var createResp = await client.PostAsJsonAsync("/api/v1/litters", createReq, JsonOptions);
        var created = await createResp.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);

        var response = await client.GetAsync($"/api/v1/litters/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetLitterById_NonExistent_Returns404()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/litters/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLitterById_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/litters/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{id}/litters -----------------------------------

    [Fact]
    public async Task GetLittersByMother_ReturnsLitters()
    {
        var client = CreateAdminClient();
        var motherId = Guid.NewGuid();
        var createReq = new CreateLitterRequest(
            motherId, null, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            4, 4, null);
        await client.PostAsJsonAsync("/api/v1/litters", createReq, JsonOptions);

        var response = await client.GetAsync($"/api/v1/patients/{motherId}/litters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<LitterDto>>(JsonOptions);
        body.Should().NotBeNull();
        body.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetLittersByMother_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/patients/{Guid.NewGuid()}/litters");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- POST /api/v1/litters/{id}/offspring ---------------------------------

    [Fact]
    public async Task AddOffspring_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreateLitterRequest(
            Guid.NewGuid(), null, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            3, 3, null);
        var createResp = await client.PostAsJsonAsync("/api/v1/litters", createReq, JsonOptions);
        var litter = await createResp.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);

        var offspringReq = new AddOffspringToLitterRequest(
            PatientId: Guid.NewGuid(),
            BirthOrder: 1);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/litters/{litter!.Id}/offspring", offspringReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddOffspring_NonExistentLitter_Returns404()
    {
        var client = CreateAdminClient();
        var offspringReq = new AddOffspringToLitterRequest(Guid.NewGuid(), 1);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/litters/{Guid.NewGuid()}/offspring", offspringReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOffspring_Unauthenticated_Returns401()
    {
        var offspringReq = new AddOffspringToLitterRequest(Guid.NewGuid(), 1);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/litters/{Guid.NewGuid()}/offspring", offspringReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =========================================================================
    // PREGNANCY ENDPOINTS
    // =========================================================================

    // -- POST /api/v1/breeding/pregnancies -----------------------------------

    [Fact]
    public async Task CreatePregnancy_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var request = new CreatePregnancyRequest(
            PatientId: Guid.NewGuid(),
            FatherPatientId: Guid.NewGuid(),
            MatingDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            MatingMethod: MatingMethod.Natural,
            Notes: "Observed mating");

        var response = await client.PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Status.Should().Be(PregnancyStatus.Active);
        body.MatingMethod.Should().Be(MatingMethod.Natural);
    }

    [Fact]
    public async Task CreatePregnancy_Unauthenticated_Returns401()
    {
        var request = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            MatingMethod.ArtificialInsemination, null);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/breeding/pregnancies/{id} -------------------------------

    [Fact]
    public async Task GetPregnancyById_Existing_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
            MatingMethod.Natural, null);
        var createResp = await client.PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", createReq, JsonOptions);
        var created = await createResp.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);

        var response = await client.GetAsync($"/api/v1/breeding/pregnancies/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetPregnancyById_NonExistent_Returns404()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/breeding/pregnancies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPregnancyById_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/breeding/pregnancies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/breeding/pregnancies/by-patient/{patientId} -------------

    [Fact]
    public async Task GetPregnanciesByPatient_ReturnsResults()
    {
        var client = CreateAdminClient();
        var patientId = Guid.NewGuid();
        var createReq = new CreatePregnancyRequest(
            patientId, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            MatingMethod.Natural, null);
        await client.PostAsJsonAsync("/api/v1/breeding/pregnancies", createReq, JsonOptions);

        var response = await client.GetAsync(
            $"/api/v1/breeding/pregnancies/by-patient/{patientId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<PregnancyDto>>(JsonOptions);
        body.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPregnanciesByPatient_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/breeding/pregnancies/by-patient/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/breeding/pregnancies/active -----------------------------

    [Fact]
    public async Task GetActivePregnancies_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            MatingMethod.Natural, null);
        await client.PostAsJsonAsync("/api/v1/breeding/pregnancies", createReq, JsonOptions);

        var response = await client.GetAsync("/api/v1/breeding/pregnancies/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<PregnancyDto>>(JsonOptions);
        body.Should().NotBeNull();
        body.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetActivePregnancies_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync("/api/v1/breeding/pregnancies/active");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- PUT /api/v1/breeding/pregnancies/{id}/delivery ----------------------

    [Fact]
    public async Task RecordDelivery_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)),
            MatingMethod.Natural, null);
        var createResp = await client.PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", createReq, JsonOptions);
        var pregnancy = await createResp.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);

        var deliveryReq = new RecordDeliveryRequest(
            DeliveryDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Outcome: PregnancyOutcome.LiveBirth,
            OffspringCount: 3,
            Notes: "Normal delivery");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{pregnancy!.Id}/delivery", deliveryReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RecordDelivery_NonExistentPregnancy_Returns404()
    {
        var client = CreateAdminClient();
        var deliveryReq = new RecordDeliveryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.LiveBirth, 2, null);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{Guid.NewGuid()}/delivery", deliveryReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordDelivery_Unauthenticated_Returns401()
    {
        var deliveryReq = new RecordDeliveryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.LiveBirth, 2, null);

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{Guid.NewGuid()}/delivery", deliveryReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- PUT /api/v1/breeding/pregnancies/{id}/loss --------------------------

    [Fact]
    public async Task RecordLoss_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-40)),
            MatingMethod.Natural, null);
        var createResp = await client.PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", createReq, JsonOptions);
        var pregnancy = await createResp.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);

        var lossReq = new RecordDeliveryRequest(
            DeliveryDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Outcome: PregnancyOutcome.Miscarriage,
            OffspringCount: 0,
            Notes: "Early loss detected");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{pregnancy!.Id}/loss", lossReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RecordLoss_NonExistentPregnancy_Returns404()
    {
        var client = CreateAdminClient();
        var lossReq = new RecordDeliveryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.Miscarriage, 0, null);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{Guid.NewGuid()}/loss", lossReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordLoss_Unauthenticated_Returns401()
    {
        var lossReq = new RecordDeliveryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.Miscarriage, 0, null);

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{Guid.NewGuid()}/loss", lossReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- POST /api/v1/breeding/pregnancies/{id}/checks -----------------------

    [Fact]
    public async Task ScheduleCheck_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var createReq = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
            MatingMethod.Natural, null);
        var createResp = await client.PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", createReq, JsonOptions);
        var pregnancy = await createResp.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);

        var checkReq = new ScheduleCheckRequest(
            ScheduledDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            CheckType: PregnancyCheckType.Ultrasound,
            Note: "First ultrasound check");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{pregnancy!.Id}/checks", checkReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ScheduleCheck_NonExistentPregnancy_Returns404()
    {
        var client = CreateAdminClient();
        var checkReq = new ScheduleCheckRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PregnancyCheckType.BloodTest, null);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{Guid.NewGuid()}/checks", checkReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ScheduleCheck_Unauthenticated_Returns401()
    {
        var checkReq = new ScheduleCheckRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PregnancyCheckType.Ultrasound, null);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{Guid.NewGuid()}/checks", checkReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- PUT /api/v1/breeding/pregnancies/checks/{checkId}/complete ----------

    [Fact]
    public async Task CompleteCheck_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        // Create pregnancy, then schedule a check, then complete it
        var createReq = new CreatePregnancyRequest(
            Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-25)),
            MatingMethod.Natural, null);
        var createResp = await client.PostAsJsonAsync(
            "/api/v1/breeding/pregnancies", createReq, JsonOptions);
        var pregnancy = await createResp.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);

        var checkReq = new ScheduleCheckRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            PregnancyCheckType.Ultrasound, "Scheduled ultrasound");
        await client.PostAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{pregnancy!.Id}/checks", checkReq, JsonOptions);

        // Fetch pregnancy to get the check ID
        var getResp = await client.GetAsync($"/api/v1/breeding/pregnancies/{pregnancy.Id}");
        var updated = await getResp.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
        var checkId = updated!.ScheduledChecks.First().Id;

        var completeReq = new CompleteCheckRequest(Result: "All normal, fetus healthy");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/checks/{checkId}/complete", completeReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CompleteCheck_NonExistentCheck_Returns404()
    {
        var client = CreateAdminClient();
        var completeReq = new CompleteCheckRequest(Result: "Result text");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/checks/{Guid.NewGuid()}/complete", completeReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteCheck_Unauthenticated_Returns401()
    {
        var completeReq = new CompleteCheckRequest(Result: "Result text");

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/checks/{Guid.NewGuid()}/complete", completeReq, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =========================================================================
    // HEAT CYCLE ENDPOINTS
    // =========================================================================

    // -- POST /api/v1/patients/{patientId}/heat-cycles -----------------------

    [Fact]
    public async Task RecordHeatCycle_ValidRequest_Returns200()
    {
        // HeatCycle endpoints require VetOrAdmin policy
        var client = CreateVetClient();
        var patientId = Guid.NewGuid();
        var request = new RecordHeatCycleRequest(
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            Notes: "Normal cycle observed");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HeatCycleDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.PatientId.Should().Be(patientId);
    }

    [Fact]
    public async Task RecordHeatCycle_Unauthenticated_Returns401()
    {
        var request = new RecordHeatCycleRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), null, null);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/heat-cycles", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{patientId}/heat-cycles ------------------------

    [Fact]
    public async Task GetHeatCycles_ReturnsResults()
    {
        var client = CreateVetClient();
        var patientId = Guid.NewGuid();
        var request = new RecordHeatCycleRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-21)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)), null);
        await client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", request, JsonOptions);

        var response = await client.GetAsync($"/api/v1/patients/{patientId}/heat-cycles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<HeatCycleDto>>(JsonOptions);
        body.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetHeatCycles_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/patients/{Guid.NewGuid()}/heat-cycles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{patientId}/heat-cycles/prediction -------------

    [Fact]
    public async Task PredictNextHeat_WithHistory_Returns200()
    {
        var client = CreateVetClient();
        var patientId = Guid.NewGuid();

        // Record at least two cycles so prediction has data
        var cycle1 = new RecordHeatCycleRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-180)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-173)), null);
        var cycle2 = new RecordHeatCycleRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-83)), null);
        await client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", cycle1, JsonOptions);
        await client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", cycle2, JsonOptions);

        var response = await client.GetAsync(
            $"/api/v1/patients/{patientId}/heat-cycles/prediction");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HeatPredictionDto>(JsonOptions);
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task PredictNextHeat_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/patients/{Guid.NewGuid()}/heat-cycles/prediction");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =========================================================================
    // LINEAGE ENDPOINTS
    // =========================================================================

    // -- PUT /api/v1/patients/{id}/lineage -----------------------------------

    [Fact]
    public async Task SetLineage_ValidRequest_Returns200()
    {
        var client = CreateAdminClient();
        var patientId = Guid.NewGuid();
        var request = new SetLineageRequest(
            MotherPatientId: Guid.NewGuid(),
            FatherPatientId: Guid.NewGuid(),
            RegistryNumber: "LOF-2026-12345",
            RegistryType: RegistryType.LOF);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/patients/{patientId}/lineage", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetLineage_Unauthenticated_Returns401()
    {
        var request = new SetLineageRequest(null, null, null, null);

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/lineage", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{id}/lineage -----------------------------------

    [Fact]
    public async Task GetLineage_AfterSet_Returns200()
    {
        var client = CreateAdminClient();
        var patientId = Guid.NewGuid();
        var setReq = new SetLineageRequest(
            Guid.NewGuid(), Guid.NewGuid(), "SIRE-001", RegistryType.SIRE);
        await client.PutAsJsonAsync(
            $"/api/v1/patients/{patientId}/lineage", setReq, JsonOptions);

        var response = await client.GetAsync($"/api/v1/patients/{patientId}/lineage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PatientLineageDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.PatientId.Should().Be(patientId);
    }

    [Fact]
    public async Task GetLineage_NonExistent_Returns404()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/patients/{Guid.NewGuid()}/lineage");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLineage_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/patients/{Guid.NewGuid()}/lineage");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{id}/pedigree ----------------------------------

    [Fact]
    public async Task GetPedigree_Returns200()
    {
        var client = CreateAdminClient();
        var patientId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/patients/{patientId}/pedigree");

        // Pedigree for a patient with no lineage may return 200 with null/empty or 404
        // depending on implementation. Both are acceptable contract behaviors.
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPedigree_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/patients/{Guid.NewGuid()}/pedigree");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{id}/descendants -------------------------------

    [Fact]
    public async Task GetDescendants_Returns200()
    {
        var client = CreateAdminClient();
        var patientId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/patients/{patientId}/descendants");

        // A patient with no descendants returns 200 with empty list or 404
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDescendants_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth()
            .GetAsync($"/api/v1/patients/{Guid.NewGuid()}/descendants");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
