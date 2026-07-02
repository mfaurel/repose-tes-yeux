const LABELS = {
  'fr-FR': {
    pageTitle:    'Paramètres',
    secTimer:     'Minuterie',
    secAdvanced:  'Pauses avancées',
    secShortcuts: 'Raccourcis clavier',
    secSystem:    'Système',
    secTeams:     'Intégration Teams',
    secCalendar:  'Intégration agenda',
    lblInterval:        'Intervalle de travail (minutes)',
    lblDuration:        'Durée de la pause (secondes)',
    lblBreakWarning:    'Notification avant la pause (secondes, 0 = désactivé)',
    lblTeamsDnd:        'Ignorer les pauses si le statut Teams est « Ne pas déranger »',
    lblLongBreakEvery:  'Grande pause toutes les N pauses (0 = désactivé)',
    lblLongBreakDur:    'Durée de la grande pause (secondes)',
    lblPosture:         'Rappel de posture toutes les N minutes (0 = désactivé)',
    lblEndOfDay:        'Heure cible de fin de journée',
    lblShortcutPause:   'Raccourci arrêt/reprise',
    lblShortcutBreak:   'Raccourci pause immédiate',
    shortcutPlaceholder:'ex: Ctrl+Alt+P',
    lblTheme:     'Thème',
    themeAuto:    'Automatique (Windows)',
    themeDark:    'Sombre',
    themeLight:   'Clair',
    lblLanguage:  'Langue',
    lblStartup:   'Lancer au démarrage de Windows',
    lblSound:     'Sons activés',
    lblDndStart:  'Ne pas déranger — début',
    lblDndEnd:    'Ne pas déranger — fin',
    lblCalendarEnabled: 'Activer la détection de réunion',
    lblCalendarType:    'Source',
    calTypeIcs:         'Fichier .ics (Outlook, Google)',
    calTypeOutlook:     'Outlook (COM, Windows)',
    lblCalendarIcsPath: 'Chemin du fichier .ics',
    btnBrowseIcs:       'Parcourir…',
    btnSave:      'Enregistrer',
    btnCancel:    'Annuler',
  },
  'en-GB': {
    pageTitle:    'Settings',
    secTimer:     'Timer',
    secAdvanced:  'Advanced breaks',
    secShortcuts: 'Keyboard shortcuts',
    secSystem:    'System',
    secTeams:     'Teams integration',
    secCalendar:  'Calendar integration',
    lblInterval:        'Work interval (minutes)',
    lblDuration:        'Break duration (seconds)',
    lblBreakWarning:    'Notification before break (seconds, 0 = disabled)',
    lblTeamsDnd:        'Skip breaks when Teams status is Do Not Disturb',
    lblLongBreakEvery:  'Long break every N breaks (0 = disabled)',
    lblLongBreakDur:    'Long break duration (seconds)',
    lblPosture:         'Posture reminder every N minutes (0 = disabled)',
    lblEndOfDay:        'End-of-day target time',
    lblShortcutPause:   'Pause/resume shortcut',
    lblShortcutBreak:   'Immediate break shortcut',
    shortcutPlaceholder:'e.g. Ctrl+Alt+P',
    lblTheme:     'Theme',
    themeAuto:    'Automatic (Windows)',
    themeDark:    'Dark',
    themeLight:   'Light',
    lblLanguage:  'Language',
    lblStartup:   'Launch at Windows startup',
    lblSound:     'Sounds enabled',
    lblDndStart:  'Do not disturb — start',
    lblDndEnd:    'Do not disturb — end',
    lblCalendarEnabled: 'Enable meeting detection',
    lblCalendarType:    'Source',
    calTypeIcs:         '.ics file (Outlook, Google)',
    calTypeOutlook:     'Outlook (COM, Windows)',
    lblCalendarIcsPath: '.ics file path',
    btnBrowseIcs:       'Browse…',
    btnSave:      'Save',
    btnCancel:    'Cancel',
  },
};

