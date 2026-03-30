using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.AddToWaitlist;

internal record AddToWaitlistCommand(
    Guid ClinicId,
    Guid PatientId,
    string OwnerName,
    string OwnerPhone,
    string? OwnerEmail,
    DateOnly PreferredDate,
    PreferredTimeSlot PreferredTimeSlot,
    Guid? VetPreference,
    string? Reason) : IRequest<Result<WaitlistEntryDto>>;
