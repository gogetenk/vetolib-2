namespace Vetolib.Agenda.Contracts;

public interface IAppointmentReader
{
    Task<IReadOnlyList<AppointmentHistoryDto>> GetOwnerHistoryAsync(
        Guid ownerId, int limit, CancellationToken ct);

    Task<AppointmentFeaturesDto?> GetFeaturesForPredictionAsync(
        Guid appointmentId, CancellationToken ct);

    Task<IReadOnlyList<AppointmentFeaturesDto>> GetAppointmentsByDateAsync(
        DateOnly date, CancellationToken ct);

    Task<int> GetCompletedAppointmentCountAsync(CancellationToken ct);
}
