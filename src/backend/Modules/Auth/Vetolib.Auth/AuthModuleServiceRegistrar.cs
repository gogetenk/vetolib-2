using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Vetolib.Auth.Api;
using Vetolib.Auth.Application;
using Vetolib.Auth.Application.Services;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth;

public static class AuthModuleServiceRegistrar
{
    internal const string LegacyScheme = "Legacy";
    internal const string KeycloakScheme = "Keycloak";
    internal const string MultiScheme = "MultiScheme";

    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration config)
    {
        // Options
        services.Configure<AuthSecurityOptions>(config.GetSection(AuthSecurityOptions.SectionName));

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AuthModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(AuthModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        // JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Owner Portal JWT Service
        services.AddScoped<IOwnerPortalJwtService, OwnerPortalJwtService>();

        // Cross-module readers
        services.AddScoped<IClinicVetReader, ClinicVetReader>();

        // Subscription enforcement
        services.AddScoped<ISubscriptionChecker, SubscriptionChecker>();

        // Claims transformation: maps Keycloak organization.id → clinic_id
        services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

        // --- Dual-stack JWT Authentication ---
        // Legacy: existing HMAC-SHA256 tokens issued by the built-in JwtTokenService.
        // Keycloak: RS256 tokens issued by Keycloak (OIDC discovery via Authority).
        // A policy scheme selects the handler based on the token's "alg" header.

        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key configuration is required. Set it in appsettings.json or environment variables.");
        var jwtIssuer = config["Jwt:Issuer"] ?? "Vetolib";
        var jwtAudience = config["Jwt:Audience"] ?? "Vetolib";

        var keycloakAuthority = config["Keycloak:Authority"];
        var keycloakAudience = config["Keycloak:Audience"] ?? "vetolib-api";
        var keycloakEnabled = !string.IsNullOrWhiteSpace(keycloakAuthority);

        var authBuilder = services.AddAuthentication(options =>
        {
            if (keycloakEnabled)
            {
                // Use policy scheme to dynamically select Legacy or Keycloak
                options.DefaultAuthenticateScheme = MultiScheme;
                options.DefaultChallengeScheme = MultiScheme;
            }
            else
            {
                // Keycloak not configured — fall back to Legacy only
                options.DefaultAuthenticateScheme = LegacyScheme;
                options.DefaultChallengeScheme = LegacyScheme;
            }
        });

        // Legacy scheme (HMAC-SHA256)
        authBuilder.AddJwtBearer(LegacyScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "sub",
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
        });

        if (keycloakEnabled)
        {
            // Keycloak scheme (RS256, OIDC discovery)
            authBuilder.AddJwtBearer(KeycloakScheme, options =>
            {
                options.Authority = keycloakAuthority;
                options.Audience = keycloakAudience;
                options.RequireHttpsMetadata = false; // dev: Aspire uses HTTP
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "realm_access.roles"
                };
            });

            // Policy scheme: inspect the JWT header "alg" to pick the right handler.
            // RS256 → Keycloak, HS256 → Legacy. Unknown → try Legacy (backward compat).
            authBuilder.AddPolicyScheme(MultiScheme, "Legacy + Keycloak selector", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return LegacyScheme;

                    var token = authHeader["Bearer ".Length..].Trim();
                    // JWT header is the first segment (base64url-encoded JSON with "alg")
                    var dotIndex = token.IndexOf('.');
                    if (dotIndex <= 0)
                        return LegacyScheme;

                    try
                    {
                        var headerJson = System.Text.Json.JsonDocument.Parse(
                            Convert.FromBase64String(PadBase64(token[..dotIndex])));
                        if (headerJson.RootElement.TryGetProperty("alg", out var alg) &&
                            alg.GetString()?.StartsWith("RS", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            return KeycloakScheme;
                        }
                    }
                    catch
                    {
                        // Malformed header — let Legacy handle (it will reject if invalid)
                    }

                    return LegacyScheme;
                };
            });
        }

        services.AddAuthorization(options =>
        {
            // Default policy: accept tokens from any registered scheme
            var schemes = keycloakEnabled
                ? new[] { LegacyScheme, KeycloakScheme }
                : new[] { LegacyScheme };

            options.DefaultPolicy = new AuthorizationPolicyBuilder(schemes)
                .RequireAuthenticatedUser()
                .Build();

            // ClinicStaff: all authenticated clinic staff (Vet, Receptionist, Admin).
            // Explicitly excludes Assistant role — used for AI endpoints that must not
            // be visible to the animal owner / external party mapped to Assistant.
            options.AddPolicy("ClinicStaff", policy =>
            {
                policy.AddAuthenticationSchemes(schemes);
                policy.RequireRole(
                    nameof(UserRole.Vet),
                    nameof(UserRole.Receptionist),
                    nameof(UserRole.Admin));
            });

            // VetOrAdmin: restricted to clinical decision-makers only.
            options.AddPolicy("VetOrAdmin", policy =>
            {
                policy.AddAuthenticationSchemes(schemes);
                policy.RequireRole(
                    nameof(UserRole.Vet),
                    nameof(UserRole.Admin));
            });
        });

        return services;
    }

    /// <summary>
    /// Pads a base64url string to standard base64 length.
    /// </summary>
    internal static string PadBase64(string base64Url)
    {
        var s = base64Url.Replace('-', '+').Replace('_', '/');
        return (s.Length % 4) switch
        {
            2 => s + "==",
            3 => s + "=",
            _ => s
        };
    }

    /// <summary>
    /// Register AuthDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;AuthDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddAuthDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuthDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthApiEndpoints();
        app.MapUserApiEndpoints();
        app.MapClinicApiEndpoints();
        app.MapClinicGroupApiEndpoints();
        app.MapOnboardingEndpoints();
        app.MapReferralApiEndpoints();
        app.MapPortalApiEndpoints();
        app.MapWebhookApiEndpoints();
        return app;
    }
}
