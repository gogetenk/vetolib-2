using Microsoft.ML.Data;

namespace Vetolib.AI.Application.ML;

/// <summary>
/// ML.NET input schema for the no-show prediction model.
/// 8 behavioral/scheduling features — no PII.
/// </summary>
internal sealed class NoShowInput
{
    /// <summary>Historical no-show rate for this owner (0.0 to 1.0).</summary>
    [ColumnName("historical_noshow_rate")]
    public float HistoricalNoShowRate { get; set; }

    /// <summary>Day of week: 0 = Sunday, 6 = Saturday (UAE week: Sun–Thu).</summary>
    [ColumnName("day_of_week")]
    public int DayOfWeek { get; set; }

    /// <summary>Hour of the appointment (e.g. 10.5 = 10:30).</summary>
    [ColumnName("hour_of_day")]
    public float HourOfDay { get; set; }

    /// <summary>Days since the owner's last clinic visit (0 = new patient).</summary>
    [ColumnName("days_since_last_visit")]
    public int DaysSinceLastVisit { get; set; }

    /// <summary>Appointment type / consultation reason (categorical).</summary>
    [ColumnName("appointment_type")]
    public string AppointmentType { get; set; } = "general";

    /// <summary>Total number of past appointments for this owner.</summary>
    [ColumnName("owner_total_appointments")]
    public int OwnerTotalAppointments { get; set; }

    /// <summary>Whether an SMS/email reminder was sent.</summary>
    [ColumnName("was_reminder_sent")]
    public bool WasReminderSent { get; set; }

    /// <summary>Days between booking and appointment date.</summary>
    [ColumnName("lead_time_days")]
    public int LeadTimeDays { get; set; }
}
