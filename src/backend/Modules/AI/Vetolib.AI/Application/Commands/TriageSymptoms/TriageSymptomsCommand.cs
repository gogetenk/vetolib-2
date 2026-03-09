using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.TriageSymptoms;

internal record TriageSymptomsCommand(
    Guid ClinicId,
    string Symptoms,
    string Species,
    string? Breed,
    int? AgeMonths,
    decimal? WeightKg,
    string CreatedBy) : IRequest<Result<TriageSuggestionDto>>;
