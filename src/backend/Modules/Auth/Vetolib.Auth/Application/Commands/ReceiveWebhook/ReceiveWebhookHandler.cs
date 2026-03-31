using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.ReceiveWebhook;

internal class ReceiveWebhookHandler : IRequestHandler<ReceiveWebhookCommand, Result>
{
    private readonly AuthDbContext _context;
    private readonly ILogger<ReceiveWebhookHandler> _logger;

    public ReceiveWebhookHandler(AuthDbContext context, ILogger<ReceiveWebhookHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ReceiveWebhookCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Signature))
            return Result.Unauthorized();

        if (string.IsNullOrWhiteSpace(cmd.EventType))
            return Result.Invalid(new ValidationError(nameof(cmd.EventType), "EventType is required"));

        // Find all active registrations that support this event type (across all clinics)
        var registrations = await _context.WebhookRegistrations
            .IgnoreQueryFilters()
            .Where(w => w.IsActive)
            .ToListAsync(ct);

        // Filter by event type support in memory (since EventTypes is a text array)
        var matching = registrations
            .Where(w => w.SupportsEventType(cmd.EventType))
            .ToList();

        if (matching.Count == 0)
        {
            _logger.LogWarning("No active webhook registrations found for event type {EventType}", cmd.EventType);
            return Result.NotFound("No matching webhook registration found");
        }

        // Try to verify signature against each matching registration
        WebhookRegistration? verified = null;
        foreach (var registration in matching)
        {
            if (VerifyHmacSignature(cmd.RawBody, cmd.Signature, registration.Secret))
            {
                verified = registration;
                break;
            }
        }

        if (verified is null)
        {
            _logger.LogWarning("HMAC signature verification failed for event type {EventType}", cmd.EventType);
            return Result.Unauthorized();
        }

        // Log the webhook
        var log = WebhookLog.Create(
            verified.ClinicId,
            verified.Id,
            cmd.EventType,
            cmd.RawBody);

        _context.WebhookLogs.Add(log);
        verified.RecordCall();

        try
        {
            // Mark as processed (actual event routing would happen via domain events / MediatR notifications)
            log.MarkProcessed();
            _logger.LogInformation(
                "Webhook received and processed: RegistrationId={RegistrationId}, EventType={EventType}, LogId={LogId}",
                verified.Id, cmd.EventType, log.Id);
        }
        catch (Exception ex)
        {
            log.MarkFailed();
            _logger.LogError(ex,
                "Webhook processing failed: RegistrationId={RegistrationId}, EventType={EventType}, LogId={LogId}",
                verified.Id, cmd.EventType, log.Id);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    internal static bool VerifyHmacSignature(string body, string signature, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        using var hmac = new HMACSHA256(keyBytes);
        var computedHash = hmac.ComputeHash(bodyBytes);
        var computedSignature = Convert.ToHexStringLower(computedHash);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signature));
    }
}
