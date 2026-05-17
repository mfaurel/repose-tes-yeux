const LOCALES = {
  'fr-FR': {
    app_name:               'Repose Tes Yeux',
    tray_tooltip_working:   'Prochaine pause dans {0}',
    tray_tooltip_break:     'Pause en cours — {0} restantes',
    tray_tooltip_paused:    'En pause — {0} restants',
    tray_pause:             'Mettre en pause',
    tray_resume:            'Reprendre',
    tray_break_now:         'Pause maintenant',
    tray_settings:          'Paramètres…',
    tray_stats:             'Statistiques',
    tray_quit:              'Quitter',
    overlay_title:          'Repose tes yeux',
    overlay_rule:           'Regarde quelque chose à {0} mètres',
    overlay_default_msg:    'Laisse tes yeux se reposer',
    overlay_skip:           'Passer',
    stats_title:            'Statistiques',
    stats_breaks_today:     'Pauses aujourd\'hui : {0}',
    settings_title:         'Paramètres',
    settings_timer_section: 'Minuterie',
    settings_overlay_section: 'Overlay',
    settings_system_section:  'Système',
    settings_interval:      'Intervalle de travail (minutes)',
    settings_duration:      'Durée de la pause (secondes)',
    settings_distance:      'Distance de regard (mètres)',
    settings_message:       'Message personnalisé (vide = message par défaut)',
    settings_language:      'Langue',
    settings_startup:       'Lancer au démarrage de Windows',
    settings_dismissible:   'Afficher le bouton « Passer »',
    settings_sound:         'Sons activés',
    settings_dnd_start:     'Ne pas déranger — début (HH:MM)',
    settings_dnd_end:       'Ne pas déranger — fin (HH:MM)',
    settings_save:          'Enregistrer',
    settings_cancel:        'Annuler',
    lang_fr:                'Français',
    lang_en:                'English',
  },
  'en-GB': {
    app_name:               'Rest Your Eyes',
    tray_tooltip_working:   'Next break in {0}',
    tray_tooltip_break:     'Break in progress — {0} remaining',
    tray_tooltip_paused:    'Paused — {0} remaining',
    tray_pause:             'Pause',
    tray_resume:            'Resume',
    tray_break_now:         'Break now',
    tray_settings:          'Settings…',
    tray_stats:             'Statistics',
    tray_quit:              'Quit',
    overlay_title:          'Rest your eyes',
    overlay_rule:           'Look at something {0} metres away',
    overlay_default_msg:    'Let your eyes rest',
    overlay_skip:           'Skip',
    stats_title:            'Statistics',
    stats_breaks_today:     'Breaks today: {0}',
    settings_title:         'Settings',
    settings_timer_section: 'Timer',
    settings_overlay_section: 'Overlay',
    settings_system_section:  'System',
    settings_interval:      'Work interval (minutes)',
    settings_duration:      'Break duration (seconds)',
    settings_distance:      'Gaze distance (metres)',
    settings_message:       'Custom message (empty = default message)',
    settings_language:      'Language',
    settings_startup:       'Launch at Windows startup',
    settings_dismissible:   'Show Skip button',
    settings_sound:         'Sounds enabled',
    settings_dnd_start:     'Do not disturb — start (HH:MM)',
    settings_dnd_end:       'Do not disturb — end (HH:MM)',
    settings_save:          'Save',
    settings_cancel:        'Cancel',
    lang_fr:                'Français',
    lang_en:                'English',
  },
};

let _lang = 'fr-FR';

function setLanguage(lang) {
  _lang = LOCALES[lang] ? lang : 'fr-FR';
}

function t(key) {
  return (LOCALES[_lang] ?? LOCALES['fr-FR'])[key]
      ?? LOCALES['fr-FR'][key]
      ?? key;
}

module.exports = { t, setLanguage };
