using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Notifications.Contracts.Dtos;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Domain;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Application.Queries.GetReminderConfig;

internal class GetReminderConfigHandler : IRequestHandler<GetReminderConfigQuery, Result<ReminderConfigDto>>
{
    private readonly NotificationsDbContext _dbContext;
    private readonly IClinicContext _clinicContext;

    public GetReminderConfigHandler(NotificationsDbContext dbContext, IClinicContext clinicContext)
    {
        _dbContext = dbContext;
        _clinicContext = clinicContext;
    }

    public async Task<Result<ReminderConfigDto>> Handle(GetReminderConfigQuery request, CancellationToken ct)
    {
        var config = await _dbContext.ReminderConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ClinicId == _clinicContext.ClinicId, ct);

        if (config is null)
        {
            // Return defaults if no config exists yet
            return Result<ReminderConfigDto>.Success(new ReminderConfigDto(
                Appointment24hEnabled: true,
                VaccinationDueEnabled: true,
                FollowUpEnabled: true,
                Appointment24hLeadTimeHours: 24,
                VaccinationDueLeadTimeDays: 7,
                PreferredReminderChannel: ReminderChannel.Email));
        }

        return Result<ReminderConfigDto>.Success(new ReminderConfigDto(
            config.Appointment24hEnabled,
            config.VaccinationDueEnabled,
            config.FollowUpEnabled,
            config.Appointment24hLeadTimeHours,
            config.VaccinationDueLeadTimeDays,
            config.PreferredReminderChannel));
    }
}
