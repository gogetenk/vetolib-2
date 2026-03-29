using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetWeightCurve;

internal record GetWeightCurveQuery(Guid PatientId)
    : IRequest<Result<IReadOnlyList<WeightCurvePointDto>>>;
