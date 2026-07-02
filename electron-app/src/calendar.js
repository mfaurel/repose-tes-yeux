const fs = require('fs');
const { execSync } = require('child_process');

function isCurrentlyBusy(settings) {
  if (!settings.calendarEnabled) return false;
  try {
    if (settings.calendarType === 'ics') return checkIcs(settings.calendarIcsPath);
    if (settings.calendarType === 'outlook') return checkOutlook();
  } catch (_) {}
  return false;
}

// ── ICS parser ─────────────────────────────────────────────────────────────

function parseIcsDate(str) {
  if (!str) return null;
  const s = str.trim();
  if (s.length === 8) {
    return new Date(
      parseInt(s.slice(0, 4), 10),
      parseInt(s.slice(4, 6), 10) - 1,
      parseInt(s.slice(6, 8), 10)
    );
  }
  const year  = parseInt(s.slice(0, 4), 10);
  const month = parseInt(s.slice(4, 6), 10) - 1;
  const day   = parseInt(s.slice(6, 8), 10);
  const hour  = parseInt(s.slice(9, 11), 10);
  const min   = parseInt(s.slice(11, 13), 10);
  const sec   = parseInt(s.slice(13, 15), 10);
  if (s.endsWith('Z')) {
    return new Date(Date.UTC(year, month, day, hour, min, sec));
  }
  return new Date(year, month, day, hour, min, sec);
}

function checkIcs(filePath) {
  if (!filePath) return false;
  const content = fs.readFileSync(filePath, 'utf8');
  // RFC 5545 line unfolding: CRLF + whitespace = continuation
  const unfolded = content.replace(/\r?\n[ \t]/g, '');
  const lines = unfolded.split(/\r?\n/);
  const now = Date.now();

  let inVEvent = false;
  let dtstart = null, dtend = null, transp = '', status = '';

  for (const line of lines) {
    const upper = line.toUpperCase();
    if (upper === 'BEGIN:VEVENT') {
      inVEvent = true;
      dtstart = dtend = null;
      transp = '';
      status = '';
      continue;
    }
    if (upper === 'END:VEVENT') {
      inVEvent = false;
      if (transp === 'TRANSPARENT' || status === 'CANCELLED') continue;
      if (dtstart !== null && dtend !== null) {
        const s = parseIcsDate(dtstart);
        const e = parseIcsDate(dtend);
        if (s && e && now >= s.getTime() && now < e.getTime()) return true;
      }
      continue;
    }
    if (!inVEvent) continue;

    const colonIdx = line.indexOf(':');
    if (colonIdx < 0) continue;
    const key = line.slice(0, colonIdx).toUpperCase();
    const value = line.slice(colonIdx + 1).trim();

    if (key === 'DTSTART' || key.startsWith('DTSTART;'))      dtstart = value;
    else if (key === 'DTEND' || key.startsWith('DTEND;'))     dtend = value;
    else if (key === 'TRANSP')                                 transp = value.toUpperCase();
    else if (key === 'STATUS')                                 status = value.toUpperCase();
  }
  return false;
}

// ── Outlook COM via PowerShell ─────────────────────────────────────────────

function checkOutlook() {
  const ps = `
try {
  $ol = [System.Runtime.InteropServices.Marshal]::GetActiveObject("Outlook.Application")
  $ns = $ol.GetNamespace("MAPI")
  $cal = $ns.GetDefaultFolder(9)
  $items = $cal.Items
  $items.IncludeRecurrences = $true
  $items.Sort("[Start]")
  $now = [DateTime]::Now
  $filter = "[Start] <= '" + $now.ToString("g") + "' And [End] > '" + $now.ToString("g") + "'"
  $busy = $items.Restrict($filter)
  if ($busy.Count -gt 0) { Write-Output "true" } else { Write-Output "false" }
} catch {
  Write-Output "false"
}
`;
  const encoded = Buffer.from(ps, 'utf16le').toString('base64');
  const out = execSync(
    `powershell -NoProfile -NonInteractive -EncodedCommand ${encoded}`,
    { windowsHide: true, timeout: 5000 }
  ).toString().trim();
  return out === 'true';
}

module.exports = { isCurrentlyBusy };
