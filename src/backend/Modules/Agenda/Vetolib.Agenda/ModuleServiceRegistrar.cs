using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Agenda.Api;
using Vetolib.Agenda.Application.Services;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddAgendaModule(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        // IAppointmentReader — used by the AI module for no-show prediction feature collection
        services.AddScoped<IAppointmentReader, AppointmentReader>();

        // IOnCallVetReader — used by the Messaging module for emergency after-hours escalation
        services.AddScoped<IOnCallVetReader, OnCallVetReader>();

        // SlotScoringService and DurationEstimator — injected directly (not via interface) by SuggestSlotHandler
        services.AddScoped<SlotScoringService>();
        services.AddScoped<DurationEstimator>();

        return services;
    }

    /// <summary>
    /// Register AgendaDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;AgendaDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddAgendaDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AgendaDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static WebApplication MapAgendaEndpoints(this WebApplication app)
    {
        app.MapAppointmentApiEndpoints();
        app.MapConsultationTypeEndpoints();
        return app;
    }
}
