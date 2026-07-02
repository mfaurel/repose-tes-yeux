const path = require('node:path');
const fs = require('node:fs');
const os = require('node:os');
const { test } = require('node:test');
const assert = require('node:assert/strict');

const { isCurrentlyBusy } = require('./calendar');

function icsUtc(date) {
  return date.toISOString().replace(/[-:]/g, '').split('.')[0] + 'Z';
}

function withIcsFile(lines, fn) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'rty-calendar-'));
  const file = path.join(dir, 'test.ics');
  fs.writeFileSync(file, lines.join('\r\n'), 'utf8');
  try {
    return fn(file);
  } finally {
    fs.rmSync(dir, { recursive: true, force: true });
  }
}

function baseSettings(overrides) {
  return { calendarEnabled: true, calendarType: 'ics', calendarIcsPath: '', ...overrides };
}

test('isCurrentlyBusy() returns false immediately when calendarEnabled is off', () => {
  assert.equal(isCurrentlyBusy(baseSettings({ calendarEnabled: false, calendarIcsPath: '/does/not/exist.ics' })), false);
});

test('isCurrentlyBusy() returns false when the ics file does not exist', () => {
  assert.equal(isCurrentlyBusy(baseSettings({ calendarIcsPath: path.join(os.tmpdir(), 'rty-missing.ics') })), false);
});

test('isCurrentlyBusy() detects a meeting currently in progress', () => {
  const now = new Date();
  const start = icsUtc(new Date(now.getTime() - 5 * 60 * 1000));
  const end = icsUtc(new Date(now.getTime() + 5 * 60 * 1000));
  withIcsFile([
    'BEGIN:VCALENDAR',
    'BEGIN:VEVENT',
    `DTSTART:${start}`,
    `DTEND:${end}`,
    'SUMMARY:Standup',
    'END:VEVENT',
    'END:VCALENDAR',
  ], (file) => {
    assert.equal(isCurrentlyBusy(baseSettings({ calendarIcsPath: file })), true);
  });
});

test('isCurrentlyBusy() ignores a meeting that already ended', () => {
  const now = new Date();
  const start = icsUtc(new Date(now.getTime() - 60 * 60 * 1000));
  const end = icsUtc(new Date(now.getTime() - 30 * 60 * 1000));
  withIcsFile([
    'BEGIN:VCALENDAR',
    'BEGIN:VEVENT',
    `DTSTART:${start}`,
    `DTEND:${end}`,
    'END:VEVENT',
    'END:VCALENDAR',
  ], (file) => {
    assert.equal(isCurrentlyBusy(baseSettings({ calendarIcsPath: file })), false);
  });
});

test('isCurrentlyBusy() ignores TRANSPARENT events even during their time window', () => {
  const now = new Date();
  const start = icsUtc(new Date(now.getTime() - 5 * 60 * 1000));
  const end = icsUtc(new Date(now.getTime() + 5 * 60 * 1000));
  withIcsFile([
    'BEGIN:VCALENDAR',
    'BEGIN:VEVENT',
    `DTSTART:${start}`,
    `DTEND:${end}`,
    'TRANSP:TRANSPARENT',
    'END:VEVENT',
    'END:VCALENDAR',
  ], (file) => {
    assert.equal(isCurrentlyBusy(baseSettings({ calendarIcsPath: file })), false);
  });
});

test('isCurrentlyBusy() ignores CANCELLED events even during their time window', () => {
  const now = new Date();
  const start = icsUtc(new Date(now.getTime() - 5 * 60 * 1000));
  const end = icsUtc(new Date(now.getTime() + 5 * 60 * 1000));
  withIcsFile([
    'BEGIN:VCALENDAR',
    'BEGIN:VEVENT',
    `DTSTART:${start}`,
    `DTEND:${end}`,
    'STATUS:CANCELLED',
    'END:VEVENT',
    'END:VCALENDAR',
  ], (file) => {
    assert.equal(isCurrentlyBusy(baseSettings({ calendarIcsPath: file })), false);
  });
});

test('isCurrentlyBusy() dispatches to the Outlook COM check for calendarType "outlook"', (t) => {
  t.mock.module('child_process', {
    namedExports: { execSync: () => Buffer.from('true\n') },
  });
  delete require.cache[require.resolve('./calendar')];
  const { isCurrentlyBusy: isBusyMocked } = require('./calendar');
  assert.equal(isBusyMocked(baseSettings({ calendarType: 'outlook' })), true);
});
