using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using MediatR;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class PendingUploadCleanupServiceTests : IDisposable
{
    private static readonly Guid TestClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly PendingUploadCleanupService _sut;
    private readonly ServiceProvider _serviceProvider;

    public PendingUploadCleanupServiceTests()
    {
        _fileStorage = Substitute.For<IFileStorage>();

        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(TestClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase($"cleanup-test-{Guid.NewGuid():N}")
            .Options;

        _context = new MessagingDbContext(options, clinicContext, publisher);

        var services = new ServiceCollection();
        services.AddScoped(_ => new MessagingDbContext(options, clinicContext, publisher));
        services.AddScoped(_ => _fileStorage);
        _serviceProvider = services.BuildServiceProvider();

        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<PendingUploadCleanupService>>();

        _sut = new PendingUploadCleanupService(scopeFactory, logger);
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task CleanupExpiredUploadsAsync_RemovesExpiredUploads()
    {
        // Arrange: create an expired pending upload
        var expiredUpload = PendingUpload.Create(
            TestClinicId, "old.pdf", "application/pdf", 1024, "uploads/old.pdf");
        expiredUpload.IsSuccess.Should().BeTrue();

        // Force the ExpiresAt to the past
        var entity = expiredUpload.Value;
        typeof(PendingUpload)
            .GetProperty(nameof(PendingUpload.ExpiresAt))!
            .SetValue(entity, DateTime.UtcNow.AddHours(-1));

        _context.PendingUploads.Add(entity);
        await _context.SaveChangesAsync();

        _context.PendingUploads.IgnoreQueryFilters().Count().Should().Be(1);

        // Act
        await _sut.CleanupExpiredUploadsAsync(CancellationToken.None);

        // Assert
        // Need a fresh context to verify since cleanup uses its own scope
        using var scope = _serviceProvider.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        var remaining = await freshContext.PendingUploads.IgnoreQueryFilters().CountAsync();
        remaining.Should().Be(0);

        await _fileStorage.Received(1).DeleteAsync("uploads/old.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CleanupExpiredUploadsAsync_KeepsNonExpiredUploads()
    {
        // Arrange: create a non-expired pending upload (default ExpiresAt is 24h from now)
        var validUpload = PendingUpload.Create(
            TestClinicId, "fresh.png", "image/png", 2048, "uploads/fresh.png");
        validUpload.IsSuccess.Should().BeTrue();

        _context.PendingUploads.Add(validUpload.Value);
        await _context.SaveChangesAsync();

        // Act
        await _sut.CleanupExpiredUploadsAsync(CancellationToken.None);

        // Assert
        using var scope = _serviceProvider.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        var remaining = await freshContext.PendingUploads.IgnoreQueryFilters().CountAsync();
        remaining.Should().Be(1);

        await _fileStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CleanupExpiredUploadsAsync_FileDeleteFails_StillRemovesDbRecord()
    {
        // Arrange
        var expiredUpload = PendingUpload.Create(
            TestClinicId, "broken.pdf", "application/pdf", 512, "uploads/broken.pdf");
        expiredUpload.IsSuccess.Should().BeTrue();

        var entity = expiredUpload.Value;
        typeof(PendingUpload)
            .GetProperty(nameof(PendingUpload.ExpiresAt))!
            .SetValue(entity, DateTime.UtcNow.AddHours(-2));

        _context.PendingUploads.Add(entity);
        await _context.SaveChangesAsync();

        _fileStorage.DeleteAsync("uploads/broken.pdf", Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new IOException("Storage unavailable")));

        // Act
        await _sut.CleanupExpiredUploadsAsync(CancellationToken.None);

        // Assert: DB record should still be removed despite file delete failure
        using var scope = _serviceProvider.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        var remaining = await freshContext.PendingUploads.IgnoreQueryFilters().CountAsync();
        remaining.Should().Be(0);
    }
}
