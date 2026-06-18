@echo off
cd /d "%~dp0electron-app"
if not exist node_modules (
    echo Installation des dependances...
    npm install
)
npm start
