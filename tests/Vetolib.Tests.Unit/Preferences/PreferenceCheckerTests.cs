using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Application.Services;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class PreferenceCheckerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = new("22222222-2222-2222-2222-222222222222");

    private readonly PreferencesDbContext _context;
    private readonly PreferenceChecker _checker;

    public PreferenceCheckerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);
        var publisher = Substitute.For<IPublisher>();
        var options = new DbContextOptionsBuilder<PreferencesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PreferencesDbContext(options, clinicContext, publisher);
        var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(
            new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
        _checker = new PreferenceChecker(_context, clinicContext, cache);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetValueAsync_NoUserOrClinicPref_ReturnsSystemDefault()
    {
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public async Task GetValueAsync_NoUserOrClinicPref_AnalyticsPosthog_ReturnsFalse()
    {
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.AnalyticsPosthog);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public async Task GetValueAsync_ClinicPrefExists_ReturnsClinicValue()
    {
        var clinicPref = ClinicPreferenceDefault.Create(ClinicId, PreferenceKey.NotificationEmail, "false").Value;
        _context.ClinicPreferenceDefaults.Add(clinicPref);
        await _context.SaveChangesAsync();
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }

    [Fact]
    public async Task GetValueAsync_UserPrefExists_ReturnsUserValue()
    {
        var clinicPref = ClinicPreferenceDefault.Create(ClinicId, PreferenceKey.NotificationEmail, "false").Value;
        _context.ClinicPreferenceDefaults.Add(clinicPref);
        var userPref = UserPreference.Create(ClinicId, UserId, PreferenceKey.NotificationEmail, "true").Value;
        _context.UserPreferences.Add(userPref);
        await _context.SaveChangesAsync();
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public async Task GetValueAsync_UserPrefExistsForOtherUser_ReturnsSystemDefault()
    {
        var otherUserId = Guid.NewGuid();
        var userPref = UserPreference.Create(ClinicId, otherUserId, PreferenceKey.NotificationEmail, "false").Value;
        _context.UserPreferences.Add(userPref);
        await _context.SaveChangesAsync();
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public async Task IsTrueAsync_BooleanPreference_ReturnsParsedBool()
    {
        var result = await _checker.IsTrueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsTrueAsync_AnalyticsPosthog_ReturnsFalse()
    {
        var result = await _checker.IsTrueAsync(UserId, PreferenceKey.AnalyticsPosthog);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsTrueAsync_NonBoolValue_ReturnsError()
    {
        var result = await _checker.IsTrueAsync(UserId, PreferenceKey.CommunicationLanguage);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not a valid boolean"));
    }

    [Fact]
    public async Task IsTrueAsync_AIDrugInteractions_IsAlwaysTrue()
    {
        var result = await _checker.IsTrueAsync(UserId, PreferenceKey.AIDrugInteractions);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetValueAsync_SecondCall_ReturnsCachedValue()
    {
        // First call populates cache
        var first = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        first.IsSuccess.Should().BeTrue();

        // Second call should hit cache (same result)
        var second = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        second.IsSuccess.Should().BeTrue();
        second.Value.Should().Be(first.Value);
    }

    [Fact]
    public async Task Invalidate_RemovesCachedValue_NextCallFetchesFresh()
    {
        // Populate cache
        await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);

        // Invalidate the cached entry
        _checker.Invalidate(ClinicId, UserId, PreferenceKey.NotificationEmail);

        // Next call should fetch fresh from DB/system defaults
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public void BuildCacheKey_ProducesConsistentKey()
    {
        var clinicId = new Guid("11111111-1111-1111-1111-111111111111");
        var userId = new Guid("22222222-2222-2222-2222-222222222222");
        var key = PreferenceChecker.BuildCacheKey(clinicId, userId, PreferenceKey.NotificationEmail);

        key.Should().Be($"pref:{clinicId}:{userId}:{PreferenceKey.NotificationEmail}");
        key.Should().Contain("NotificationEmail");
    }

    [Fact]
    public void BuildCacheKey_DifferentKeys_ProduceDifferentCacheKeys()
    {
        var key1 = PreferenceChecker.BuildCacheKey(ClinicId, UserId, PreferenceKey.NotificationEmail);
        var key2 = PreferenceChecker.BuildCacheKey(ClinicId, UserId, PreferenceKey.NotificationPush);

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildCacheKey_DifferentUsers_ProduceDifferentCacheKeys()
    {
        var otherUserId = new Guid("33333333-3333-3333-3333-333333333333");
        var key1 = PreferenceChecker.BuildCacheKey(ClinicId, UserId, PreferenceKey.NotificationEmail);
        var key2 = PreferenceChecker.BuildCacheKey(ClinicId, otherUserId, PreferenceKey.NotificationEmail);

        key1.Should().NotBe(key2);
    }

    [Fact]
    public async Task GetValueAsync_ClinicPrefForDifferentKey_ReturnsSystemDefaultForRequestedKey()
    {
        // Clinic has a pref for NotificationEmail (set to "true"), but we query NotificationPush
        // whose system default is "false" — values differ so cross-key leakage would be detected
        var clinicPref = ClinicPreferenceDefault.Create(ClinicId, PreferenceKey.NotificationEmail, "true").Value;
        _context.ClinicPreferenceDefaults.Add(clinicPref);
        await _context.SaveChangesAsync();

        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationPush);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false"); // system default for NotificationPush
    }

    [Fact]
    public async Task GetValueAsync_UserPrefOverridesClinicPref_ForSameKey()
    {
        // Clinic says "false", user says "true" — user wins (and "true" differs from system default "false")
        var clinicPref = ClinicPreferenceDefault.Create(ClinicId, PreferenceKey.AnalyticsPosthog, "false").Value;
        _context.ClinicPreferenceDefaults.Add(clinicPref);
        var userPref = UserPreference.Create(ClinicId, UserId, PreferenceKey.AnalyticsPosthog, "true").Value;
        _context.UserPreferences.Add(userPref);
        await _context.SaveChangesAsync();

        var result = await _checker.GetValueAsync(UserId, PreferenceKey.AnalyticsPosthog);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("true");
    }

    [Fact]
    public async Task GetValueAsync_BookingKey_ReturnsSystemDefault()
    {
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.BookingMaxAdvanceDays);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("28");
    }

    [Fact]
    public async Task IsTrueAsync_WithUserPrefSetToFalse_ReturnsFalse()
    {
        var userPref = UserPreference.Create(ClinicId, UserId, PreferenceKey.NotificationEmail, "false").Value;
        _context.UserPreferences.Add(userPref);
        await _context.SaveChangesAsync();

        var result = await _checker.IsTrueAsync(UserId, PreferenceKey.NotificationEmail);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Invalidate_AfterClinicPrefAdded_ReturnsFreshValue()
    {
        // Populate cache with system default
        await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);

        // Add clinic pref that overrides the default
        var clinicPref = ClinicPreferenceDefault.Create(ClinicId, PreferenceKey.NotificationEmail, "false").Value;
        _context.ClinicPreferenceDefaults.Add(clinicPref);
        await _context.SaveChangesAsync();

        // Invalidate cache
        _checker.Invalidate(ClinicId, UserId, PreferenceKey.NotificationEmail);

        // Should now return the clinic pref value
        var result = await _checker.GetValueAsync(UserId, PreferenceKey.NotificationEmail);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("false");
    }
}
