using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ListPatients;

internal record ListPatientsQuery() : IRequest<Result<List<PatientDto>>>;
