using Ardalis.Result;
using MediatR;

namespace Vetolib.AI.Application.Commands.GenerateHealthAlerts;

internal record GenerateHealthAlertsCommand : IRequest<Result<int>>;
