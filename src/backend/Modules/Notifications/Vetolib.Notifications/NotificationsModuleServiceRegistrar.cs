using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Notifications.Api;
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

        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapReminderApiEndpoints();
        return app;
    }
}
