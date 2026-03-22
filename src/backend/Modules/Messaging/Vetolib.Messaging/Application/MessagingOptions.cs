using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application;

internal class MessagingOptions
{
    public const string SectionName = "Messaging";

    /// <summary>
    /// Maximum messages an owner can send per day per clinic (default: 5).
    /// </summary>
    public int DailyMessageLimit { get; set; } = 5;

    /// <summary>
    /// Hours before a pending upload expires (default: 24).
    /// </summary>
    public int PendingUploadExpiryHours { get; set; } = 24;

    /// <summary>
    /// Days before a portal token expires (default: 90).
    /// </summary>
    public int PortalTokenExpiryDays { get; set; } = 90;

    /// <summary>
    /// AI classification confidence below this threshold flags the message for manual review (default: 0.6).
    /// </summary>
    public double ClassificationReviewThreshold { get; set; } = 0.6;

    /// <summary>
    /// Triage confidence below this threshold marks the conversation as uncertain (default: 0.7).
    /// </summary>
    public double TriageUncertaintyThreshold { get; set; } = 0.7;

    /// <summary>
    /// Minutes before an unviewed MedicalUrgency conversation triggers emergency escalation (default: 10).
    /// </summary>
    public int EmergencyEscalationMinutes { get; set; } = 10;

    /// <summary>
    /// Interval in minutes between emergency escalation scans (default: 1).
    /// </summary>
    public int EscalationScanIntervalMinutes { get; set; } = 1;

    /// <summary>
    /// Maximum file size in bytes for uploads (default: 10 MB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Maximum files per upload request (default: 5).
    /// </summary>
    public int MaxFilesPerUpload { get; set; } = 5;

    /// <summary>
    /// Maximum attachments per message (default: 3).
    /// </summary>
    public int MaxAttachmentsPerMessage { get; set; } = 3;

    /// <summary>
    /// Maximum message body length in characters (default: 2000).
    /// </summary>
    public int MaxMessageBodyLength { get; set; } = 2000;

    /// <summary>
    /// Number of days for triage stats lookback window (default: 30).
    /// </summary>
    public int TriageStatsWindowDays { get; set; } = 30;

    /// <summary>
    /// SLA estimates per message category (business hours).
    /// </summary>
    public Dictionary<string, string> SlaEstimates { get; set; } = new()
    {
        ["MedicalUrgency"] = "15 minutes",
        ["PostOperativeFollowUp"] = "2 hours",
        ["MedicalQuestion"] = "8 hours",
        ["AppointmentRequest"] = "4 hours",
        ["Administrative"] = "24 hours",
        ["Feedback"] = "48 hours",
        ["Other"] = "24 hours"
    };
}
