const path = require('path');
const fs = require('fs');
const { app } = require('electron');

function getPath() {
  return path.join(app.getPath('userData'), 'stats.json');
}

let _data = null;

function getData() {
  if (_data) return _data;
  try {
    _data = JSON.parse(fs.readFileSync(getPath(), 'utf8'));
    // Migrate legacy format (just a counter, no history)
    if (typeof _data === 'number' || !_data.history) {
      _data = { history: [], totalBreaks: 0, totalPauseSec: 0 };
    }
  } catch {
    _data = { history: [], totalBreaks: 0, totalPauseSec: 0 };
  }
  return _data;
}

function persist() {
  const p = getPath();
  fs.mkdirSync(path.dirname(p), { recursive: true });
  fs.writeFileSync(p, JSON.stringify(_data, null, 2), 'utf8');
}

function todayStr() {
  return new Date().toISOString().slice(0, 10);
}

function increment(pauseDurationSec) {
  const d = getData();
  const today = todayStr();
  let entry = d.history.find(e => e.date === today);
  if (!entry) {
    entry = { date: today, breaks: 0, totalPauseSec: 0 };
    d.history.push(entry);
  }
  entry.breaks++;
  entry.totalPauseSec += pauseDurationSec || 0;
  d.totalBreaks = (d.totalBreaks || 0) + 1;
  d.totalPauseSec = (d.totalPauseSec || 0) + (pauseDurationSec || 0);
  persist();
}

function getToday() {
  const d = getData();
  const today = todayStr();
  const entry = d.history.find(e => e.date === today);
  return entry ? entry.breaks : 0;
}

function getAll() {
  return getData();
}

module.exports = { increment, getToday, getAll };
