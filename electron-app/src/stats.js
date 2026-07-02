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

// ── Gamification ───────────────────────────────────────────────────────────

// Total number of distinct days where at least one break was completed
function computeStreak(d) {
  return (d.history || []).filter(e => e.breaks >= 1).length;
}

const BADGE_DEFS = [
  {
    id: 'first_break',
    label_fr: 'Premier regard',   label_en: 'First Rest',
    desc_fr:  'Première pause effectuée', desc_en: 'Completed your first break',
    check: d => d.totalBreaks >= 1,
  },
  {
    id: 'daily_5',
    label_fr: 'Journée active',   label_en: 'Active Day',
    desc_fr:  '5 pauses en une journée', desc_en: '5 breaks in a single day',
    check: d => (d.history || []).some(e => e.breaks >= 5),
  },
  {
    id: 'streak_3',
    label_fr: 'Régulier',         label_en: 'Consistent',
    desc_fr:  '3 jours actifs',   desc_en: '3 active days',
    check: d => computeStreak(d) >= 3,
  },
  {
    id: 'streak_7',
    label_fr: 'Assidu',           label_en: 'Dedicated',
    desc_fr:  '7 jours actifs',   desc_en: '7 active days',
    check: d => computeStreak(d) >= 7,
  },
  {
    id: 'streak_30',
    label_fr: 'Habitué',          label_en: 'Habitual',
    desc_fr:  '30 jours actifs',  desc_en: '30 active days',
    check: d => computeStreak(d) >= 30,
  },
  {
    id: 'breaks_100',
    label_fr: 'Centenaire',       label_en: 'Centurion',
    desc_fr:  '100 pauses au total', desc_en: '100 total breaks',
    check: d => d.totalBreaks >= 100,
  },
  {
    id: 'breaks_500',
    label_fr: 'Marathonien',      label_en: 'Marathoner',
    desc_fr:  '500 pauses au total', desc_en: '500 total breaks',
    check: d => d.totalBreaks >= 500,
  },
  {
    id: 'breaks_1000',
    label_fr: 'Légende',          label_en: 'Legend',
    desc_fr:  '1 000 pauses au total', desc_en: '1,000 total breaks',
    check: d => d.totalBreaks >= 1000,
  },
];

function getGamification() {
  const d = getData();
  const streak = computeStreak(d);
  const pick = ({ id, label_fr, label_en, desc_fr, desc_en }) =>
    ({ id, label_fr, label_en, desc_fr, desc_en });
  return {
    streak,
    earned: BADGE_DEFS.filter(b =>  b.check(d)).map(pick),
    locked: BADGE_DEFS.filter(b => !b.check(d)).map(pick),
  };
}

module.exports = { increment, getToday, getAll, getGamification };
