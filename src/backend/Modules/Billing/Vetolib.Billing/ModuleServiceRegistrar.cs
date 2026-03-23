using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Billing.Api;
using Vetolib.Billing.Application;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;
using Vetolib.Billing.Infrastructure.Jobs;
using Vetolib.Shared.Infrastructure.Behaviors;

namespace Vetolib.Billing;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<BillingOptions>(configuration.GetSection(BillingOptions.SectionName));
        services.Configure<EReportingJobOptions>(configuration.GetSection(EReportingJobOptions.SectionName));

        // Tax resolver
        services.AddSingleton<ICountryTaxResolver, CountryTaxResolver>();

        // PDF generators
        services.AddSingleton<InvoicePdfGeneratorFactory>();

        // E-reporting gateway (mock — will be replaced by real implementation)
        services.AddSingleton<IEReportingGateway, MockEReportingGateway>();

        // E-reporting background job
        services.AddHostedService<EReportingJob>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly, includeInternalTypes: true);

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
        app.MapEReportingApiEndpoints();
        return app;
    }
}
