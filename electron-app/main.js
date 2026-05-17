const { app, BrowserWindow, screen, Tray, Menu, nativeImage, ipcMain } = require('electron');
const path = require('path');

// Single instance lock — must be before app.whenReady
if (!app.requestSingleInstanceLock()) { app.quit(); }

const { load: loadSettings, save: saveSettings } = require('./src/settings');
const { t, setLanguage } = require('./src/i18n');
const stats = require('./src/stats');

// ── State ──────────────────────────────────────────────────────────────────
let settings = loadSettings();
let state = 'working'; // 'working' | 'break' | 'paused'
let remainingMs = 0;
let pausedRemainingMs = 0;
let tickInterval = null;

let overlayWindows = [];
let settingsWindow = null;
let statsWindow = null;
let tray = null;

setLanguage(settings.language);

// ── Helpers ────────────────────────────────────────────────────────────────

function fmt(ms) {
  const totalSec = Math.max(0, Math.ceil(ms / 1000));
  const m = Math.floor(totalSec / 60);
  const s = totalSec % 60;
  return m > 0 ? `${m}:${s.toString().padStart(2, '0')}` : `${s}s`;
}

function isInDND() {
  const { doNotDisturbStart: ds, doNotDisturbEnd: de } = settings;
  if (!ds || !de) return false;
  const now = new Date();
  const nowM = now.getHours() * 60 + now.getMinutes();
  const [sh, sm] = ds.split(':').map(Number);
  const [eh, em] = de.split(':').map(Number);
  const s = sh * 60 + sm;
  const e = eh * 60 + em;
  return s <= e ? nowM >= s && nowM <= e : nowM >= s || nowM <= e;
}

// 16×16 RGBA solid teal (#4fc3f7) — no external asset required
function createTrayIcon() {
  const size = 16;
  const buf = Buffer.alloc(size * size * 4);
  for (let i = 0; i < size * size; i++) {
    buf[i * 4 + 0] = 79;   // R
    buf[i * 4 + 1] = 195;  // G
    buf[i * 4 + 2] = 247;  // B
    buf[i * 4 + 3] = 255;  // A
  }
  return nativeImage.createFromBuffer(buf, { width: size, height: size });
}

// ── Tray ───────────────────────────────────────────────────────────────────

function buildTrayMenu() {
  return Menu.buildFromTemplate([
    {
      label: state === 'paused' ? t('tray_resume') : t('tray_pause'),
      enabled: state !== 'break',
      click: () => { state === 'paused' ? resumeWork() : pauseWork(); },
    },
    {
      label: t('tray_break_now'),
      enabled: state === 'working',
      click: () => triggerBreak(),
    },
    { type: 'separator' },
    { label: t('tray_settings'), click: openSettings },
    { label: t('tray_stats'),    click: openStats },
    { type: 'separator' },
    { label: t('tray_quit'),     click: () => app.quit() },
  ]);
}

function updateTrayTooltip() {
  if (!tray) return;
  const template =
    state === 'break'  ? t('tray_tooltip_break') :
    state === 'paused' ? t('tray_tooltip_paused') :
                         t('tray_tooltip_working');
  tray.setToolTip(template.replace('{0}', fmt(remainingMs)));
}

function rebuildTrayMenu() {
  if (tray) tray.setContextMenu(buildTrayMenu());
}

// ── Timer state machine ────────────────────────────────────────────────────

function startWorkPhase() {
  state = 'working';
  remainingMs = settings.workIntervalMinutes * 60 * 1000;
  rebuildTrayMenu();
  updateTrayTooltip();
}

function triggerBreak() {
  if (isInDND()) {
    startWorkPhase();
    return;
  }
  state = 'break';
  remainingMs = settings.breakDurationSeconds * 1000;
  stats.increment();
  createOverlays();
  rebuildTrayMenu();
  updateTrayTooltip();
}

function endBreak() {
  // Ask overlays to play end-sound, then close after brief delay
  for (const win of overlayWindows) {
    if (!win.isDestroyed()) win.webContents.send('overlay:end-sound');
  }
  setTimeout(closeOverlays, 300);
  startWorkPhase();
}

