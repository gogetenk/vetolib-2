using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.Extensions.Configuration;

namespace Vetolib.Agenda.Application.Services;

internal class CheckInHmacService
{
    private readonly byte[] _keyBytes;

    /// <summary>
    /// Time window (in minutes) before and after the scheduled time during which check-in is allowed.
    /// </summary>
    internal const int CheckInWindowMinutes = 30;

    public CheckInHmacService(IConfiguration configuration)
    {
        var key = configuration["CheckIn:HmacKey"]
                  ?? configuration["Jwt:Key"]
                  ?? throw new InvalidOperationException(
                      "CheckIn:HmacKey or Jwt:Key must be configured for QR check-in.");
        _keyBytes = Encoding.UTF8.GetBytes(key);
    }

    /// <summary>
    /// Computes an HMAC-SHA256 signature for the given payload fields.
    /// </summary>
    internal string ComputeSignature(
        Guid appointmentId,
        string patientName,
        string ownerName,
        DateTime scheduledTime,
        Guid clinicId)
    {
        var data = BuildSignatureInput(appointmentId, patientName, ownerName, scheduledTime, clinicId);
        using var hmac = new HMACSHA256(_keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verifies the HMAC signature and checks the check-in time window.
    /// </summary>
    internal Result VerifyAndValidateTimeWindow(
        Guid appointmentId,
        string patientName,
        string ownerName,
        DateTime scheduledTime,
        Guid clinicId,
        string signature,
        DateTime utcNow)
    {
        var expectedSignature = ComputeSignature(appointmentId, patientName, ownerName, scheduledTime, clinicId);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature)))
        {
            return Result.Error("INVALID_SIGNATURE:QR code signature is invalid or has been tampered with.");
        }

        var diff = Math.Abs((utcNow - scheduledTime).TotalMinutes);
        if (diff > CheckInWindowMinutes)
        {
            return Result.Error(
                $"CHECKIN_WINDOW_EXPIRED:Check-in is only allowed within {CheckInWindowMinutes} minutes of the scheduled time.");
        }

        return Result.Success();
    }

    private static string BuildSignatureInput(
        Guid appointmentId,
        string patientName,
        string ownerName,
        DateTime scheduledTime,
        Guid clinicId)
    {
        return $"{appointmentId}|{patientName}|{ownerName}|{scheduledTime:O}|{clinicId}";
    }
}
