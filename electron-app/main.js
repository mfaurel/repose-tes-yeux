const { app, BrowserWindow, screen, Tray, Menu, nativeImage, ipcMain } = require('electron');
const path = require('path');

const WORK_INTERVAL_MS = 20 * 60 * 1000; // 20 minutes
const REST_DURATION_MS = 20 * 1000;       // 20 seconds

let overlayWindows = [];
let tray = null;
let workTimer = null;
let isResting = false;

// --- Overlay windows (one per monitor) ---

function createOverlaysOnAllScreens() {
  closeOverlays();

  const displays = screen.getAllDisplays();

  for (const display of displays) {
    const { x, y, width, height } = display.bounds;

    const win = new BrowserWindow({
      x,
      y,
      width,
      height,
      frame: false,
      transparent: true,
      resizable: false,
      movable: false,
      skipTaskbar: true,
      focusable: false,        // never steal focus
      hasShadow: false,
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        contextIsolation: true,
        nodeIntegration: false,
      },
    });

    // 'screen-saver' level: above fullscreen apps on Windows & macOS
    win.setAlwaysOnTop(true, 'screen-saver');
    win.setVisibleOnAllWorkspaces(true, { visibleOnFullScreen: true });
    win.setIgnoreMouseEvents(false); // allow click-to-dismiss

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

// --- Timer logic ---

function startWorkTimer() {
  clearTimeout(workTimer);
  workTimer = setTimeout(triggerRest, WORK_INTERVAL_MS);
}

function triggerRest() {
  if (isResting) return;
  isResting = true;

  createOverlaysOnAllScreens();

  setTimeout(() => {
    endRest();
  }, REST_DURATION_MS);
}

function endRest() {
  closeOverlays();
  isResting = false;
  startWorkTimer();
}

// --- IPC: renderer can dismiss early ---

ipcMain.on('dismiss', () => {
  endRest();
});

// Expose rest duration to renderer
ipcMain.handle('get-rest-duration', () => REST_DURATION_MS);

// --- Tray ---

function createTray() {
  // Fallback: 1x1 transparent icon if no asset exists
  const icon = nativeImage.createEmpty();
  tray = new Tray(icon);

  const menu = Menu.buildFromTemplate([
    {
      label: 'Tester le rappel maintenant',
      click: () => { if (!isResting) triggerRest(); },
    },
    { type: 'separator' },
    { label: 'Quitter', click: () => app.quit() },
  ]);

  tray.setToolTip('Repose Tes Yeux — 20/20/20');
  tray.setContextMenu(menu);
}

// --- App lifecycle ---

app.whenReady().then(() => {
  createTray();
  startWorkTimer();

  // Keep app alive without a visible window
  app.on('window-all-closed', (e) => e.preventDefault());
});

app.on('before-quit', () => {
  clearTimeout(workTimer);
  closeOverlays();
});
