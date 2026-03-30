using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using Microsoft.Extensions.ML;
using Vetolib.AI.Api;
using Vetolib.AI.Application;
using Vetolib.AI.Application.ML;
using Vetolib.AI.Application.Rules;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;
using Vetolib.Shared.Infrastructure.Behaviors;

namespace Vetolib.AI;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddAIModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<AIOptions>(configuration.GetSection(AIOptions.SectionName));

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        // IChatClient — dual-mode: Ollama (dev, OpenAI-compatible API) / OpenAI (prod)
        // Ollama exposes an OpenAI-compatible API at /v1, so we use the OpenAI client for both.
        // Dev: set ConnectionStrings:ollama (injected by Aspire .WithReference(ollama))
        // Prod: set AI:Provider = "openai", AI:OpenAI:Endpoint, AI:OpenAI:ApiKey, AI:OpenAI:Model
        var ollamaConnectionString = configuration.GetConnectionString("ollama");
        var aiProvider = configuration["AI:Provider"] ?? "ollama";

        IChatClient? innerClient = null;

        if (aiProvider == "openai")
        {
            var endpoint = configuration["AI:OpenAI:Endpoint"]
                ?? "https://api.openai.com/v1";
            var apiKey = configuration["AI:OpenAI:ApiKey"] ?? string.Empty;
            var model = configuration["AI:OpenAI:Model"] ?? "gpt-4o";

            innerClient = new OpenAIClient(
                    new System.ClientModel.ApiKeyCredential(apiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
                .GetChatClient(model)
                .AsIChatClient();
        }
        else if (!string.IsNullOrWhiteSpace(ollamaConnectionString))
        {
            // Ollama exposes OpenAI-compatible API at <host>/v1
            var model = configuration["AI:Ollama:Model"] ?? "llama3.1";
            var ollamaApiBase = ollamaConnectionString.TrimEnd('/') + "/v1";

            innerClient = new OpenAIClient(
                    new System.ClientModel.ApiKeyCredential("ollama"),
                    new OpenAIClientOptions { Endpoint = new Uri(ollamaApiBase) })
                .GetChatClient(model)
                .AsIChatClient();
        }

        if (innerClient is not null)
        {
            services
                .AddChatClient(innerClient)
                .UseOpenTelemetry();
        }

        // ML.NET no-show prediction service
        // PredictionEnginePool is registered only if the model file exists.
        // If not present, NoShowPredictionService returns ML_MODEL_NOT_LOADED gracefully.
        var modelPath = configuration["AI:NoShow:ModelPath"];
        if (!string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath))
        {
            services.AddPredictionEnginePool<NoShowInput, NoShowOutput>()
                .FromFile(modelPath, watchForChanges: false);
        }

        services.AddScoped<INoShowPredictionService, NoShowPredictionService>();

        // SOAP Notes Generator — uses LLM if IChatClient is available, template fallback otherwise
        // When LLM is available, wraps ClaudeSoapNotesGenerator with Polly Circuit Breaker;
        // falls back to TemplateSoapNotesGenerator when the circuit is open.
        services.AddScoped<TemplateSoapNotesGenerator>();
        if (innerClient is not null)
        {
            services.AddScoped<ClaudeSoapNotesGenerator>();
            services.AddScoped<ISoapNotesGenerator>(sp =>
                new ResilientSoapNotesGenerator(
                    sp.GetRequiredService<ClaudeSoapNotesGenerator>(),
                    sp.GetRequiredService<TemplateSoapNotesGenerator>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ResilientSoapNotesGenerator>>()));
        }
        else
        {
            services.AddScoped<ISoapNotesGenerator, TemplateSoapNotesGenerator>();
        }

        // Health Alert Rules — 17 rule-based alert generators
        services.AddTransient<IHealthAlertRule, CatRenalScreeningRule>();
        services.AddTransient<IHealthAlertRule, CardiacBreedRule>();
        services.AddTransient<IHealthAlertRule, HipDysplasiaRule>();
        services.AddTransient<IHealthAlertRule, BrachycephalicAirwayRule>();
        services.AddTransient<IHealthAlertRule, DentalProphylaxisRule>();
        services.AddTransient<IHealthAlertRule, SeniorWellnessRule>();
        services.AddTransient<IHealthAlertRule, VaccinationOverdueRule>();
        services.AddTransient<IHealthAlertRule, VaccinationDueRule>();
        services.AddTransient<IHealthAlertRule, WeightTrendRule>();
        services.AddTransient<IHealthAlertRule, DiabetesRiskRule>();
        services.AddTransient<IHealthAlertRule, ArthritisFollowUpRule>();
        // Falcon-specific rules
        services.AddTransient<IHealthAlertRule, FalconMoltWeightLossRule>();
        services.AddTransient<IHealthAlertRule, FalconAspergillosisRiskRule>();
        services.AddTransient<IHealthAlertRule, FalconBumblefootRule>();
        services.AddTransient<IHealthAlertRule, FalconTrichomoniasisRule>();
        services.AddTransient<IHealthAlertRule, FalconMoltAnomalyRule>();
        services.AddTransient<IHealthAlertRule, FalconHealthCertificateRule>();
        services.AddTransient<IHealthAlertRule, FalconPostHuntRecoveryRule>();

        // Health Alert Background Job
        services.Configure<HealthAlertJobOptions>(configuration.GetSection(HealthAlertJobOptions.SectionName));
        var jobOptions = new HealthAlertJobOptions();
        configuration.GetSection(HealthAlertJobOptions.SectionName).Bind(jobOptions);
        if (jobOptions.Enabled)
        {
            services.AddHostedService<HealthAlertGeneratorJob>();
        }

        return services;
    }

    /// <summary>
    /// Register AIDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;AIDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddAIDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AIDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAIApiEndpoints(); // This already calls MapHealthAlertEndpoints() internally
        return app;
    }
}
