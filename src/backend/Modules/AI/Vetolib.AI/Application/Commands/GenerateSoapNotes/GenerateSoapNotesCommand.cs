using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.GenerateSoapNotes;

internal record GenerateSoapNotesCommand(
    string Species,
    string Breed,
    string PatientName,
    string Symptoms,
    string Vitals,
    string Diagnosis,
    string TreatmentPlan,
    List<string> Prescriptions) : IRequest<Result<SoapNoteDto>>;
