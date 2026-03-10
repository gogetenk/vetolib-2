using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure;

/// <summary>
/// Extension methods to register shared infrastructure services (audit trail).
///
/// Typical usage in Program.cs:
/// <code>
///   builder.Services.AddSharedInfrastructure();
///   builder.Services.AddAuditInterceptor&lt;AuthDbContext&gt;();
///   builder.Services.AddAuditInterceptor&lt;AgendaDbContext&gt;();
///   // ... repeat for each module DbContext
/// </code>
/// </summary>
public static class SharedInfrastructureExtensions
{
    /// <summary>
    /// Registers the audit interceptor singleton and the IUserContext scoped service.
    /// Must be called before AddAuditInterceptor&lt;T&gt;.
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<AuditSaveChangesInterceptor>();
        services.AddScoped<IUserContext, UserContext>();
        return services;
    }

    /// <summary>
    /// Wires the audit interceptor into a specific DbContext via
    /// IDbContextOptionsConfiguration&lt;T&gt; (EF Core 8+ mechanism).
    /// Call once per DbContext type after calling AddSharedInfrastructure().
    /// </summary>
    public static IServiceCollection AddAuditInterceptor<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddSingleton<IDbContextOptionsConfiguration<TContext>>(sp =>
            new AuditInterceptorOptionsConfiguration<TContext>(
                sp.GetRequiredService<AuditSaveChangesInterceptor>()));
        return services;
    }

    private sealed class AuditInterceptorOptionsConfiguration<TContext>
        : IDbContextOptionsConfiguration<TContext>
        where TContext : DbContext
    {
        private readonly AuditSaveChangesInterceptor _interceptor;

        public AuditInterceptorOptionsConfiguration(AuditSaveChangesInterceptor interceptor)
        {
            _interceptor = interceptor;
        }

        public void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_interceptor);
        }
    }
}
