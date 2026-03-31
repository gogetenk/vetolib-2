using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;
using IHttpResult = Microsoft.AspNetCore.Http.IResult;

namespace Vetolib.Api.Dashboard;

internal static class DashboardEndpoints
{
    internal static IEndpointRouteBuilder MapDashboardApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Dashboard");

        group.MapGet("/stats", GetStats)
            .WithName("GetDashboardStats")
            .WithSummary("Get dashboard statistics")
            .WithDescription("Returns key metrics: today's appointment count, pending check-ins, unpaid invoice total, and total patients.")
            .CacheOutput("Dashboard1min");

        group.MapGet("/today-appointments", GetTodayAppointments)
            .WithName("GetTodayAppointments")
            .WithSummary("Get today's appointments")
            .WithDescription("Returns the list of all appointments scheduled for today with patient, vet, and status details.")
            .CacheOutput("Dashboard1min");

        group.MapGet("/recent-activity", GetRecentActivity)
            .WithName("GetRecentActivity")
            .WithSummary("Get recent activity feed")
            .WithDescription("Returns the most recent clinic activity items including appointments and status changes.");

        group.MapGet("/analytics", GetAnalytics)
            .WithName("GetDashboardAnalytics")
            .WithSummary("Get dashboard analytics")
            .WithDescription("Returns analytics data including revenue by month, no-show rate, patients by species, and appointments by status. Requires Vet or Admin role.")
            .RequireAuthorization("VetOrAdmin")
            .CacheOutput("Dashboard1min");

        group.MapGet("/revenue-trend", GetRevenueTrend)
            .WithName("GetRevenueTrend")
            .WithSummary("Get monthly revenue trend for the last 12 months")
            .WithDescription("Returns monthly revenue totals from paid invoices for the last 12 months, with zero-fill for months without revenue. Currency is AED.")
            .RequireAuthorization("VetOrAdmin")
            .CacheOutput("Dashboard1min");

        group.MapGet("/consultation-breakdown", GetConsultationBreakdown)
            .WithName("GetConsultationBreakdown")
            .WithSummary("Get appointment count per consultation type this month")
            .WithDescription("Returns the number of appointments grouped by consultation reason for the current calendar month.")
            .CacheOutput("Dashboard1min");

        group.MapGet("/species-distribution", GetSpeciesDistribution)
            .WithName("GetSpeciesDistribution")
            .WithSummary("Get patient count per species")
            .WithDescription("Returns the total number of patients grouped by species, ordered by count descending.")
            .CacheOutput("Dashboard1min");

        group.MapGet("/vet-workload", GetVetWorkload)
            .WithName("GetVetWorkload")
            .WithSummary("Get appointment count per veterinarian this week")
            .WithDescription("Returns the number of appointments per veterinarian for the current ISO week (Monday to Sunday).")
            .CacheOutput("Dashboard1min");

        app.MapMonthlyReportEndpoint();

