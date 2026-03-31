using Vetolib.Agenda.Contracts;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Api.Dashboard;

/// <summary>
/// Pure function that assembles a MonthlyReportDto from raw module query results.
/// Extracted for testability — no I/O, no DI.
/// </summary>
internal static class MonthlyReportBuilder
{
    internal static MonthlyReportDto Build(
        int year,
        int month,
        int totalPatients,
        AppointmentsAnalyticsDto appointmentsAnalytics,
        IReadOnlyList<RevenueByMonthDto> revenueByMonth,
        IReadOnlyList<ConsultationBreakdownDto> consultationBreakdown,
        IReadOnlyList<PatientsBySpeciesDto> speciesData,
        IReadOnlyList<VetWorkloadDto> vetWorkloadData)
    {
        var monthKey = $"{year:D4}-{month:D2}";

        // Appointment stats
        var totalAppointments = appointmentsAnalytics.AppointmentsByStatus.Sum(s => s.Count);
        var completedCount = appointmentsAnalytics.AppointmentsByStatus
            .Where(s => s.Status == "COMPLETED")
            .Sum(s => s.Count);
        var cancelledCount = appointmentsAnalytics.AppointmentsByStatus
            .Where(s => s.Status == "CANCELLED")
            .Sum(s => s.Count);
        var noShowCount = appointmentsAnalytics.AppointmentsByStatus
            .Where(s => s.Status == "NOSHOW")
            .Sum(s => s.Count);

        // Revenue for the requested month
        var monthRevenue = revenueByMonth.FirstOrDefault(r => r.Month == monthKey);
        var totalRevenue = monthRevenue?.Total ?? 0m;
        var currency = monthRevenue?.Currency ?? "AED";
        var avgRevenuePerAppointment = completedCount > 0
            ? Math.Round(totalRevenue / completedCount, 2)
            : 0m;

        // Top 5 consultation reasons
        var topConsultationReasons = consultationBreakdown
            .OrderByDescending(c => c.Count)
            .Take(5)
            .Select(c => new ConsultationReasonDto(c.ConsultationType, c.Count))
            .ToList();

        // Species distribution
        var speciesDistribution = speciesData
            .OrderByDescending(s => s.Count)
            .Select(s => new ReportSpeciesDto(s.Species, s.Count))
            .ToList();

        // Vet workload
        var vetWorkload = vetWorkloadData
            .OrderByDescending(v => v.AppointmentCount)
            .Select(v => new ReportVetWorkloadDto(v.VeterinarianId, v.VeterinarianName, v.AppointmentCount))
            .ToList();

        return new MonthlyReportDto(
            ReportPeriod: monthKey,
            GeneratedAt: DateTime.UtcNow.ToString("o"),
            Patients: new PatientSummaryDto(totalPatients),
            Appointments: new AppointmentSummaryDto(
                totalAppointments, completedCount, cancelledCount, noShowCount,
                appointmentsAnalytics.NoShowRate),
            Revenue: new RevenueSummaryDto(totalRevenue, avgRevenuePerAppointment, currency),
            TopConsultationReasons: topConsultationReasons,
            SpeciesDistribution: speciesDistribution,
            VetWorkload: vetWorkload,
            NpsScore: null);
    }
}
