using Ardalis.Result;
using MediatR;
using Vetolib.Notifications.Contracts.Dtos;

namespace Vetolib.Notifications.Application.Commands.UpdateReminderConfig;

internal record UpdateReminderConfigCommand(
    bool Appointment24hEnabled,
    bool VaccinationDueEnabled,
    bool FollowUpEnabled,
    int Appointment24hLeadTimeHours,
    int VaccinationDueLeadTimeDays) : IRequest<Result<ReminderConfigDto>>;
