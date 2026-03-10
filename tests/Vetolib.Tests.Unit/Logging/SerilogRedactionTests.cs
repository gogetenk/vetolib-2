using System.IO;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Xunit;

namespace Vetolib.Tests.Unit.Logging;

/// <summary>
/// Verifies that the Serilog sensitive-data enricher masks PII and secrets
/// before they reach any log sink (C-01 security requirement).
/// </summary>
public class SerilogRedactionTests
{
    [Fact]
    public void Serilog_redacts_password_from_structured_log()
    {
        var output = new StringWriter();
        var logger = new LoggerConfiguration()
            .Enrich.WithSensitiveDataMasking(options =>
            {
                options.MaskProperties.Add("Password");
                options.MaskingOperators.Add(new EmailAddressMaskingOperator());
            })
            .WriteTo.TextWriter(output)
            .CreateLogger();

        logger.Information("Login attempt with {Password} for {Email}",
            "S3cret!Pass", "admin@desertpaws.ae");

        var logOutput = output.ToString();
        Assert.DoesNotContain("S3cret!Pass", logOutput);
        Assert.DoesNotContain("admin@desertpaws.ae", logOutput);
        Assert.Contains("***", logOutput);
    }

    [Fact]
    public void Serilog_redacts_token_properties()
    {
        var output = new StringWriter();
        var logger = new LoggerConfiguration()
            .Enrich.WithSensitiveDataMasking(options =>
            {
                options.MaskProperties.Add("Token");
                options.MaskProperties.Add("RefreshToken");
                options.MaskProperties.Add("AccessToken");
                options.MaskProperties.Add("Secret");
                options.MaskProperties.Add("To");
            })
            .WriteTo.TextWriter(output)
            .CreateLogger();

        logger.Information(
            "Token={Token} Refresh={RefreshToken} Access={AccessToken} Secret={Secret} To={To}",
            "jwt.header.payload", "refresh-abc", "access-xyz", "supersecret", "user@example.ae");

        var logOutput = output.ToString();
        Assert.DoesNotContain("jwt.header.payload", logOutput);
        Assert.DoesNotContain("refresh-abc", logOutput);
        Assert.DoesNotContain("access-xyz", logOutput);
        Assert.DoesNotContain("supersecret", logOutput);
        Assert.DoesNotContain("user@example.ae", logOutput);
    }
}
