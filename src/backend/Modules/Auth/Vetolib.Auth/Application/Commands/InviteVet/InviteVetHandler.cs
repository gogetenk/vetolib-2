using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Commands.InviteVet;

internal class InviteVetHandler : IRequestHandler<InviteVetCommand, Result>
{
    private readonly AuthDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InviteVetHandler> _logger;

    /// <summary>
    /// Maximum number of invitations that can be sent to a single vet email per day.
    /// </summary>
    internal const int MaxInvitationsPerEmailPerDay = 3;

    public InviteVetHandler(
        AuthDbContext context,
        IEmailSender emailSender,
        ILogger<InviteVetHandler> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result> Handle(InviteVetCommand cmd, CancellationToken ct)
    {
        var normalizedEmail = cmd.VetEmail.Trim().ToLowerInvariant();

        // Rate limit: max 3 invitations per email per day
        var today = DateTime.UtcNow.Date;
        var invitationsToday = await _context.VetInvitationLogs
            .IgnoreQueryFilters() // Not tenant-scoped
            .CountAsync(v => v.VetEmail == normalizedEmail && v.SentAt >= today, ct);

        if (invitationsToday >= MaxInvitationsPerEmailPerDay)
            return Result.Conflict("RATE_LIMIT:This vet has already received the maximum number of invitations today. Please try again tomorrow.");

        // Create the invitation log
        var createResult = VetInvitationLog.Create(cmd.VetEmail, cmd.OwnerName, cmd.PetName, cmd.Message);
        if (!createResult.IsSuccess)
            return Result.Invalid(createResult.ValidationErrors.ToList());

        var invitation = createResult.Value;

        // Send the email
        var emailResult = await _emailSender.SendAsync(BuildEmail(normalizedEmail, cmd.OwnerName, cmd.PetName, cmd.Message), ct);
        if (!emailResult.IsSuccess)
        {
            _logger.LogWarning("Failed to send vet invitation email to {VetEmail}: {Errors}",
                normalizedEmail, string.Join(", ", emailResult.Errors));
            return Result.Error("Failed to send invitation email. Please try again later.");
        }

        // Persist the log after successful send
        _context.VetInvitationLogs.Add(invitation);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Vet invitation sent to {VetEmail} from owner {OwnerName} for pet {PetName}",
            normalizedEmail, cmd.OwnerName, cmd.PetName);

        return Result.Success();
    }

    internal static EmailMessage BuildEmail(string vetEmail, string ownerName, string petName, string? message)
    {
        var customMessage = string.IsNullOrWhiteSpace(message)
            ? ""
            : $"\n\nPersonal message from {ownerName}:\n\"{message}\"";

        var plainText = $"""
            Hi,

            {ownerName} uses Vetara to manage {petName}'s health records and wants you to join.

            Vetara is a modern veterinary platform with AI-powered health alerts, online booking, and shared medical records.{customMessage}

            Start your free trial: https://app.vetara.ae/register

            Best regards,
            The Vetara Team
            """;

        var htmlBody = $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;">
                <div style="background: linear-gradient(135deg, #0ea5e9, #6366f1); padding: 32px; border-radius: 12px 12px 0 0;">
                    <h1 style="color: white; margin: 0; font-size: 24px;">Vetara</h1>
                    <p style="color: rgba(255,255,255,0.9); margin: 8px 0 0;">Modern Veterinary Platform</p>
                </div>
                <div style="padding: 32px; background: #f9fafb; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 12px 12px;">
                    <p style="font-size: 16px;">Hi,</p>
                    <p style="font-size: 16px;"><strong>{System.Net.WebUtility.HtmlEncode(ownerName)}</strong> uses Vetara to manage <strong>{System.Net.WebUtility.HtmlEncode(petName)}</strong>'s health records and wants you to join.</p>
                    <p style="font-size: 16px;">Vetara is a modern veterinary platform with:</p>
                    <ul style="font-size: 16px;">
                        <li>AI-powered health alerts</li>
                        <li>Online booking</li>
                        <li>Shared medical records</li>
                    </ul>
                    {(string.IsNullOrWhiteSpace(message) ? "" : $"<div style=\"background: #e0f2fe; padding: 16px; border-radius: 8px; margin: 16px 0;\"><p style=\"margin: 0; font-style: italic; color: #0369a1;\">\"{System.Net.WebUtility.HtmlEncode(message)}\"</p><p style=\"margin: 8px 0 0; font-size: 14px; color: #64748b;\">— {System.Net.WebUtility.HtmlEncode(ownerName)}</p></div>")}
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="https://app.vetara.ae/register" style="background: #6366f1; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 16px;">Start Your Free Trial</a>
                    </div>
                    <p style="font-size: 14px; color: #64748b;">Best regards,<br>The Vetara Team</p>
                </div>
            </body>
            </html>
            """;

        return new EmailMessage(
            To: vetEmail,
            Subject: $"{ownerName} wants you to join Vetara — modern veterinary platform",
            HtmlBody: htmlBody,
            PlainTextBody: plainText);
    }
}
