namespace Vetolib.Messaging.Application.Services;

internal interface IBusinessHoursChecker
{
    Task<bool> IsWithinBusinessHoursAsync(Guid clinicId, DateTime utcNow, CancellationToken ct = default);
}
