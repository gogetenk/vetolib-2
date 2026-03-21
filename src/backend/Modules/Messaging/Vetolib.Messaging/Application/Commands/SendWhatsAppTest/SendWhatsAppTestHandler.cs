using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.SendWhatsAppTest;

internal sealed class SendWhatsAppTestHandler : IRequestHandler<SendWhatsAppTestCommand, Result>
{
    private readonly IChannelDispatcher _channelDispatcher;
    private readonly IClinicContext _clinicContext;

    public SendWhatsAppTestHandler(
        IChannelDispatcher channelDispatcher,
        IClinicContext clinicContext)
    {
        _channelDispatcher = channelDispatcher;
        _clinicContext = clinicContext;
    }

    public async Task<Result> Handle(SendWhatsAppTestCommand request, CancellationToken ct)
    {
        var message = new ChannelMessage(
            request.RecipientPhone,
            request.TemplateName,
            new Dictionary<string, string>(),
            _clinicContext.ClinicId);

        return await _channelDispatcher.SendAsync(message, ct);
    }
}
