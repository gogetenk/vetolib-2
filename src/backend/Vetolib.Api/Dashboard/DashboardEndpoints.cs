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
            .CacheOutput("Dashboard1min");

        group.MapGet("/today-appointments", GetTodayAppointments)
            .WithName("GetTodayAppointments")
            .CacheOutput("Dashboard1min");

        group.MapGet("/recent-activity", GetRecentActivity)
            .WithName("GetRecentActivity");

        group.MapGet("/analytics", GetAnalytics)
            .WithName("GetDashboardAnalytics")
            .RequireAuthorization("VetOrAdmin")
            .CacheOutput("Dashboard1min");

        return app;
    }

    private static async Task<IHttpResult> GetStats(ISender sender, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DashboardEndpoints");

        // All queries use public types from .Contracts — no runtime assembly coupling
        var appointmentsResult = await sender.Send(new GetTodayAppointmentsQuery());
        if (!appointmentsResult.IsSuccess)
        {
            logger.LogWarning("Dashboard GetStats: GetTodayAppointmentsQuery failed — {Errors}",
                string.Join("; ", appointmentsResult.Errors));
            return Result<DashboardStatsDto>.Error("DASHBOARD_APPOINTMENTS_UNAVAILABLE:Impossible de recuperer les rendez-vous du jour")
                .ToMinimalApiResult();
        }

        var unpaidResult = await sender.Send(new GetUnpaidInvoicesTotalQuery());
        if (!unpaidResult.IsSuccess)
        {
            logger.LogWarning("Dashboard GetStats: GetUnpaidInvoicesTotalQuery failed — {Errors}",
                string.Join("; ", unpaidResult.Errors));
            return Result<DashboardStatsDto>.Error("DASHBOARD_INVOICES_UNAVAILABLE:Impossible de recuperer le total des factures impayees")
                .ToMinimalApiResult();
        }

        var patientCountResult = await sender.Send(new GetPatientCountQuery());
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

        var revenueResult = await sender.Send(new GetRevenueByMonthQuery());
        if (!revenueResult.IsSuccess)
        {
            logger.LogWarning("Dashboard Analytics: GetRevenueByMonthQuery failed — {Errors}",
                string.Join("; ", revenueResult.Errors));
            return Result<DashboardAnalyticsDto>.Error(
                "ANALYTICS_REVENUE_UNAVAILABLE:Impossible de recuperer les revenus par mois")
                .ToMinimalApiResult();
        }

        var appointmentsAnalyticsResult = await sender.Send(new GetAppointmentsAnalyticsQuery());
        if (!appointmentsAnalyticsResult.IsSuccess)
        {
            logger.LogWarning("Dashboard Analytics: GetAppointmentsAnalyticsQuery failed — {Errors}",
                string.Join("; ", appointmentsAnalyticsResult.Errors));
            return Result<DashboardAnalyticsDto>.Error(
                "ANALYTICS_APPOINTMENTS_UNAVAILABLE:Impossible de recuperer les analytiques de rendez-vous")
                .ToMinimalApiResult();
        }

        var speciesResult = await sender.Send(new GetPatientsBySpeciesQuery());
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
