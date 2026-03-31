using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.MedicalRecords.Api;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddMedicalRecordsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        // Cross-module reader interfaces
        services.AddScoped<IPatientReader, PatientReader>();

        // Cross-module writer interfaces — used by Messaging to attach notes to medical records
        services.AddScoped<IPatientRecordWriter, PatientRecordWriter>();

        // Cross-module reader — used by AI module for predictive health alerts
        services.AddScoped<IPatientAlertDataReader, PatientAlertDataReader>();

        // Cross-module linker — used by Auth module for owner registration auto-link
        services.AddScoped<IOwnerAccountLinker, OwnerAccountLinker>();

        return services;
    }

    /// <summary>
    /// Register MedicalRecordsDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;MedicalRecordsDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddMedicalRecordsDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MedicalRecordsDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapMedicalRecordsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatientEndpoints();
        app.MapOwnerEndpoints();
        app.MapMedicalRecordEndpoints();
        app.MapDrugCatalogEndpoints();
        app.MapWeightEndpoints();
        app.MapMedicalRecordTemplateEndpoints();
        app.MapSharedRecordEndpoints();
        return app;
    }
}
