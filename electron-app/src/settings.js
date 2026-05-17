const path = require('path');
const fs = require('fs');
const { app } = require('electron');

const DEFAULTS = {
  workIntervalMinutes: 20,
  breakDurationSeconds: 20,
  distanceMetres: 20,
  overlayMessage: '',
  language: 'fr-FR',
  launchAtStartup: false,
  overlayDismissible: true,
  soundEnabled: true,
  doNotDisturbStart: '',
  doNotDisturbEnd: '',
};

function getPath() {
  return path.join(app.getPath('userData'), 'settings.json');
}

function load() {
  try {
    const raw = fs.readFileSync(getPath(), 'utf8');
    return { ...DEFAULTS, ...JSON.parse(raw) };
  } catch {
    return { ...DEFAULTS };
  }
}

function save(settings) {
  const p = getPath();
  fs.mkdirSync(path.dirname(p), { recursive: true });
  fs.writeFileSync(p, JSON.stringify(settings, null, 2), 'utf8');
}

module.exports = { load, save, DEFAULTS };
