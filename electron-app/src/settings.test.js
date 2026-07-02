const path = require('node:path');
const fs = require('node:fs');
const os = require('node:os');
const { test } = require('node:test');
const assert = require('node:assert/strict');

const SETTINGS_MODULE = require.resolve('./settings');

// settings.js reads `app` from `require('electron')` at load time, so the
// mock must be registered before each fresh require of the module.
function freshSettings(t, userDataDir) {
  t.mock.module('electron', { namedExports: { app: { getPath: () => userDataDir } } });
  delete require.cache[SETTINGS_MODULE];
  return require('./settings');
}

function withTmpDir(fn) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'rty-settings-'));
  try {
    return fn(dir);
  } finally {
    fs.rmSync(dir, { recursive: true, force: true });
  }
}

test('load() returns DEFAULTS when no settings file exists', (t) => {
  withTmpDir((dir) => {
    const settings = freshSettings(t, dir);
    assert.deepEqual(settings.load(), settings.DEFAULTS);
  });
});

test('load() returns DEFAULTS when the settings file is corrupt JSON', (t) => {
  withTmpDir((dir) => {
    fs.writeFileSync(path.join(dir, 'settings.json'), '{not valid json', 'utf8');
    const settings = freshSettings(t, dir);
    assert.deepEqual(settings.load(), settings.DEFAULTS);
  });
});

test('save() then load() round-trips and merges over DEFAULTS', (t) => {
  withTmpDir((dir) => {
    const settings = freshSettings(t, dir);
    settings.save({ ...settings.DEFAULTS, workIntervalMinutes: 45, language: 'en-GB' });
    const reloaded = settings.load();
    assert.equal(reloaded.workIntervalMinutes, 45);
    assert.equal(reloaded.language, 'en-GB');
    assert.equal(reloaded.breakDurationSeconds, settings.DEFAULTS.breakDurationSeconds);
  });
});

test('save() creates the parent directory if missing', (t) => {
  withTmpDir((dir) => {
    const nested = path.join(dir, 'nested', 'userData');
    const settings = freshSettings(t, nested);
    settings.save(settings.DEFAULTS);
    assert.ok(fs.existsSync(path.join(nested, 'settings.json')));
  });
});

test('load() fills in keys missing from a partially-saved file', (t) => {
  withTmpDir((dir) => {
    fs.writeFileSync(path.join(dir, 'settings.json'), JSON.stringify({ theme: 'dark' }), 'utf8');
    const settings = freshSettings(t, dir);
    const loaded = settings.load();
    assert.equal(loaded.theme, 'dark');
    assert.equal(loaded.workIntervalMinutes, settings.DEFAULTS.workIntervalMinutes);
  });
});
