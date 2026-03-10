using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.DismissWelcomeBanner;

internal record DismissWelcomeBannerCommand(Guid UserId) : IRequest<Result>;
