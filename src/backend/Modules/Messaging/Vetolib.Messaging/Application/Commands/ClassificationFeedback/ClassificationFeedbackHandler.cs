using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.ClassificationFeedback;

internal class ClassificationFeedbackHandler : IRequestHandler<ClassificationFeedbackCommand, Result>
{
    private readonly MessagingDbContext _context;

    public ClassificationFeedbackHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ClassificationFeedbackCommand cmd, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result.NotFound("Conversation not found");

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == cmd.MessageId && m.ConversationId == cmd.ConversationId, ct);

        if (message is null)
            return Result.NotFound("Message not found");

        var feedbackResult = message.RecordClassificationFeedback(cmd.IsCorrect);
        if (!feedbackResult.IsSuccess)
            return feedbackResult;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