function applyLabels(lang) {
  const L = LABELS[lang] ?? LABELS['fr-FR'];
  document.getElementById('pageTitle').textContent   = L.pageTitle;
  document.getElementById('secTimer').textContent    = L.secTimer;
  document.getElementById('secAdvanced').textContent = L.secAdvanced;
  document.getElementById('secShortcuts').textContent = L.secShortcuts;
  document.getElementById('secSystem').textContent   = L.secSystem;
  document.getElementById('lblInterval').textContent  = L.lblInterval;
  document.getElementById('lblDuration').textContent  = L.lblDuration;
  document.getElementById('lblLongBreakEvery').textContent = L.lblLongBreakEvery;
  document.getElementById('lblLongBreakDur').textContent   = L.lblLongBreakDur;
  document.getElementById('lblPosture').textContent   = L.lblPosture;
  document.getElementById('lblEndOfDay').textContent  = L.lblEndOfDay;
  document.getElementById('lblShortcutPause').textContent = L.lblShortcutPause;
  document.getElementById('lblShortcutBreak').textContent = L.lblShortcutBreak;
  document.getElementById('lblTheme').textContent    = L.lblTheme;
  document.getElementById('lblLanguage').textContent  = L.lblLanguage;
  document.getElementById('lblStartup').textContent   = L.lblStartup;
  document.getElementById('lblSound').textContent     = L.lblSound;
  document.getElementById('lblDndStart').textContent  = L.lblDndStart;
  document.getElementById('lblDndEnd').textContent    = L.lblDndEnd;
  document.getElementById('saveBtn').textContent      = L.btnSave;
  document.getElementById('cancelBtn').textContent    = L.btnCancel;
  document.getElementById('shortcutPause').placeholder  = L.shortcutPlaceholder;
  document.getElementById('shortcutBreak').placeholder  = L.shortcutPlaceholder;
  document.getElementById('lblBreakWarning').textContent    = L.lblBreakWarning;
  document.getElementById('secTeams').textContent           = L.secTeams;
  document.getElementById('lblTeamsDnd').textContent        = L.lblTeamsDnd;
  document.getElementById('secCalendar').textContent        = L.secCalendar;
  document.getElementById('lblCalendarEnabled').textContent  = L.lblCalendarEnabled;
  document.getElementById('lblCalendarType').textContent    = L.lblCalendarType;
  document.getElementById('lblCalendarIcsPath').textContent = L.lblCalendarIcsPath;
  document.getElementById('btnBrowseIcs').textContent       = L.btnBrowseIcs;

  // Theme select options
  const themeEl = document.getElementById('theme');
  themeEl.options[0].text = L.themeAuto;
  themeEl.options[1].text = L.themeDark;
  themeEl.options[2].text = L.themeLight;

  // Calendar type select options
  const calTypeEl = document.getElementById('calendarType');
  calTypeEl.options[0].text = L.calTypeIcs;
  calTypeEl.options[1].text = L.calTypeOutlook;
}

function bindRange(id) {
  const input = document.getElementById(id);
  const val   = document.getElementById(id + 'Val');
  const update = () => { val.textContent = input.value; };
  input.addEventListener('input', update);
  update();
}

