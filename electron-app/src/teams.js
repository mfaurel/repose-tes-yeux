const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

function isTeamsDnd(settings) {
  if (!settings.teamsDndEnabled) return false;
  try { return checkClassicTeams() || checkNewTeams(); } catch (_) {}
  return false;
}

// ── Classic Teams (v1) ─────────────────────────────────────────────────────
// Reads %APPDATA%\Microsoft\Teams\storage.json — present when classic Teams is installed.

function checkClassicTeams() {
  const p = path.join(
    process.env.APPDATA || path.join(require('os').homedir(), 'AppData', 'Roaming'),
    'Microsoft', 'Teams', 'storage.json'
  );
  try {
    const data = JSON.parse(fs.readFileSync(p, 'utf8'));
    const status = (data.userStatus || data.status || '').toLowerCase();
    return status === 'dnd' || status === 'donotdisturb' || status === 'presenting';
  } catch (_) { return false; }
}

// ── New Teams (v2) via PowerShell ─────────────────────────────────────────
// Checks Windows Focus Assist state (which new Teams sets automatically
// when the user activates Do Not Disturb in the Teams status picker).

function checkNewTeams() {
  const ps = `
$dnd = $false
try {
  # New Teams sets Windows DND / Focus Assist when user picks DoNotDisturb
  $qaKey = 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.notifications.quiethourssettings\\windows.data.notifications.quiethourssettings\\Current'
  if (Test-Path $qaKey) {
    $raw = (Get-ItemProperty -Path $qaKey -Name 'Data' -ErrorAction Stop).Data
    # Byte at offset 4: 0 = off, 1 = priority-only, 2 = alarms-only
    if ($raw -and $raw.Length -gt 4 -and $raw[4] -gt 0) { $dnd = $true }
  }
} catch {}
if ($dnd) { Write-Output "true" } else { Write-Output "false" }
`;
  const encoded = Buffer.from(ps, 'utf16le').toString('base64');
  const out = execSync(
    `powershell -NoProfile -NonInteractive -EncodedCommand ${encoded}`,
    { windowsHide: true, timeout: 3000 }
  ).toString().trim();
  return out === 'true';
}

module.exports = { isTeamsDnd };
