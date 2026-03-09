using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Billing.Api;
using Vetolib.Billing.Application.Behaviors;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly);

        return services;
    }

    /// <summary>
    /// Register BillingDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;BillingDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddBillingDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BillingDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapInvoiceApiEndpoints();
        return app;
    }
}
