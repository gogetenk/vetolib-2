using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Notifications.Api;
using Vetolib.Notifications.Contracts;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Infrastructure.Behaviors;

namespace Vetolib.Notifications;

/// <summary>
/// Registers all Notifications module services.
/// MassTransit consumers are discovered automatically by MassTransit
/// when AddConsumers(typeof(NotificationsModuleServiceRegistrar).Assembly) is called
/// in the host's MassTransit configuration.
/// </summary>
public static class NotificationsModuleServiceRegistrar
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(NotificationsModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(NotificationsModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        services.AddHostedService<ReminderSchedulerService>();

        // SMS provider — swap StubSmsProvider for TwilioSmsProvider when ready
        services.AddSingleton<ISmsProvider, StubSmsProvider>();

        return services;
    }

    /// <summary>
    /// Register NotificationsDbContext with a connection string (for non-Aspire scenarios / tests).
    /// Uses AddDbContext (not pooled) because MultiTenantDbContext requires scoped IClinicContext.
    /// When using Aspire, also call builder.EnrichNpgsqlDbContext&lt;NotificationsDbContext&gt;().
    /// </summary>
    public static IServiceCollection AddNotificationsDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationsDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapReminderApiEndpoints();
        return app;
    }
}
