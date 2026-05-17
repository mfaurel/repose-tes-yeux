using System.Text.Json.Serialization;

namespace ReposeTesYeux.Settings;

public class AppSettings
{
    public int WorkIntervalMinutes { get; set; } = 20;
    public int BreakDurationSeconds { get; set; } = 20;
    public int DistanceMetres { get; set; } = 20;
    public string OverlayMessage { get; set; } = "";
    public string Language { get; set; } = "fr-FR";
    public bool LaunchAtStartup { get; set; } = false;
    public bool OverlayDismissible { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;
    public string DoNotDisturbStart { get; set; } = "";
    public string DoNotDisturbEnd { get; set; } = "";

    [JsonIgnore]
    public bool HasDoNotDisturbWindow =>
        TimeOnly.TryParse(DoNotDisturbStart, out _) && TimeOnly.TryParse(DoNotDisturbEnd, out _);

    [JsonIgnore]
    public (TimeOnly Start, TimeOnly End)? DoNotDisturbWindow =>
        TimeOnly.TryParse(DoNotDisturbStart, out var s) && TimeOnly.TryParse(DoNotDisturbEnd, out var e)
            ? (s, e)
            : null;

    public AppSettings WithDefaults()
    {
        WorkIntervalMinutes = Math.Clamp(WorkIntervalMinutes, 1, 120);
        BreakDurationSeconds = Math.Clamp(BreakDurationSeconds, 5, 300);
        DistanceMetres = Math.Clamp(DistanceMetres, 1, 100);
        Language = string.IsNullOrWhiteSpace(Language) ? "fr-FR" : Language;
        return this;
    }
}
