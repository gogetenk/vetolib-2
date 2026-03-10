using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.CreateTemplate;

internal class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, Result<ResponseTemplateDto>>
{
    private readonly MessagingDbContext _context;

    public CreateTemplateHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ResponseTemplateDto>> Handle(CreateTemplateCommand command, CancellationToken ct)
    {
        var result = ResponseTemplate.Create(
            command.ClinicId,
            command.Name,
            command.ContentEn,
            command.ContentAr,
            command.Category);

        if (!result.IsSuccess)
            return Result<ResponseTemplateDto>.Invalid(result.ValidationErrors);

        _context.ResponseTemplates.Add(result.Value);
        await _context.SaveChangesAsync(ct);

        return Result<ResponseTemplateDto>.Success(result.Value.ToDto());
    }
}
