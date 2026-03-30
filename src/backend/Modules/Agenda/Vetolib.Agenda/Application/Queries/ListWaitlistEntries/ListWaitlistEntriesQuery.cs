using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListWaitlistEntries;

internal record ListWaitlistEntriesQuery : IRequest<Result<List<WaitlistEntryDto>>>;
