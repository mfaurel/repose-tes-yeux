const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('overlay', {
  getConfig:    ()  => ipcRenderer.invoke('overlay:config'),
  dismiss:      ()  => ipcRenderer.send('overlay:dismiss'),
  onEndSound:   (cb) => ipcRenderer.on('overlay:end-sound', cb),
});

contextBridge.exposeInMainWorld('settingsApi', {
  get:  ()  => ipcRenderer.invoke('settings:get'),
  save: (s) => ipcRenderer.invoke('settings:save', s),
});

contextBridge.exposeInMainWorld('statsApi', {
  get: () => ipcRenderer.invoke('stats:get'),
});
