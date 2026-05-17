namespace ReposeTesYeux.I18n;

public static class Strings
{
    private static readonly Dictionary<string, Dictionary<string, string>> Locales = new()
    {
        ["fr-FR"] = new()
        {
            ["app_name"] = "Repose Tes Yeux",
            ["tray_tooltip_working"] = "Prochaine pause dans {0}",
            ["tray_tooltip_break"] = "Pause en cours — {0} restantes",
            ["tray_tooltip_paused"] = "En pause",
            ["menu_pause"] = "Mettre en pause",
            ["menu_resume"] = "Reprendre",
            ["menu_break_now"] = "Pause maintenant",
            ["menu_settings"] = "Paramètres…",
            ["menu_stats"] = "Statistiques",
            ["menu_quit"] = "Quitter",
            ["overlay_default_message"] = "Repose tes yeux !",
            ["overlay_instruction"] = "Regarde quelque chose à au moins {0} m pendant {1} secondes.",
            ["overlay_rule"] = "Règle 20/20/20",
            ["overlay_skip"] = "Passer",
            ["overlay_countdown"] = "{0} s",
            ["settings_title"] = "Paramètres — Repose Tes Yeux",
            ["settings_work_interval"] = "Intervalle de travail (minutes)",
            ["settings_break_duration"] = "Durée de la pause (secondes)",
            ["settings_distance"] = "Distance recommandée (mètres)",
            ["settings_message"] = "Message de la pause",
            ["settings_language"] = "Langue",
            ["settings_startup"] = "Lancer au démarrage de Windows",
            ["settings_dismissible"] = "Autoriser la fermeture manuelle de la pause",
            ["settings_sound"] = "Sons activés",
            ["settings_dnd_start"] = "Ne pas déranger — début (HH:mm)",
            ["settings_dnd_end"] = "Ne pas déranger — fin (HH:mm)",
            ["settings_save"] = "Enregistrer",
            ["settings_cancel"] = "Annuler",
            ["stats_title"] = "Statistiques",
            ["stats_breaks_today"] = "Pauses aujourd'hui : {0}",
            ["stats_close"] = "Fermer",
        },
        ["en-GB"] = new()
        {
            ["app_name"] = "Rest Your Eyes",
            ["tray_tooltip_working"] = "Next break in {0}",
            ["tray_tooltip_break"] = "Break in progress — {0} left",
            ["tray_tooltip_paused"] = "Paused",
            ["menu_pause"] = "Pause",
            ["menu_resume"] = "Resume",
            ["menu_break_now"] = "Break now",
            ["menu_settings"] = "Settings…",
            ["menu_stats"] = "Statistics",
            ["menu_quit"] = "Quit",
            ["overlay_default_message"] = "Rest your eyes!",
            ["overlay_instruction"] = "Look at something at least {0} m away for {1} seconds.",
            ["overlay_rule"] = "20/20/20 Rule",
            ["overlay_skip"] = "Skip",
            ["overlay_countdown"] = "{0} s",
            ["settings_title"] = "Settings — Rest Your Eyes",
            ["settings_work_interval"] = "Work interval (minutes)",
            ["settings_break_duration"] = "Break duration (seconds)",
            ["settings_distance"] = "Recommended distance (metres)",
            ["settings_message"] = "Break message",
            ["settings_language"] = "Language",
            ["settings_startup"] = "Launch at Windows startup",
            ["settings_dismissible"] = "Allow manual dismissal of break",
            ["settings_sound"] = "Sounds enabled",
            ["settings_dnd_start"] = "Do not disturb — start (HH:mm)",
            ["settings_dnd_end"] = "Do not disturb — end (HH:mm)",
            ["settings_save"] = "Save",
            ["settings_cancel"] = "Cancel",
            ["stats_title"] = "Statistics",
            ["stats_breaks_today"] = "Breaks today: {0}",
            ["stats_close"] = "Close",
        }
    };

    private static string _language = "fr-FR";

    public static void SetLanguage(string language)
    {
        _language = Locales.ContainsKey(language) ? language : "fr-FR";
    }

    public static string Get(string key)
    {
        if (Locales.TryGetValue(_language, out var locale) && locale.TryGetValue(key, out var value))
            return value;
        if (Locales["fr-FR"].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }
}
