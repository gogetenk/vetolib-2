using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Background service that runs every hour and deletes expired PendingUploads (older than 24h).
/// Removes both the stored file and the database record.
/// </summary>
internal class PendingUploadCleanupService : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingUploadCleanupService> _logger;

    public PendingUploadCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingUploadCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingUploadCleanupService started");

        using var timer = new PeriodicTimer(CleanupInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupExpiredUploadsAsync(stoppingToken);
        }
    }

    internal async Task CleanupExpiredUploadsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

            var now = DateTime.UtcNow;

            // IgnoreQueryFilters: cleanup is cross-clinic
            var expiredUploads = await context.PendingUploads
                .IgnoreQueryFilters()
                .Where(p => p.ExpiresAt < now)
                .ToListAsync(ct);

            if (expiredUploads.Count == 0)
                return;

            foreach (var upload in expiredUploads)
            {
                try
                {
                    await fileStorage.DeleteAsync(upload.StoragePath, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to delete file {StoragePath} for expired PendingUpload {UploadId}",
                        upload.StoragePath, upload.Id);
                    // Continue: remove DB record even if file deletion fails
                }

                context.PendingUploads.Remove(upload);
            }

            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Cleaned up {Count} expired pending uploads", expiredUploads.Count);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pending upload cleanup");
        }
    }
}
