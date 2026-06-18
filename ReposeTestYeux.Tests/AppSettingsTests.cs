using ReposeTesYeux.Settings;

namespace ReposeTesYeux.Tests;

public class AppSettingsTests
{
    [Fact]
    public void WithDefaults_ClampsWorkIntervalAboveMax()
    {
        var s = new AppSettings { WorkIntervalMinutes = 999 }.WithDefaults();
        Assert.Equal(120, s.WorkIntervalMinutes);
    }

    [Fact]
    public void WithDefaults_ClampsWorkIntervalBelowMin()
    {
        var s = new AppSettings { WorkIntervalMinutes = 0 }.WithDefaults();
        Assert.Equal(1, s.WorkIntervalMinutes);
    }

    [Fact]
    public void WithDefaults_ClampsBreakDurationAboveMax()
    {
        var s = new AppSettings { BreakDurationSeconds = 9999 }.WithDefaults();
        Assert.Equal(300, s.BreakDurationSeconds);
    }

    [Fact]
    public void WithDefaults_ClampsBreakDurationBelowMin()
    {
        var s = new AppSettings { BreakDurationSeconds = 0 }.WithDefaults();
        Assert.Equal(5, s.BreakDurationSeconds);
    }

    [Fact]
    public void WithDefaults_ClampsDistanceBelowMin()
    {
        var s = new AppSettings { DistanceMetres = 0 }.WithDefaults();
        Assert.Equal(1, s.DistanceMetres);
    }

    [Fact]
    public void WithDefaults_EmptyLanguage_FallsBackToFrFR()
    {
        var s = new AppSettings { Language = "   " }.WithDefaults();
        Assert.Equal("fr-FR", s.Language);
    }

    [Fact]
    public void WithDefaults_ValidValues_Unchanged()
    {
        var s = new AppSettings { WorkIntervalMinutes = 25, BreakDurationSeconds = 30, DistanceMetres = 6, Language = "en-GB" }.WithDefaults();
        Assert.Equal(25, s.WorkIntervalMinutes);
        Assert.Equal(30, s.BreakDurationSeconds);
        Assert.Equal(6, s.DistanceMetres);
        Assert.Equal("en-GB", s.Language);
    }

    [Fact]
    public void DoNotDisturbWindow_StartEqualsEnd_HasWindow()
    {
        var s = new AppSettings { DoNotDisturbStart = "09:00", DoNotDisturbEnd = "09:00" };
        Assert.True(s.HasDoNotDisturbWindow);
    }

    [Fact]
    public void DoNotDisturbWindow_EmptyStrings_HasNoWindow()
    {
        var s = new AppSettings { DoNotDisturbStart = "", DoNotDisturbEnd = "" };
        Assert.False(s.HasDoNotDisturbWindow);
        Assert.Null(s.DoNotDisturbWindow);
    }
}