async function init() {
  const settings = await window.settingsApi.get();

  // Populate fields
  document.getElementById('workIntervalMinutes').value       = settings.workIntervalMinutes;
  document.getElementById('breakDurationSeconds').value      = settings.breakDurationSeconds;
  document.getElementById('breakWarningSeconds').value       = settings.breakWarningSeconds ?? 0;
  document.getElementById('longBreakEvery').value            = settings.longBreakEvery ?? 0;
  document.getElementById('longBreakDurationSeconds').value  = settings.longBreakDurationSeconds ?? 300;
  document.getElementById('postureReminderMinutes').value    = settings.postureReminderMinutes ?? 0;
  document.getElementById('endOfDayTarget').value            = settings.endOfDayTarget ?? '';
  document.getElementById('shortcutPause').value             = settings.shortcutPause ?? 'Ctrl+Alt+P';
  document.getElementById('shortcutBreak').value             = settings.shortcutBreak ?? 'Ctrl+Alt+B';
  document.getElementById('theme').value                     = settings.theme ?? 'auto';
  document.getElementById('language').value                  = settings.language;
  document.getElementById('launchAtStartup').checked         = settings.launchAtStartup;
  document.getElementById('soundEnabled').checked            = settings.soundEnabled;
  document.getElementById('doNotDisturbStart').value         = settings.doNotDisturbStart ?? '';
  document.getElementById('doNotDisturbEnd').value           = settings.doNotDisturbEnd ?? '';
  document.getElementById('teamsDndEnabled').checked         = settings.teamsDndEnabled ?? false;
  document.getElementById('calendarEnabled').checked         = settings.calendarEnabled ?? false;
  document.getElementById('calendarType').value              = settings.calendarType ?? 'ics';
  document.getElementById('calendarIcsPath').value           = settings.calendarIcsPath ?? '';

  // Range live values
  bindRange('workIntervalMinutes');
  bindRange('breakDurationSeconds');
  bindRange('breakWarningSeconds');
  bindRange('longBreakEvery');
  bindRange('longBreakDurationSeconds');
  bindRange('postureReminderMinutes');

  // Calendar UI visibility
  function updateCalendarUI() {
    const enabled = document.getElementById('calendarEnabled').checked;
    const type = document.getElementById('calendarType').value;
    document.getElementById('calendarTypeField').style.display = enabled ? '' : 'none';
    document.getElementById('calendarIcsField').style.display  = (enabled && type === 'ics') ? '' : 'none';
  }
  updateCalendarUI();
  document.getElementById('calendarEnabled').addEventListener('change', updateCalendarUI);
  document.getElementById('calendarType').addEventListener('change', updateCalendarUI);

  document.getElementById('btnBrowseIcs').addEventListener('click', async () => {
    const p = await window.calendarApi.browse();
    if (p) document.getElementById('calendarIcsPath').value = p;
  });

  // Labels in current language
  applyLabels(settings.language);

  // Relabel if language changes live
  document.getElementById('language').addEventListener('change', (e) => {
    applyLabels(e.target.value);
  });

  // Save
  document.getElementById('saveBtn').addEventListener('click', async () => {
    const updated = {
      workIntervalMinutes:      parseInt(document.getElementById('workIntervalMinutes').value, 10),
      breakDurationSeconds:     parseInt(document.getElementById('breakDurationSeconds').value, 10),
      breakWarningSeconds:      parseInt(document.getElementById('breakWarningSeconds').value, 10),
      longBreakEvery:           parseInt(document.getElementById('longBreakEvery').value, 10),
      longBreakDurationSeconds: parseInt(document.getElementById('longBreakDurationSeconds').value, 10),
      postureReminderMinutes:   parseInt(document.getElementById('postureReminderMinutes').value, 10),
      endOfDayTarget:           document.getElementById('endOfDayTarget').value,
      shortcutPause:            document.getElementById('shortcutPause').value.trim(),
      shortcutBreak:            document.getElementById('shortcutBreak').value.trim(),
      theme:                    document.getElementById('theme').value,
      language:                 document.getElementById('language').value,
      launchAtStartup:          document.getElementById('launchAtStartup').checked,
      soundEnabled:             document.getElementById('soundEnabled').checked,
      doNotDisturbStart:        document.getElementById('doNotDisturbStart').value,
      doNotDisturbEnd:          document.getElementById('doNotDisturbEnd').value,
      teamsDndEnabled:          document.getElementById('teamsDndEnabled').checked,
      calendarEnabled:          document.getElementById('calendarEnabled').checked,
      calendarType:             document.getElementById('calendarType').value,
      calendarIcsPath:          document.getElementById('calendarIcsPath').value,
    };
    await window.settingsApi.save(updated);
    window.close();
  });

  // Cancel
  document.getElementById('cancelBtn').addEventListener('click', () => window.close());
}

init();
