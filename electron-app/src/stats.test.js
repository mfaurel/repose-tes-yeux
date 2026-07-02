const path = require('node:path');
const fs = require('node:fs');
const os = require('node:os');
const { test } = require('node:test');
const assert = require('node:assert/strict');

const STATS_MODULE = require.resolve('./stats');

// stats.js caches its data in a module-level variable, so each test needs a
// fresh require (cache cleared) alongside a fresh mocked userData directory.
function freshStats(t, userDataDir) {
  t.mock.module('electron', { namedExports: { app: { getPath: () => userDataDir } } });
  delete require.cache[STATS_MODULE];
  return require('./stats');
}

function withTmpDir(fn) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'rty-stats-'));
  try {
    return fn(dir);
  } finally {
    fs.rmSync(dir, { recursive: true, force: true });
  }
}

function writeStatsFile(dir, data) {
  fs.mkdirSync(dir, { recursive: true });
  fs.writeFileSync(path.join(dir, 'stats.json'), JSON.stringify(data), 'utf8');
}

const today = () => new Date().toISOString().slice(0, 10);

test('increment() creates a fresh entry for today and updates totals', (t) => {
  withTmpDir((dir) => {
    const stats = freshStats(t, dir);
    stats.increment(20);
    stats.increment(30);
    const all = stats.getAll();
    const entry = all.history.find((e) => e.date === today());
    assert.equal(entry.breaks, 2);
    assert.equal(entry.totalPauseSec, 50);
    assert.equal(all.totalBreaks, 2);
    assert.equal(all.totalPauseSec, 50);
  });
});

test('getToday() returns 0 when nothing recorded yet', (t) => {
  withTmpDir((dir) => {
    const stats = freshStats(t, dir);
    assert.equal(stats.getToday(), 0);
  });
});

test('legacy number-only stats file is migrated to the history format', (t) => {
  withTmpDir((dir) => {
    writeStatsFile(dir, 42);
    const stats = freshStats(t, dir);
    assert.deepEqual(stats.getAll(), { history: [], totalBreaks: 0, totalPauseSec: 0 });
  });
});

test('getGamification() awards first_break once at least one break is logged', (t) => {
  withTmpDir((dir) => {
    const stats = freshStats(t, dir);
    let ids = stats.getGamification().earned.map((b) => b.id);
    assert.ok(!ids.includes('first_break'));
    stats.increment(20);
    ids = stats.getGamification().earned.map((b) => b.id);
    assert.ok(ids.includes('first_break'));
  });
});

test('getGamification() awards daily_5 only when a single day reaches 5 breaks', (t) => {
  withTmpDir((dir) => {
    writeStatsFile(dir, {
      history: [{ date: today(), breaks: 5, totalPauseSec: 100 }],
      totalBreaks: 5,
      totalPauseSec: 100,
    });
    const stats = freshStats(t, dir);
    const ids = stats.getGamification().earned.map((b) => b.id);
    assert.ok(ids.includes('daily_5'));
  });
});

test('getGamification() streak counts distinct active days, not consecutiveness', (t) => {
  withTmpDir((dir) => {
    writeStatsFile(dir, {
      history: [
        { date: '2024-01-01', breaks: 1, totalPauseSec: 20 },
        { date: '2024-01-05', breaks: 1, totalPauseSec: 20 },
        { date: '2024-01-09', breaks: 0, totalPauseSec: 0 },
        { date: '2024-01-10', breaks: 2, totalPauseSec: 40 },
      ],
      totalBreaks: 4,
      totalPauseSec: 80,
    });
    const stats = freshStats(t, dir);
    const g = stats.getGamification();
    assert.equal(g.streak, 3);
    const earnedIds = g.earned.map((b) => b.id);
    assert.ok(earnedIds.includes('streak_3'));
    assert.ok(!earnedIds.includes('streak_7'));
  });
});

test('getGamification() partitions all badges between earned and locked', (t) => {
  withTmpDir((dir) => {
    writeStatsFile(dir, {
      history: [{ date: today(), breaks: 1, totalPauseSec: 20 }],
      totalBreaks: 100,
      totalPauseSec: 2000,
    });
    const stats = freshStats(t, dir);
    const g = stats.getGamification();
    assert.equal(g.earned.length + g.locked.length, 8);
    assert.ok(g.earned.map((b) => b.id).includes('breaks_100'));
    assert.ok(g.locked.map((b) => b.id).includes('breaks_500'));
  });
});
