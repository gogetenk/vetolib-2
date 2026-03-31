using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.LinkOwnerByMicrochip;

internal record LinkOwnerByMicrochipCommand(
    Guid OwnerAccountId,
    string MicrochipNumber) : IRequest<Result>;
