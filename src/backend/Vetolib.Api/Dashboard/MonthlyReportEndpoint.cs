using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;
using IHttpResult = Microsoft.AspNetCore.Http.IResult;

namespace Vetolib.Api.Dashboard;

internal static class MonthlyReportEndpoint
{
    internal static IEndpointRouteBuilder MapMonthlyReportEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/dashboard/report/monthly", GetMonthlyReport)
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("api")
            .WithTags("Dashboard")
            .WithName("GetMonthlyReport")
            .WithSummary("Get monthly clinic analytics report")
            .WithDescription(
                "Returns a comprehensive monthly report with key metrics: patients, appointments, " +
                "revenue, top consultation reasons, species distribution, vet workload, and NPS score. " +
                "Admin role required. Year and month parameters identify the report period.");

        return app;
    }

    private static async Task<IHttpResult> GetMonthlyReport(
        int year,
        int month,
        ISender sender,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MonthlyReportEndpoint");

        // Validate year/month parameters
        if (year < 2020 || year > 2100)
            return Result<MonthlyReportDto>.Invalid(
                new ValidationError("year", "Year must be between 2020 and 2100.", "INVALID_YEAR", ValidationSeverity.Error))
                .ToMinimalApiResult();

        if (month < 1 || month > 12)
            return Result<MonthlyReportDto>.Invalid(
                new ValidationError("month", "Month must be between 1 and 12.", "INVALID_MONTH", ValidationSeverity.Error))
                .ToMinimalApiResult();

        // Parallelize all independent queries
        var patientCountTask = sender.Send(new GetPatientCountQuery());
        var appointmentsAnalyticsTask = sender.Send(new GetAppointmentsAnalyticsQuery());
        var revenueByMonthTask = sender.Send(new GetRevenueByMonthQuery());
        var consultationBreakdownTask = sender.Send(new GetConsultationBreakdownQuery());
        var speciesTask = sender.Send(new GetPatientsBySpeciesQuery());
        var vetWorkloadTask = sender.Send(new GetVetWorkloadQuery());

        await Task.WhenAll(
            patientCountTask,
            appointmentsAnalyticsTask,
            revenueByMonthTask,
            consultationBreakdownTask,
            speciesTask,
            vetWorkloadTask);

        // Check results and build report
        var patientCountResult = patientCountTask.Result;
        if (!patientCountResult.IsSuccess)
        {
            logger.LogWarning("MonthlyReport: GetPatientCountQuery failed — {Errors}",
                string.Join("; ", patientCountResult.Errors));
            return Result<MonthlyReportDto>.Error(
                "REPORT_PATIENTS_UNAVAILABLE:Unable to retrieve patient count")
                .ToMinimalApiResult();
        }

        var appointmentsResult = appointmentsAnalyticsTask.Result;
        if (!appointmentsResult.IsSuccess)
        {
            logger.LogWarning("MonthlyReport: GetAppointmentsAnalyticsQuery failed — {Errors}",
                string.Join("; ", appointmentsResult.Errors));
            return Result<MonthlyReportDto>.Error(
                "REPORT_APPOINTMENTS_UNAVAILABLE:Unable to retrieve appointment analytics")
                .ToMinimalApiResult();
        }

        var revenueResult = revenueByMonthTask.Result;
        if (!revenueResult.IsSuccess)
        {
            logger.LogWarning("MonthlyReport: GetRevenueByMonthQuery failed — {Errors}",
                string.Join("; ", revenueResult.Errors));
            return Result<MonthlyReportDto>.Error(
                "REPORT_REVENUE_UNAVAILABLE:Unable to retrieve revenue data")
                .ToMinimalApiResult();
        }

        var consultationResult = consultationBreakdownTask.Result;
        if (!consultationResult.IsSuccess)
        {
            logger.LogWarning("MonthlyReport: GetConsultationBreakdownQuery failed — {Errors}",
                string.Join("; ", consultationResult.Errors));
            return Result<MonthlyReportDto>.Error(
                "REPORT_CONSULTATION_UNAVAILABLE:Unable to retrieve consultation breakdown")
                .ToMinimalApiResult();
        }

        var speciesResult = speciesTask.Result;
        if (!speciesResult.IsSuccess)
        {
            logger.LogWarning("MonthlyReport: GetPatientsBySpeciesQuery failed — {Errors}",
                string.Join("; ", speciesResult.Errors));
            return Result<MonthlyReportDto>.Error(
                "REPORT_SPECIES_UNAVAILABLE:Unable to retrieve species distribution")
                .ToMinimalApiResult();
        }

        var vetWorkloadResult = vetWorkloadTask.Result;
        if (!vetWorkloadResult.IsSuccess)
        {
            logger.LogWarning("MonthlyReport: GetVetWorkloadQuery failed — {Errors}",
                string.Join("; ", vetWorkloadResult.Errors));
            return Result<MonthlyReportDto>.Error(
                "REPORT_VET_WORKLOAD_UNAVAILABLE:Unable to retrieve vet workload")
                .ToMinimalApiResult();
        }

        // Build the report using the pure builder
        var report = MonthlyReportBuilder.Build(
            year, month,
            patientCountResult.Value,
            appointmentsResult.Value,
            revenueResult.Value,
            consultationResult.Value,
            speciesResult.Value,
            vetWorkloadResult.Value);

        return Result<MonthlyReportDto>.Success(report).ToMinimalApiResult();
    }
}

// Monthly report DTOs — internal to Vetolib.Api
internal record MonthlyReportDto(
    string ReportPeriod,
    string GeneratedAt,
    PatientSummaryDto Patients,
    AppointmentSummaryDto Appointments,
    RevenueSummaryDto Revenue,
    IReadOnlyList<ConsultationReasonDto> TopConsultationReasons,
    IReadOnlyList<ReportSpeciesDto> SpeciesDistribution,
    IReadOnlyList<ReportVetWorkloadDto> VetWorkload,
    decimal? NpsScore);

internal record PatientSummaryDto(int TotalPatients);

internal record AppointmentSummaryDto(
    int Total,
    int Completed,
    int Cancelled,
    int NoShow,
    decimal NoShowRate);

internal record RevenueSummaryDto(
    decimal TotalAed,
    decimal AveragePerAppointmentAed,
    string Currency);

internal record ConsultationReasonDto(string Reason, int Count);

internal record ReportSpeciesDto(string Species, int Count);

internal record ReportVetWorkloadDto(Guid VeterinarianId, string VeterinarianName, int AppointmentCount);
