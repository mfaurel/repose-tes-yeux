const { app, BrowserWindow, screen, Tray, Menu, nativeImage, ipcMain } = require('electron');
const path = require('path');
const fs = require('fs');
const { execSync } = require('child_process');

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

// ── Startup shortcut management ────────────────────────────────────────────

function manageStartupShortcut(enable) {
  const lnkPath = path.join(
    process.env.APPDATA,
    'Microsoft', 'Windows', 'Start Menu', 'Programs', 'Startup',
    'ReposeTeYeux.lnk'
  );

  if (!enable) {
    try { if (fs.existsSync(lnkPath)) fs.unlinkSync(lnkPath); } catch (_) {}
    return;
  }

  if (fs.existsSync(lnkPath)) return;

  const vbsPath = path.resolve(app.getAppPath(), '..', 'Lancer.vbs');
  const scriptDir = path.dirname(vbsPath);
  const psScript = `
$ws = New-Object -ComObject WScript.Shell
$lnk = $ws.CreateShortcut('${lnkPath.replace(/'/g, "''")}')
$lnk.TargetPath = 'wscript.exe'
$lnk.Arguments = '"${vbsPath.replace(/\\/g, '\\\\').replace(/'/g, "''")}"'
$lnk.WorkingDirectory = '${scriptDir.replace(/'/g, "''")}'
$lnk.Description = 'Repose Tes Yeux'
$lnk.Save()
`;
  const encoded = Buffer.from(psScript, 'utf16le').toString('base64');
  try {
    execSync(`powershell -WindowStyle Hidden -NoProfile -EncodedCommand ${encoded}`, { windowsHide: true });
  } catch (_) {}
}

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

// 3-col × 5-row bitmap font for tray timer (MSB = left column)
const BITMAP_FONT = {
  '0': [0b111, 0b101, 0b101, 0b101, 0b111],
  '1': [0b010, 0b110, 0b010, 0b010, 0b111],
  '2': [0b111, 0b001, 0b111, 0b100, 0b111],
  '3': [0b111, 0b001, 0b111, 0b001, 0b111],
  '4': [0b101, 0b101, 0b111, 0b001, 0b001],
  '5': [0b111, 0b100, 0b111, 0b001, 0b111],
  '6': [0b111, 0b100, 0b111, 0b101, 0b111],
  '7': [0b111, 0b001, 0b001, 0b010, 0b010],
  '8': [0b111, 0b101, 0b111, 0b101, 0b111],
  '9': [0b111, 0b101, 0b111, 0b001, 0b111],
  ':': [0b000, 0b010, 0b000, 0b010, 0b000],
};

// 32×32 icon with 2-digit countdown at 3× scale — much more readable than MM:SS
// Working: teal on dark blue | Break: orange on dark amber | Paused: gray on dark
// Last minute of work: red on dark red (urgent visual cue)
function createTimerIcon(ms, currentState) {
  const size = 32, scale = 3;
  const charW = 3 * scale; // 9px per digit
  const charH = 5 * scale; // 15px tall

  const totalSec = Math.max(0, Math.ceil(ms / 1000));
  const isLastMinute = currentState === 'working' && totalSec < 60;

  // Show minutes when ≥60s; show seconds when <60s (last minute or break countdown)
  const val = totalSec >= 60 ? Math.min(99, Math.floor(totalSec / 60)) : (totalSec % 60);
  const text = val.toString().padStart(2, '0');

  const totalW = 2 * charW + 1; // 19px (1px gap between digits)
  const xStart = Math.floor((size - totalW) / 2); // 6
  const yStart = Math.floor((size - charH) / 2);  // 8

  let bgR, bgG, bgB, fgR, fgG, fgB;
  if (currentState === 'break') {
    [bgR, bgG, bgB] = [40, 18, 0];
    [fgR, fgG, fgB] = [255, 152, 0];
  } else if (currentState === 'paused') {
    [bgR, bgG, bgB] = [22, 22, 22];
    [fgR, fgG, fgB] = [130, 130, 130];
  } else if (isLastMinute) {
    [bgR, bgG, bgB] = [42, 8, 8];
    [fgR, fgG, fgB] = [255, 60, 60];
  } else {
    [bgR, bgG, bgB] = [10, 26, 38];
    [fgR, fgG, fgB] = [79, 195, 247];
  }

  const buf = Buffer.alloc(size * size * 4, 0);
  for (let i = 0; i < size * size; i++) {
    buf[i * 4] = bgR; buf[i * 4 + 1] = bgG; buf[i * 4 + 2] = bgB; buf[i * 4 + 3] = 255;
  }

  for (let ci = 0; ci < 2; ci++) {
    const rows = BITMAP_FONT[text[ci]];
    if (!rows) continue;
    const cx = xStart + ci * (charW + 1);
    for (let row = 0; row < 5; row++) {
      const bits = rows[row];
      for (let col = 0; col < 3; col++) {
        if (!((bits >> (2 - col)) & 1)) continue;
        for (let dy = 0; dy < scale; dy++) {
          for (let dx = 0; dx < scale; dx++) {
            const px = cx + col * scale + dx;
            const py = yStart + row * scale + dy;
            if (px < 0 || px >= size || py < 0 || py >= size) continue;
            const i = (py * size + px) * 4;
            buf[i] = fgR; buf[i + 1] = fgG; buf[i + 2] = fgB; buf[i + 3] = 255;
          }
        }
      }
    }
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

function updateTray() {
  if (!tray) return;
  const template =
    state === 'break'  ? t('tray_tooltip_break') :
    state === 'paused' ? t('tray_tooltip_paused') :
                         t('tray_tooltip_working');
  tray.setToolTip(template.replace('{0}', fmt(remainingMs)));
  tray.setImage(createTimerIcon(remainingMs, state));
}

function rebuildTrayMenu() {
  if (tray) tray.setContextMenu(buildTrayMenu());
}

// ── Timer state machine ────────────────────────────────────────────────────

function startWorkPhase() {
  state = 'working';
  remainingMs = settings.workIntervalMinutes * 60 * 1000;
  rebuildTrayMenu();
  updateTray();
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
  updateTray();
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
  updateTray();
}

function resumeWork() {
  if (state !== 'paused') return;
  state = 'working';
  remainingMs = pausedRemainingMs;
  rebuildTrayMenu();
  updateTray();
}

function tick() {
  if (state === 'paused') return;
  remainingMs = Math.max(0, remainingMs - 1000);
  updateTray();
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
  manageStartupShortcut(settings.launchAtStartup);
  if (state === 'working') remainingMs = settings.workIntervalMinutes * 60 * 1000;
  rebuildTrayMenu();
  updateTray();
  return { ok: true };
});

ipcMain.handle('stats:get', () => ({ breaksToday: stats.getToday() }));

// ── App lifecycle ──────────────────────────────────────────────────────────

app.whenReady().then(() => {
  tray = new Tray(createTimerIcon(settings.workIntervalMinutes * 60 * 1000, 'working'));
  rebuildTrayMenu();
  // Clear any old registry-based startup entry left by previous builds
  app.setLoginItemSettings({ openAtLogin: false });
  manageStartupShortcut(settings.launchAtStartup);
  startWorkPhase();
  tickInterval = setInterval(tick, 1000);
  app.on('window-all-closed', (e) => e.preventDefault());
});

app.on('before-quit', () => {
  clearInterval(tickInterval);
  closeOverlays();
});
