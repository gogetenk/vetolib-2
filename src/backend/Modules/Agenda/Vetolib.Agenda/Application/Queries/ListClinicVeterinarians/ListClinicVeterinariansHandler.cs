using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListClinicVeterinarians;

internal class ListClinicVeterinariansHandler
    : IRequestHandler<ListClinicVeterinariansQuery, Result<List<ClinicVeterinarianDto>>>
{
    private readonly IClinicVetReader _vetReader;

    public ListClinicVeterinariansHandler(IClinicVetReader vetReader)
    {
        _vetReader = vetReader;
    }

    public async Task<Result<List<ClinicVeterinarianDto>>> Handle(
        ListClinicVeterinariansQuery query,
        CancellationToken ct)
    {
        var result = await _vetReader.GetVeterinariansForClinic(query.ClinicId, ct);

        if (!result.IsSuccess)
            return Result<List<ClinicVeterinarianDto>>.Error(new ErrorList(result.Errors));

        var dtos = result.Value
            .Select(v => new ClinicVeterinarianDto(v.Id, v.Name))
            .ToList();

        return Result<List<ClinicVeterinarianDto>>.Success(dtos);
    }
}
