using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure.Email;

public static class EmailServiceExtensions
{
    /// <summary>
    /// Register IEmailSender based on configuration:
    ///   Email:Provider = "console"  → ConsoleEmailSender (default for dev)
    ///   Email:Provider = "smtp"     → SmtpEmailSender
    /// </summary>
    public static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Email:Provider"] ?? "console";

        if (provider.Equals("smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<SmtpEmailOptions>(configuration.GetSection("Email:Smtp"));
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            // Default: console (dev-friendly)
            services.AddScoped<IEmailSender, ConsoleEmailSender>();
        }

        return services;
    }
}
