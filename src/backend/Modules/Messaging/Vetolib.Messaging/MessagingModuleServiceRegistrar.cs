using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Messaging.Api;
using Vetolib.Messaging.Application.Services;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Messaging.Infrastructure;

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
        services.AddValidatorsFromAssembly(typeof(MessagingModuleServiceRegistrar).Assembly);

        // Internal services
        services.AddScoped<IBusinessHoursChecker, BusinessHoursChecker>();

        // Portal context (scoped per request, populated by MagicLinkEndpointFilter)
        services.AddScoped<IPortalContext, PortalContext>();

        return services;
    }

    /// <summary>
    /// Register MessagingDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;MessagingDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddMessagingDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MessagingDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapMessagingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMessagingApiEndpoints();
        app.MapPortalEndpoints();
        return app;
    }
}
