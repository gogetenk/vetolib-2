using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.GenerateSoapNotes;

internal class GenerateSoapNotesHandler : IRequestHandler<GenerateSoapNotesCommand, Result<SoapNoteDto>>
{
    private readonly ISoapNotesGenerator _soapNotesGenerator;
    private readonly ILogger<GenerateSoapNotesHandler> _logger;

    public GenerateSoapNotesHandler(
        ISoapNotesGenerator soapNotesGenerator,
        ILogger<GenerateSoapNotesHandler> logger)
    {
        _soapNotesGenerator = soapNotesGenerator;
        _logger = logger;
    }

    public async Task<Result<SoapNoteDto>> Handle(
        GenerateSoapNotesCommand cmd,
        CancellationToken ct)
    {
        var request = new SoapNoteRequest(
            Species: cmd.Species,
            Breed: cmd.Breed,
            PatientName: cmd.PatientName,
            Symptoms: cmd.Symptoms,
            Vitals: cmd.Vitals,
            Diagnosis: cmd.Diagnosis,
            TreatmentPlan: cmd.TreatmentPlan,
            Prescriptions: cmd.Prescriptions);

        _logger.LogInformation(
            "Generating SOAP notes for patient {PatientName} ({Species}/{Breed})",
            cmd.PatientName, cmd.Species, cmd.Breed);

        var result = await _soapNotesGenerator.GenerateAsync(request, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("SOAP note generation failed: {Errors}",
                string.Join(", ", result.Errors));
        }

        return result;
    }
}