        return app;
    }

    private static async Task<IHttpResult> GetStats(ISender sender, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DashboardEndpoints");

        // All queries use public types from .Contracts — no runtime assembly coupling
        // Parallelize independent queries to reduce latency on cache miss (P-17)
        var appointmentsTask = sender.Send(new GetTodayAppointmentsQuery());
        var unpaidTask = sender.Send(new GetUnpaidInvoicesTotalQuery());
        var patientCountTask = sender.Send(new GetPatientCountQuery());

        await Task.WhenAll(appointmentsTask, unpaidTask, patientCountTask);

        var appointmentsResult = appointmentsTask.Result;
        if (!appointmentsResult.IsSuccess)
        {
            logger.LogWarning("Dashboard GetStats: GetTodayAppointmentsQuery failed — {Errors}",
                string.Join("; ", appointmentsResult.Errors));
            return Result<DashboardStatsDto>.Error("DASHBOARD_APPOINTMENTS_UNAVAILABLE:Impossible de recuperer les rendez-vous du jour")
                .ToMinimalApiResult();
        }

        var unpaidResult = unpaidTask.Result;
        if (!unpaidResult.IsSuccess)
        {
            logger.LogWarning("Dashboard GetStats: GetUnpaidInvoicesTotalQuery failed — {Errors}",
                string.Join("; ", unpaidResult.Errors));
            return Result<DashboardStatsDto>.Error("DASHBOARD_INVOICES_UNAVAILABLE:Impossible de recuperer le total des factures impayees")
                .ToMinimalApiResult();
        }

        var patientCountResult = patientCountTask.Result;
        if (!patientCountResult.IsSuccess)
        {
            logger.LogWarning("Dashboard GetStats: GetPatientCountQuery failed — {Errors}",
                string.Join("; ", patientCountResult.Errors));
            return Result<DashboardStatsDto>.Error("DASHBOARD_PATIENTS_UNAVAILABLE:Impossible de recuperer le nombre de patients")
                .ToMinimalApiResult();
        }

        var todayAppointments = appointmentsResult.Value;
        var pendingCheckin = todayAppointments.Count(a =>
            a.Status == AppointmentStatus.Scheduled);

        var stats = new DashboardStatsDto(
            AppointmentsToday: todayAppointments.Count,
            PendingCheckin: pendingCheckin,
            UnpaidInvoicesAed: unpaidResult.Value,
            TotalPatients: patientCountResult.Value);

        return Result<DashboardStatsDto>.Success(stats).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetTodayAppointments(ISender sender)
    {
        var result = await sender.Send(new GetTodayAppointmentsQuery());

        if (!result.IsSuccess)
            return result.ToMinimalApiResult();

        var dtos = result.Value.Select(a => new TodayAppointmentDto(
            Id: a.Id.ToString(),
            PatientName: a.AnimalName,
            Species: string.Empty,
            OwnerName: a.OwnerName,
            VetName: a.VeterinarianName,
            VetId: a.VeterinarianId.ToString(),
            Status: a.Status.ToString().ToUpperInvariant(),
            ScheduledAt: a.Date.ToDateTime(a.StartTime).ToString("o")))
            .ToList();

        return Result<IReadOnlyList<TodayAppointmentDto>>.Success(dtos).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetRecentActivity(ISender sender)
    {
        var appointmentsResult = await sender.Send(new GetTodayAppointmentsQuery());

        var activities = new List<ActivityDto>();

        if (appointmentsResult.IsSuccess)
        {
            foreach (var a in appointmentsResult.Value.Take(20))
            {
                activities.Add(new ActivityDto(
                    Id: a.Id.ToString(),
                    Type: "APPOINTMENT",
                    Message: $"Appointment for {a.AnimalName} — {a.Status}",
                    OccurredAt: a.Date.ToDateTime(a.StartTime).ToString("o"),
                    RelatedId: a.Id.ToString()));
            }
        }

        return Result<IReadOnlyList<ActivityDto>>.Success(activities).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetAnalytics(ISender sender, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DashboardEndpoints");

        // Parallelize independent queries to reduce latency on cache miss (P-17)
        var revenueTask = sender.Send(new GetRevenueByMonthQuery());
        var appointmentsAnalyticsTask = sender.Send(new GetAppointmentsAnalyticsQuery());
        var speciesTask = sender.Send(new GetPatientsBySpeciesQuery());

        await Task.WhenAll(revenueTask, appointmentsAnalyticsTask, speciesTask);

        var revenueResult = revenueTask.Result;
        if (!revenueResult.IsSuccess)
        {
            logger.LogWarning("Dashboard Analytics: GetRevenueByMonthQuery failed — {Errors}",
                string.Join("; ", revenueResult.Errors));
            return Result<DashboardAnalyticsDto>.Error(
                "ANALYTICS_REVENUE_UNAVAILABLE:Impossible de recuperer les revenus par mois")
                .ToMinimalApiResult();
        }

        var appointmentsAnalyticsResult = appointmentsAnalyticsTask.Result;
        if (!appointmentsAnalyticsResult.IsSuccess)
        {
            logger.LogWarning("Dashboard Analytics: GetAppointmentsAnalyticsQuery failed — {Errors}",
                string.Join("; ", appointmentsAnalyticsResult.Errors));
            return Result<DashboardAnalyticsDto>.Error(
                "ANALYTICS_APPOINTMENTS_UNAVAILABLE:Impossible de recuperer les analytiques de rendez-vous")
                .ToMinimalApiResult();
        }

        var speciesResult = speciesTask.Result;
        if (!speciesResult.IsSuccess)
        {
            logger.LogWarning("Dashboard Analytics: GetPatientsBySpeciesQuery failed — {Errors}",
                string.Join("; ", speciesResult.Errors));
            return Result<DashboardAnalyticsDto>.Error(
                "ANALYTICS_SPECIES_UNAVAILABLE:Impossible de recuperer la repartition par espece")
                .ToMinimalApiResult();
        }

        var analytics = new DashboardAnalyticsDto(
            RevenueByMonth: revenueResult.Value.Select(r => new RevenueMonthDto(r.Month, r.Total, r.Currency)).ToList(),
            NoShowRate: appointmentsAnalyticsResult.Value.NoShowRate,
            PatientsBySpecies: speciesResult.Value.Select(s => new SpeciesCountDto(s.Species, s.Count)).ToList(),
            AppointmentsByStatus: appointmentsAnalyticsResult.Value.AppointmentsByStatus
                .Select(s => new StatusCountDto(s.Status, s.Count)).ToList());

        return Result<DashboardAnalyticsDto>.Success(analytics).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetRevenueTrend(ISender sender)
    {
        var result = await sender.Send(new GetRevenueByMonthQuery());

        if (!result.IsSuccess)
            return Result<IReadOnlyList<RevenueMonthDto>>.Error(
                "REVENUE_TREND_UNAVAILABLE:Unable to retrieve revenue trend data")
                .ToMinimalApiResult();

        var dtos = result.Value
            .Select(r => new RevenueMonthDto(r.Month, r.Total, r.Currency))
            .ToList();

        return Result<IReadOnlyList<RevenueMonthDto>>.Success(dtos).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetConsultationBreakdown(ISender sender)
    {
        var result = await sender.Send(new GetConsultationBreakdownQuery());

        if (!result.IsSuccess)
            return Result<IReadOnlyList<ConsultationBreakdownItemDto>>.Error(
                "CONSULTATION_BREAKDOWN_UNAVAILABLE:Unable to retrieve consultation breakdown")
                .ToMinimalApiResult();

        var dtos = result.Value
            .Select(r => new ConsultationBreakdownItemDto(r.ConsultationType, r.Count))
            .ToList();

        return Result<IReadOnlyList<ConsultationBreakdownItemDto>>.Success(dtos).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetSpeciesDistribution(ISender sender)
    {
        var result = await sender.Send(new GetPatientsBySpeciesQuery());

        if (!result.IsSuccess)
            return Result<IReadOnlyList<SpeciesCountDto>>.Error(
                "SPECIES_DISTRIBUTION_UNAVAILABLE:Unable to retrieve species distribution")
                .ToMinimalApiResult();

        var dtos = result.Value
            .Select(s => new SpeciesCountDto(s.Species, s.Count))
            .ToList();

        return Result<IReadOnlyList<SpeciesCountDto>>.Success(dtos).ToMinimalApiResult();
    }

    private static async Task<IHttpResult> GetVetWorkload(ISender sender)
    {
        var result = await sender.Send(new GetVetWorkloadQuery());

        if (!result.IsSuccess)
            return Result<IReadOnlyList<VetWorkloadItemDto>>.Error(
                "VET_WORKLOAD_UNAVAILABLE:Unable to retrieve vet workload data")
                .ToMinimalApiResult();

        var dtos = result.Value
            .Select(v => new VetWorkloadItemDto(v.VeterinarianId, v.VeterinarianName, v.AppointmentCount))
            .ToList();

        return Result<IReadOnlyList<VetWorkloadItemDto>>.Success(dtos).ToMinimalApiResult();
    }
}

// Dashboard-specific DTOs (Vetolib.Api only — not exposed to modules)
internal record DashboardStatsDto(
    int AppointmentsToday,
    int PendingCheckin,
    decimal UnpaidInvoicesAed,
    int TotalPatients);

internal record TodayAppointmentDto(
    string Id,
    string PatientName,
    string Species,
    string OwnerName,
    string VetName,
    string VetId,
    string Status,
    string ScheduledAt);

internal record ActivityDto(
    string Id,
    string Type,
    string Message,
    string OccurredAt,
    string? RelatedId);

internal record DashboardAnalyticsDto(
    IReadOnlyList<RevenueMonthDto> RevenueByMonth,
    decimal NoShowRate,
    IReadOnlyList<SpeciesCountDto> PatientsBySpecies,
    IReadOnlyList<StatusCountDto> AppointmentsByStatus);

internal record RevenueMonthDto(string Month, decimal Total, string Currency);
internal record SpeciesCountDto(string Species, int Count);
internal record StatusCountDto(string Status, int Count);
internal record ConsultationBreakdownItemDto(string ConsultationType, int Count);
internal record VetWorkloadItemDto(Guid VeterinarianId, string VeterinarianName, int AppointmentCount);
