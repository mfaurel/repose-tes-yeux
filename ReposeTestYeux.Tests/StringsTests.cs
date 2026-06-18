using ReposeTesYeux.I18n;

namespace ReposeTesYeux.Tests;

public class StringsTests : IDisposable
{
    public void Dispose()
    {
        // Reset to default language after each test
        Strings.SetLanguage("fr-FR");
    }

    [Fact]
    public void Get_DefaultLanguage_ReturnsFrenchString()
    {
        Strings.SetLanguage("fr-FR");
        Assert.Equal("Repose Tes Yeux", Strings.Get("app_name"));
    }

    [Fact]
    public void SetLanguage_EnGB_ReturnsEnglishString()
    {
        Strings.SetLanguage("en-GB");
        Assert.Equal("Rest Your Eyes", Strings.Get("app_name"));
    }

    [Fact]
    public void SetLanguage_UnknownLocale_FallsBackToFrench()
    {
        Strings.SetLanguage("zh-CN");
        Assert.Equal("Repose Tes Yeux", Strings.Get("app_name"));
    }

    [Fact]
    public void Get_UnknownKey_ReturnsKeyItself()
    {
        var result = Strings.Get("this_key_does_not_exist");
        Assert.Equal("this_key_does_not_exist", result);
    }

    [Fact]
    public void Get_AllKnownKeys_ReturnsNonEmpty()
    {
        var keys = new[]
        {
            "app_name", "tray_tooltip_working", "tray_tooltip_break", "tray_tooltip_paused",
            "menu_pause", "menu_resume", "menu_break_now", "menu_settings", "menu_stats", "menu_quit",
            "overlay_default_message", "overlay_instruction", "overlay_rule", "overlay_skip", "overlay_countdown",
            "settings_title", "settings_work_interval", "settings_break_duration", "settings_distance",
            "settings_message", "settings_language", "settings_startup", "settings_dismissible",
            "settings_sound", "settings_dnd_start", "settings_dnd_end", "settings_save", "settings_cancel",
            "stats_title", "stats_breaks_today", "stats_close",
        };

        foreach (var lang in new[] { "fr-FR", "en-GB" })
        {
            Strings.SetLanguage(lang);
            foreach (var key in keys)
                Assert.False(string.IsNullOrEmpty(Strings.Get(key)), $"Key '{key}' is empty for locale '{lang}'");
        }
    }
}
