using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Api.Dashboard;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Dashboard;

public class MonthlyReportBuilderTests
{
    private static readonly Guid Vet1Id = new("aaaa1111-1111-1111-1111-111111111111");
    private static readonly Guid Vet2Id = new("bbbb2222-2222-2222-2222-222222222222");

    private static AppointmentsAnalyticsDto BuildAppointmentAnalytics(
        int completed = 20, int cancelled = 3, int noShow = 2, decimal noShowRate = 8.0m)
    {
        var statuses = new List<AppointmentStatusCountDto>
        {
            new("COMPLETED", completed),
            new("CANCELLED", cancelled),
            new("NOSHOW", noShow)
        };
        return new AppointmentsAnalyticsDto(noShowRate, statuses);
    }

    private static IReadOnlyList<RevenueByMonthDto> BuildRevenueData(
        string targetMonth = "2026-03", decimal targetTotal = 15000m)
    {
        return new List<RevenueByMonthDto>
        {
            new("2026-01", 10000m, "AED"),
            new("2026-02", 12000m, "AED"),
            new(targetMonth, targetTotal, "AED")
        };
    }

    private static IReadOnlyList<ConsultationBreakdownDto> BuildConsultationBreakdown()
    {
        return new List<ConsultationBreakdownDto>
        {
            new("Vaccination", 15),
            new("General Checkup", 12),
            new("Surgery", 5),
            new("Dental Cleaning", 4),
            new("Grooming", 3),
            new("Blood Test", 2)
        };
    }

    private static IReadOnlyList<PatientsBySpeciesDto> BuildSpeciesData()
    {
        return new List<PatientsBySpeciesDto>
        {
            new("Dog", 45),
            new("Cat", 30),
            new("Bird", 10),
            new("Rabbit", 5)
        };
    }

    private static IReadOnlyList<VetWorkloadDto> BuildVetWorkload()
    {
        return new List<VetWorkloadDto>
        {
            new(Vet1Id, "Dr. Aisha Al-Mansoori", 18),
            new(Vet2Id, "Dr. Omar Khalil", 12)
        };
    }

    [Fact]
    public void Build_WithValidData_ReturnsCorrectReportPeriod()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.ReportPeriod.Should().Be("2026-03");
    }

    [Fact]
    public void Build_WithValidData_ReturnsCorrectPatientCount()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.Patients.TotalPatients.Should().Be(90);
    }

    [Fact]
    public void Build_WithValidData_ReturnsCorrectAppointmentSummary()
    {
        // Arrange
        var analytics = BuildAppointmentAnalytics(completed: 20, cancelled: 3, noShow: 2, noShowRate: 8.0m);

        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90, analytics,
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.Appointments.Total.Should().Be(25);
        report.Appointments.Completed.Should().Be(20);
        report.Appointments.Cancelled.Should().Be(3);
        report.Appointments.NoShow.Should().Be(2);
        report.Appointments.NoShowRate.Should().Be(8.0m);
    }

    [Fact]
    public void Build_WithMatchingMonthRevenue_ReturnsCorrectRevenue()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(completed: 20),
            BuildRevenueData(targetMonth: "2026-03", targetTotal: 15000m),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.Revenue.TotalAed.Should().Be(15000m);
        report.Revenue.Currency.Should().Be("AED");
        report.Revenue.AveragePerAppointmentAed.Should().Be(750m); // 15000 / 20
    }

    [Fact]
    public void Build_WithNoRevenueForMonth_ReturnsZeroRevenue()
    {
        // Arrange — revenue data doesn't include 2026-06
        var revenueData = new List<RevenueByMonthDto>
        {
            new("2026-03", 15000m, "AED")
        };

        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 6, 90,
            BuildAppointmentAnalytics(),
            revenueData,
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.Revenue.TotalAed.Should().Be(0m);
        report.Revenue.AveragePerAppointmentAed.Should().Be(0m);
        report.Revenue.Currency.Should().Be("AED");
    }

    [Fact]
    public void Build_WithZeroCompletedAppointments_ReturnsZeroAverageRevenue()
    {
        // Arrange — no completed appointments
        var analytics = BuildAppointmentAnalytics(completed: 0, cancelled: 5, noShow: 0, noShowRate: 0m);

        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90, analytics,
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.Revenue.AveragePerAppointmentAed.Should().Be(0m);
    }

    [Fact]
    public void Build_ConsultationReasons_ReturnsTop5OrderedByCount()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(), // 6 items — only top 5 returned
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.TopConsultationReasons.Should().HaveCount(5);
        report.TopConsultationReasons[0].Reason.Should().Be("Vaccination");
        report.TopConsultationReasons[0].Count.Should().Be(15);
        report.TopConsultationReasons[4].Reason.Should().Be("Grooming");
        // "Blood Test" (count=2) should NOT be in top 5
        report.TopConsultationReasons.Should().NotContain(r => r.Reason == "Blood Test");
    }

    [Fact]
    public void Build_SpeciesDistribution_OrderedByCountDescending()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.SpeciesDistribution.Should().HaveCount(4);
        report.SpeciesDistribution[0].Species.Should().Be("Dog");
        report.SpeciesDistribution[0].Count.Should().Be(45);
        report.SpeciesDistribution[3].Species.Should().Be("Rabbit");
    }

    [Fact]
    public void Build_VetWorkload_OrderedByAppointmentCountDescending()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.VetWorkload.Should().HaveCount(2);
        report.VetWorkload[0].VeterinarianName.Should().Be("Dr. Aisha Al-Mansoori");
        report.VetWorkload[0].AppointmentCount.Should().Be(18);
        report.VetWorkload[1].VeterinarianName.Should().Be("Dr. Omar Khalil");
    }

    [Fact]
    public void Build_NpsScore_IsNullWhenNotAvailable()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.NpsScore.Should().BeNull();
    }

    [Fact]
    public void Build_GeneratedAt_IsValidIso8601()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 90,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        var parsed = DateTime.TryParse(report.GeneratedAt, out _);
        parsed.Should().BeTrue();
    }

    [Fact]
    public void Build_WithEmptyData_ReturnsZeroedReport()
    {
        // Arrange
        var emptyAnalytics = new AppointmentsAnalyticsDto(0m, new List<AppointmentStatusCountDto>());
        var emptyRevenue = new List<RevenueByMonthDto>();
        var emptyConsultation = new List<ConsultationBreakdownDto>();
        var emptySpecies = new List<PatientsBySpeciesDto>();
        var emptyVetWorkload = new List<VetWorkloadDto>();

        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 3, 0,
            emptyAnalytics, emptyRevenue, emptyConsultation, emptySpecies, emptyVetWorkload);

        // Assert
        report.ReportPeriod.Should().Be("2026-03");
        report.Patients.TotalPatients.Should().Be(0);
        report.Appointments.Total.Should().Be(0);
        report.Appointments.Completed.Should().Be(0);
        report.Revenue.TotalAed.Should().Be(0m);
        report.Revenue.AveragePerAppointmentAed.Should().Be(0m);
        report.TopConsultationReasons.Should().BeEmpty();
        report.SpeciesDistribution.Should().BeEmpty();
        report.VetWorkload.Should().BeEmpty();
    }

    [Fact]
    public void Build_SingleDigitMonth_PadsWithZero()
    {
        // Act
        var report = MonthlyReportBuilder.Build(
            2026, 1, 10,
            BuildAppointmentAnalytics(),
            BuildRevenueData(),
            BuildConsultationBreakdown(),
            BuildSpeciesData(),
            BuildVetWorkload());

        // Assert
        report.ReportPeriod.Should().Be("2026-01");
    }
}
