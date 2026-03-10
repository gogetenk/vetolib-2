using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Preferences.Application.Services;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Infrastructure.Behaviors;

namespace Vetolib.Preferences;

public static class PreferencesModuleServiceRegistrar
{
    public static IServiceCollection AddPreferencesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR — register handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(PreferencesModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(PreferencesModuleServiceRegistrar).Assembly);

        // IPreferenceChecker — cross-module interface implemented here
        services.AddScoped<IPreferenceChecker, PreferenceChecker>();

        return services;
    }

    /// <summary>
    /// Register PreferencesDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;PreferencesDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddPreferencesDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PreferencesDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapPreferencesEndpoints(this IEndpointRouteBuilder app)
    {
        // No endpoints in this scaffold task — they will be added in subsequent tasks.
        return app;
    }
}
