using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Application.Commands.RemoveFromWaitlist;

internal record RemoveFromWaitlistCommand(Guid EntryId) : IRequest<Result>;
