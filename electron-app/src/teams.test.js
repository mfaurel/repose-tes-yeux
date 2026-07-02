const { test } = require('node:test');
const assert = require('node:assert/strict');

const TEAMS_MODULE = require.resolve('./teams');

function freshTeams(t, { readFileSync, execSync }) {
  t.mock.module('fs', {
    namedExports: {
      readFileSync: readFileSync ?? (() => { throw new Error('ENOENT'); }),
    },
  });
  t.mock.module('child_process', {
    namedExports: {
      execSync: execSync ?? (() => 'false'),
    },
  });
  delete require.cache[TEAMS_MODULE];
  return require('./teams');
}

test('isTeamsDnd() returns false without touching fs/exec when disabled', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => { throw new Error('should not be called'); },
    execSync: () => { throw new Error('should not be called'); },
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: false }), false);
});

test('isTeamsDnd() detects classic Teams "DoNotDisturb" status', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => JSON.stringify({ userStatus: 'DoNotDisturb' }),
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: true }), true);
});

test('isTeamsDnd() detects classic Teams "Presenting" status', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => JSON.stringify({ userStatus: 'Presenting' }),
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: true }), true);
});

test('isTeamsDnd() reads the legacy "status" key as a fallback', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => JSON.stringify({ status: 'dnd' }),
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: true }), true);
});

test('isTeamsDnd() falls through to Focus Assist when classic Teams storage is unreadable', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => { throw new Error('ENOENT'); },
    execSync: () => 'true',
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: true }), true);
});

test('isTeamsDnd() returns false when neither classic Teams nor Focus Assist report DND', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => JSON.stringify({ userStatus: 'Available' }),
    execSync: () => 'false',
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: true }), false);
});

test('isTeamsDnd() swallows errors from the Focus Assist PowerShell check', (t) => {
  const { isTeamsDnd } = freshTeams(t, {
    readFileSync: () => { throw new Error('ENOENT'); },
    execSync: () => { throw new Error('powershell not found'); },
  });
  assert.equal(isTeamsDnd({ teamsDndEnabled: true }), false);
});
