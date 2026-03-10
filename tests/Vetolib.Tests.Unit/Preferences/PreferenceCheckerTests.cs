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
        _checker = new PreferenceChecker(_context);
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
}
