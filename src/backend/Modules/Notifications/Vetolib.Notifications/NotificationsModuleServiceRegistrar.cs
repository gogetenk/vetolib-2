using Microsoft.Extensions.DependencyInjection;

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
        // MassTransit consumer registration is handled in Program.cs via
        // x.AddConsumers(typeof(NotificationsModuleServiceRegistrar).Assembly)
        // No additional registrations needed here.
        return services;
    }
}
