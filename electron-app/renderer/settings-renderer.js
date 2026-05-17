const LABELS = {
  'fr-FR': {
    pageTitle:   'Paramètres',
    secTimer:    'Minuterie',
    secOverlay:  'Overlay',
    secSystem:   'Système',
    lblInterval:   'Intervalle de travail (minutes)',
    lblDuration:   'Durée de la pause (secondes)',
    lblDistance:   'Distance de regard (mètres)',
    lblMessage:    'Message personnalisé',
    msgPlaceholder: 'Laisse vide pour le message par défaut',
    lblDismissible: 'Afficher le bouton « Passer »',
    lblLanguage:   'Langue',
    lblStartup:    'Lancer au démarrage de Windows',
    lblSound:      'Sons activés',
    lblDndStart:   'Ne pas déranger — début',
    lblDndEnd:     'Ne pas déranger — fin',
    btnSave:       'Enregistrer',
    btnCancel:     'Annuler',
  },
  'en-GB': {
    pageTitle:   'Settings',
    secTimer:    'Timer',
    secOverlay:  'Overlay',
    secSystem:   'System',
    lblInterval:   'Work interval (minutes)',
    lblDuration:   'Break duration (seconds)',
    lblDistance:   'Gaze distance (metres)',
    lblMessage:    'Custom message',
    msgPlaceholder: 'Leave empty for the default message',
    lblDismissible: 'Show Skip button',
    lblLanguage:   'Language',
    lblStartup:    'Launch at Windows startup',
    lblSound:      'Sounds enabled',
    lblDndStart:   'Do not disturb — start',
    lblDndEnd:     'Do not disturb — end',
    btnSave:       'Save',
    btnCancel:     'Cancel',
  },
};

function applyLabels(lang) {
  const L = LABELS[lang] ?? LABELS['fr-FR'];
  document.getElementById('pageTitle').textContent   = L.pageTitle;
  document.getElementById('secTimer').textContent    = L.secTimer;
  document.getElementById('secOverlay').textContent  = L.secOverlay;
  document.getElementById('secSystem').textContent   = L.secSystem;
  document.getElementById('lblInterval').textContent  = L.lblInterval;
  document.getElementById('lblDuration').textContent  = L.lblDuration;
  document.getElementById('lblDistance').textContent  = L.lblDistance;
  document.getElementById('lblMessage').textContent   = L.lblMessage;
  document.getElementById('lblDismissible').textContent = L.lblDismissible;
  document.getElementById('lblLanguage').textContent  = L.lblLanguage;
  document.getElementById('lblStartup').textContent   = L.lblStartup;
  document.getElementById('lblSound').textContent     = L.lblSound;
  document.getElementById('lblDndStart').textContent  = L.lblDndStart;
  document.getElementById('lblDndEnd').textContent    = L.lblDndEnd;
  document.getElementById('saveBtn').textContent      = L.btnSave;
  document.getElementById('cancelBtn').textContent    = L.btnCancel;
  document.getElementById('overlayMessage').placeholder = L.msgPlaceholder;
}

// Live range value display
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
  document.getElementById('workIntervalMinutes').value  = settings.workIntervalMinutes;
  document.getElementById('breakDurationSeconds').value = settings.breakDurationSeconds;
  document.getElementById('distanceMetres').value       = settings.distanceMetres;
  document.getElementById('overlayMessage').value       = settings.overlayMessage;
  document.getElementById('language').value             = settings.language;
  document.getElementById('launchAtStartup').checked   = settings.launchAtStartup;
  document.getElementById('overlayDismissible').checked = settings.overlayDismissible;
  document.getElementById('soundEnabled').checked       = settings.soundEnabled;
  document.getElementById('doNotDisturbStart').value   = settings.doNotDisturbStart ?? '';
  document.getElementById('doNotDisturbEnd').value     = settings.doNotDisturbEnd ?? '';

  // Range live values
  bindRange('workIntervalMinutes');
  bindRange('breakDurationSeconds');
  bindRange('distanceMetres');

  // Labels in current language
  applyLabels(settings.language);

  // Relabel if language changes live
  document.getElementById('language').addEventListener('change', (e) => {
    applyLabels(e.target.value);
  });

  // Save
  document.getElementById('saveBtn').addEventListener('click', async () => {
    const updated = {
      workIntervalMinutes:  parseInt(document.getElementById('workIntervalMinutes').value, 10),
      breakDurationSeconds: parseInt(document.getElementById('breakDurationSeconds').value, 10),
      distanceMetres:       parseInt(document.getElementById('distanceMetres').value, 10),
      overlayMessage:       document.getElementById('overlayMessage').value.trim(),
      language:             document.getElementById('language').value,
      launchAtStartup:      document.getElementById('launchAtStartup').checked,
      overlayDismissible:   document.getElementById('overlayDismissible').checked,
      soundEnabled:         document.getElementById('soundEnabled').checked,
      doNotDisturbStart:    document.getElementById('doNotDisturbStart').value,
      doNotDisturbEnd:      document.getElementById('doNotDisturbEnd').value,
    };
    await window.settingsApi.save(updated);
    window.close();
  });

  // Cancel
  document.getElementById('cancelBtn').addEventListener('click', () => window.close());
}

init();
