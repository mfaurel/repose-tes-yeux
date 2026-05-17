const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('overlay', {
  dismiss: () => ipcRenderer.send('dismiss'),
  getRestDuration: () => ipcRenderer.invoke('get-rest-duration'),
});
