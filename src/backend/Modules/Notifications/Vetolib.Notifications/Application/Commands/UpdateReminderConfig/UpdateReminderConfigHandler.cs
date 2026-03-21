using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Notifications.Contracts.Dtos;
using Vetolib.Notifications.Domain;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Application.Commands.UpdateReminderConfig;

internal class UpdateReminderConfigHandler : IRequestHandler<UpdateReminderConfigCommand, Result<ReminderConfigDto>>
{
    private readonly NotificationsDbContext _dbContext;
    private readonly IClinicContext _clinicContext;

    public UpdateReminderConfigHandler(NotificationsDbContext dbContext, IClinicContext clinicContext)
    {
        _dbContext = dbContext;
        _clinicContext = clinicContext;
    }

    public async Task<Result<ReminderConfigDto>> Handle(UpdateReminderConfigCommand request, CancellationToken ct)
    {
        var config = await _dbContext.ReminderConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ClinicId == _clinicContext.ClinicId, ct);

        if (config is null)
        {
            var createResult = ReminderConfig.CreateDefault(_clinicContext.ClinicId);
            if (!createResult.IsSuccess)
                return Result<ReminderConfigDto>.Invalid(createResult.ValidationErrors.ToList());

            config = createResult.Value;
            _dbContext.ReminderConfigs.Add(config);
        }

        var updateResult = config.Update(
            request.Appointment24hEnabled,
            request.VaccinationDueEnabled,
            request.FollowUpEnabled,
            request.Appointment24hLeadTimeHours,
            request.VaccinationDueLeadTimeDays);

        if (!updateResult.IsSuccess)
            return Result<ReminderConfigDto>.Invalid(updateResult.ValidationErrors.ToList());

        await _dbContext.SaveChangesAsync(ct);

        return Result<ReminderConfigDto>.Success(new ReminderConfigDto(
            config.Appointment24hEnabled,
            config.VaccinationDueEnabled,
            config.FollowUpEnabled,
            config.Appointment24hLeadTimeHours,
            config.VaccinationDueLeadTimeDays));
    }
}
