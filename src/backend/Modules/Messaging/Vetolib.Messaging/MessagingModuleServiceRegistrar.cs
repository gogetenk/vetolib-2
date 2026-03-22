using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;
using Vetolib.Messaging.Api;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging;

public static class MessagingModuleServiceRegistrar
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, IConfiguration configuration)
    {
        // File storage — Azure Blob when connection string is configured, otherwise local
        var azureSection = configuration.GetSection(AzureBlobStorageOptions.SectionName);
        if (!string.IsNullOrEmpty(azureSection[nameof(AzureBlobStorageOptions.ConnectionString)]))
        {
            services.Configure<AzureBlobStorageOptions>(azureSection);
            services.AddScoped<IFileStorage, AzureBlobFileStorage>();
        }
        else
        {
            services.Configure<LocalFileStorageOptions>(configuration.GetSection(LocalFileStorageOptions.SectionName));
            services.AddScoped<IFileStorage, LocalFileStorage>();
        }

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

        // Message classification — AI-backed with keyword fallback via circuit breaker
        services.AddScoped<KeywordFallbackClassifier>();
        var chatClientRegistered = services.Any(s => s.ServiceType == typeof(IChatClient));
        if (chatClientRegistered)
        {
            services.AddScoped<ClaudeMessageClassifier>();
            services.AddScoped<IMessageClassifier>(sp =>
                new ResilientMessageClassifier(
                    sp.GetRequiredService<ClaudeMessageClassifier>(),
                    sp.GetRequiredService<KeywordFallbackClassifier>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ResilientMessageClassifier>>()));
        }
        else
        {
            services.AddScoped<IMessageClassifier>(sp =>
                sp.GetRequiredService<KeywordFallbackClassifier>());
        }

        // Emergency escalation background service
        services.AddHostedService<EmergencyEscalationBackgroundService>();

        // Pending upload cleanup — removes expired uploads every hour
        services.AddHostedService<PendingUploadCleanupService>();

        // SSE broadcaster — singleton so all scopes share the same connection registry
        services.AddSingleton<IMessagingEventBroadcaster, MessagingEventBroadcaster>();

        // Portal context (scoped per request, populated by MagicLinkEndpointFilter)
        services.AddScoped<IPortalContext, PortalContext>();

        // WhatsApp — token encryption
        // In non-Development environments, WhatsApp:EncryptionKey MUST be configured.
        // A random fallback key means encrypted tokens become unrecoverable after restart.
        var encryptionKeyBase64 = configuration["WhatsApp:EncryptionKey"];
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
        if (!string.IsNullOrEmpty(encryptionKeyBase64))
        {
            var keyBytes = Convert.FromBase64String(encryptionKeyBase64);
            services.AddSingleton<ITokenEncryptor>(new AesTokenEncryptor(keyBytes));
        }
        else if (string.Equals(aspnetEnv, "Development", StringComparison.OrdinalIgnoreCase))
        {
            // Dev/test only: generate a random key (tokens will be lost on restart)
            var devKey = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(devKey);
            services.AddSingleton<ITokenEncryptor>(new AesTokenEncryptor(devKey));
        }
        else
        {
            throw new InvalidOperationException(
                "WhatsApp:EncryptionKey is required in non-Development environments. " +
                "Generate a 32-byte base64 key and set it in configuration.");
        }

        // WhatsApp — channel dispatcher
        services.AddScoped<IChannelDispatcher, WhatsAppSender>();

        // HttpClient for WhatsApp Graph API with Polly resilience
        services.AddHttpClient("WhatsApp")
            .AddResilienceHandler("WhatsAppResilience", builder =>
            {
                // Timeout: 10s per attempt
                builder.AddTimeout(TimeSpan.FromSeconds(10));

                // Retry: 2 retries with exponential backoff (1s, 2s)
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutRejectedException>()
                        .HandleResult(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
                });

                // Circuit Breaker: opens after 3 consecutive failures, stays open 30s
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 1.0,          // 100% failure ratio within sampling window
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 3,        // At least 3 calls before evaluating
                    BreakDuration = TimeSpan.FromSeconds(30),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutRejectedException>()
                        .HandleResult(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
                });
            });

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
        app.MapMessagingSseEndpoints();
        app.MapWhatsAppEndpoints();
        return app;
    }
}
