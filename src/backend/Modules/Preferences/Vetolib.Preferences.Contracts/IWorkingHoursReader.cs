using Ardalis.Result;

namespace Vetolib.Preferences.Contracts;

/// <summary>
/// Cross-module interface for other modules (e.g. Agenda) to read clinic working hours.
/// Implemented in the Preferences runtime assembly.
/// </summary>
public interface IWorkingHoursReader
{
    /// <summary>
    /// Returns all 7 days of working hours for the current clinic (tenant-filtered).
    /// </summary>
    Task<Result<IReadOnlyList<WorkingHoursDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns working hours for a specific day of the week for the current clinic.
    /// </summary>
    Task<Result<WorkingHoursDto>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken ct = default);
}
