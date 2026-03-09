using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatients;

internal record ImportPatientsCommand(
    Guid ClinicId,
    Stream CsvStream) : IRequest<Result<ImportReportDto>>;
