const { test } = require('node:test');
const assert = require('node:assert/strict');
const { t, tArr, setLanguage } = require('./i18n');

test('defaults to fr-FR', () => {
  setLanguage('fr-FR');
  assert.equal(t('app_name'), 'Repose Tes Yeux');
});

test('setLanguage switches active locale', () => {
  setLanguage('en-GB');
  assert.equal(t('app_name'), 'Rest Your Eyes');
  setLanguage('fr-FR');
});

test('setLanguage falls back to fr-FR for an unknown language', () => {
  setLanguage('de-DE');
  assert.equal(t('app_name'), 'Repose Tes Yeux');
});

test('t() falls back to fr-FR value for a key missing in the active locale', () => {
  setLanguage('en-GB');
  assert.equal(t('lang_fr'), 'Français');
  setLanguage('fr-FR');
});

test('t() returns the key itself when unknown everywhere', () => {
  assert.equal(t('does_not_exist'), 'does_not_exist');
});

test('tArr() returns an array for a list key', () => {
  setLanguage('fr-FR');
  const tips = tArr('posture_tips');
  assert.ok(Array.isArray(tips));
  assert.ok(tips.length > 0);
});

test('tArr() returns an empty array for a non-list or missing key', () => {
  assert.deepEqual(tArr('app_name'), []);
  assert.deepEqual(tArr('does_not_exist'), []);
});
