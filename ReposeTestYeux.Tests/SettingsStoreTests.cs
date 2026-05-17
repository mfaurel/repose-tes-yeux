using ReposeTesYeux.Settings;

namespace ReposeTesYeux.Tests;

public class SettingsStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;
    private readonly SettingsStore _store;

    public SettingsStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ReposeTesYeuxTests_" + Guid.NewGuid());
        _testFile = Path.Combine(_testDir, "settings.json");
        Directory.CreateDirectory(_testDir);
        _store = new SettingsStore(_testFile);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Fact]
    public void Load_WhenNoFile_ReturnsDefaults()
    {
        var settings = _store.Load();
        Assert.Equal(20, settings.WorkIntervalMinutes);
        Assert.Equal(20, settings.BreakDurationSeconds);
        Assert.Equal(20, settings.DistanceMetres);
        Assert.Equal("fr-FR", settings.Language);
    }

    [Fact]
    public void Save_ThenLoad_RoundTrips()
    {
        var original = new AppSettings
        {
            WorkIntervalMinutes = 25,
            BreakDurationSeconds = 30,
            DistanceMetres = 6,
            Language = "en-GB",
            SoundEnabled = false,
            OverlayDismissible = false,
        };

        _store.Save(original);
        var loaded = _store.Load();

        Assert.Equal(25, loaded.WorkIntervalMinutes);
        Assert.Equal(30, loaded.BreakDurationSeconds);
        Assert.Equal(6, loaded.DistanceMetres);
        Assert.Equal("en-GB", loaded.Language);
        Assert.False(loaded.SoundEnabled);
        Assert.False(loaded.OverlayDismissible);
    }

    [Fact]
    public void Load_WithCorruptJson_ReturnsDefaults()
    {
        File.WriteAllText(_testFile, "{ this is not valid json !!!");
        var settings = _store.Load();
        Assert.Equal(20, settings.WorkIntervalMinutes);
    }

    [Fact]
    public void Load_ClampsOutOfRangeValues()
    {
        var tooLarge = new AppSettings { WorkIntervalMinutes = 999, BreakDurationSeconds = 999 };
        _store.Save(tooLarge);
        var loaded = _store.Load();
        Assert.Equal(120, loaded.WorkIntervalMinutes);
        Assert.Equal(300, loaded.BreakDurationSeconds);
    }

    [Fact]
    public void Load_ClampsMinValues()
    {
        var tooSmall = new AppSettings { WorkIntervalMinutes = 0, BreakDurationSeconds = 0, DistanceMetres = 0 };
        _store.Save(tooSmall);
        var loaded = _store.Load();
        Assert.Equal(1, loaded.WorkIntervalMinutes);
        Assert.Equal(5, loaded.BreakDurationSeconds);
        Assert.Equal(1, loaded.DistanceMetres);
    }

    [Fact]
    public void Load_EmptyLanguage_FallsBackToFrFR()
    {
        var s = new AppSettings { Language = "" };
        _store.Save(s);
        var loaded = _store.Load();
        Assert.Equal("fr-FR", loaded.Language);
    }

    [Fact]
    public void DoNotDisturbWindow_ParsedCorrectly()
    {
        var s = new AppSettings { DoNotDisturbStart = "12:00", DoNotDisturbEnd = "13:00" };
        Assert.True(s.HasDoNotDisturbWindow);
        var window = s.DoNotDisturbWindow;
        Assert.NotNull(window);
        Assert.Equal(new TimeOnly(12, 0), window!.Value.Start);
        Assert.Equal(new TimeOnly(13, 0), window!.Value.End);
    }

    [Fact]
    public void DoNotDisturbWindow_InvalidTime_ReturnsNull()
    {
        var s = new AppSettings { DoNotDisturbStart = "not-a-time", DoNotDisturbEnd = "13:00" };
        Assert.False(s.HasDoNotDisturbWindow);
        Assert.Null(s.DoNotDisturbWindow);
    }

}
