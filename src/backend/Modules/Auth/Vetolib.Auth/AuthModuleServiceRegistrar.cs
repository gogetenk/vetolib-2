using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Vetolib.Auth.Api;
using Vetolib.Auth.Application.Services;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth;

public static class AuthModuleServiceRegistrar
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration config)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AuthModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(AuthModuleServiceRegistrar).Assembly);

        // JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // JWT Authentication
        var jwtKey = config["Jwt:Key"] ?? "super-secret-key-for-vetolib-jwt-token-generation-minimum-32-chars";
        var jwtIssuer = config["Jwt:Issuer"] ?? "Vetolib";
        var jwtAudience = config["Jwt:Audience"] ?? "Vetolib";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
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

        services.AddAuthorization(options =>
        {
            // ClinicStaff: all authenticated clinic staff (Vet, Receptionist, Admin).
            // Explicitly excludes Assistant role — used for AI endpoints that must not
            // be visible to the animal owner / external party mapped to Assistant.
            options.AddPolicy("ClinicStaff", policy =>
                policy.RequireRole(
                    nameof(UserRole.Vet),
                    nameof(UserRole.Receptionist),
                    nameof(UserRole.Admin)));

            // VetOrAdmin: restricted to clinical decision-makers only.
            options.AddPolicy("VetOrAdmin", policy =>
                policy.RequireRole(
                    nameof(UserRole.Vet),
                    nameof(UserRole.Admin)));
        });

        return services;
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
        return app;
    }
}
