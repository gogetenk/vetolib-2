using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Messaging.Api;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging;

public static class MessagingModuleServiceRegistrar
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MessagingModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(MessagingModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        // Internal services
        services.AddScoped<IBusinessHoursChecker, BusinessHoursChecker>();

        // AI triage routing services
        services.AddScoped<IMessageRouter, MessageRouter>();
        services.AddScoped<ITriageOrchestrator, TriageOrchestrator>();

        // Emergency escalation background service
        services.AddHostedService<EmergencyEscalationBackgroundService>();

        // SSE broadcaster — singleton so all scopes share the same connection registry
        services.AddSingleton<IMessagingEventBroadcaster, MessagingEventBroadcaster>();

        // Portal context (scoped per request, populated by MagicLinkEndpointFilter)
        services.AddScoped<IPortalContext, PortalContext>();

        // Override IClinicContext with PortalAwareClinicContext for this module:
        // resolves ClinicId from portal magic link token (HttpContext.Items) first,
        // then falls back to the standard JWT claim. This enables MessagingDbContext's
        // global query filter to work for portal endpoints without IgnoreQueryFilters().
        // For non-portal requests the JWT fallback path is identical to ClinicContext.
        services.AddScoped<IClinicContext, PortalAwareClinicContext>();

        return services;
    }

    /// <summary>
    /// Register MessagingDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;MessagingDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddMessagingDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MessagingDbContext>((sp, opts) =>
        {
            opts.UseNpgsql(connectionString);
        });
        return services;
    }

    public static IEndpointRouteBuilder MapMessagingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMessagingApiEndpoints();
        app.MapPortalEndpoints();
        app.MapMessagingSseEndpoints();
        return app;
    }
}