function pauseWork() {
  if (state !== 'working') return;
  state = 'paused';
  pausedRemainingMs = remainingMs;
  rebuildTrayMenu();
  updateTrayTooltip();
}

function resumeWork() {
  if (state !== 'paused') return;
  state = 'working';
  remainingMs = pausedRemainingMs;
  rebuildTrayMenu();
  updateTrayTooltip();
}

function tick() {
  if (state === 'paused') return;
  remainingMs = Math.max(0, remainingMs - 1000);
  updateTrayTooltip();
  if (remainingMs <= 0) {
    if (state === 'working') triggerBreak();
    else if (state === 'break') endBreak();
  }
}

// ── Overlay windows ────────────────────────────────────────────────────────

function createOverlays() {
  closeOverlays();
  for (const display of screen.getAllDisplays()) {
    const { x, y, width, height } = display.bounds;
    const win = new BrowserWindow({
      x, y, width, height,
      frame: false, transparent: true,
      resizable: false, movable: false,
      skipTaskbar: true, focusable: false,
      hasShadow: false,
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        contextIsolation: true, nodeIntegration: false,
      },
    });
    win.setAlwaysOnTop(true, 'screen-saver');
    win.setVisibleOnAllWorkspaces(true, { visibleOnFullScreen: true });
    win.loadFile(path.join(__dirname, 'renderer', 'index.html'));
    overlayWindows.push(win);
  }
}

function closeOverlays() {
  for (const win of overlayWindows) {
    if (!win.isDestroyed()) win.close();
  }
  overlayWindows = [];
}

// ── Secondary windows ──────────────────────────────────────────────────────

function openSettings() {
  if (settingsWindow && !settingsWindow.isDestroyed()) {
    settingsWindow.focus(); return;
  }
  settingsWindow = new BrowserWindow({
    width: 480, height: 610,
    resizable: false, skipTaskbar: false,
    title: t('settings_title'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true, nodeIntegration: false,
    },
  });
  settingsWindow.setMenu(null);
  settingsWindow.loadFile(path.join(__dirname, 'renderer', 'settings.html'));
}

function openStats() {
  if (statsWindow && !statsWindow.isDestroyed()) {
    statsWindow.focus(); return;
  }
  statsWindow = new BrowserWindow({
    width: 320, height: 200,
    resizable: false, skipTaskbar: false,
    title: t('stats_title'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true, nodeIntegration: false,
    },
  });
  statsWindow.setMenu(null);
  statsWindow.loadFile(path.join(__dirname, 'renderer', 'stats.html'));
}

// ── IPC handlers ───────────────────────────────────────────────────────────

ipcMain.handle('overlay:config', () => ({
  dismissible:     settings.overlayDismissible,
  soundEnabled:    settings.soundEnabled,
  distanceMetres:  settings.distanceMetres,
  overlayMessage:  settings.overlayMessage,
  language:        settings.language,
  breakDurationMs: settings.breakDurationSeconds * 1000,
}));

ipcMain.on('overlay:dismiss', () => {
  if (state === 'break') endBreak();
});

ipcMain.handle('settings:get', () => ({ ...settings }));

ipcMain.handle('settings:save', (_, newSettings) => {
  settings = { ...settings, ...newSettings };
  saveSettings(settings);
  setLanguage(settings.language);
  app.setLoginItemSettings({ openAtLogin: settings.launchAtStartup });
  if (state === 'working') remainingMs = settings.workIntervalMinutes * 60 * 1000;
  rebuildTrayMenu();
  updateTrayTooltip();
  return { ok: true };
});

ipcMain.handle('stats:get', () => ({ breaksToday: stats.getToday() }));

// ── App lifecycle ──────────────────────────────────────────────────────────

app.whenReady().then(() => {
  tray = new Tray(createTrayIcon());
  rebuildTrayMenu();
  app.setLoginItemSettings({ openAtLogin: settings.launchAtStartup });
  startWorkPhase();
  tickInterval = setInterval(tick, 1000);
  app.on('window-all-closed', (e) => e.preventDefault());
});

app.on('before-quit', () => {
  clearInterval(tickInterval);
  closeOverlays();
});
