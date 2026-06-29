const CIRCUMFERENCE = 2 * Math.PI * 52;

// ── Audio synthesis ────────────────────────────────────────────────────────
function playTone(frequency, durationSec, volume = 0.25) {
  try {
    const ctx = new AudioContext();
    const osc  = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.frequency.value = frequency;
    osc.type = 'sine';
    gain.gain.setValueAtTime(0, ctx.currentTime);
    gain.gain.linearRampToValueAtTime(volume, ctx.currentTime + 0.05);
    gain.gain.linearRampToValueAtTime(0, ctx.currentTime + durationSec);
    osc.start(ctx.currentTime);
    osc.stop(ctx.currentTime + durationSec + 0.05);
  } catch (_) { /* audio not available */ }
}

// ── Localised strings (subset used in overlay) ────────────────────────────
const STRINGS = {
  'fr-FR': {
    title:         'Repose tes yeux',
    longBreakTitle:'Grande pause !',
    rule:          'Regarde quelque chose à {dist} mètres',
    skip:          'Passer',
    default_msg:   'Laisse tes yeux se reposer',
    exercise_label:'Exercice suggéré',
  },
  'en-GB': {
    title:         'Rest your eyes',
    longBreakTitle:'Long break!',
    rule:          'Look at something {dist} metres away',
    skip:          'Skip',
    default_msg:   'Let your eyes rest',
    exercise_label:'Suggested exercise',
  },
};

function s(lang, key) {
  return (STRINGS[lang] ?? STRINGS['fr-FR'])[key] ?? STRINGS['fr-FR'][key] ?? key;
}

// ── Init ───────────────────────────────────────────────────────────────────
async function init() {
  const cfg = await window.overlay.getConfig();
  const lang = cfg.language ?? 'fr-FR';

  // Long break visual adjustments
  if (cfg.isLongBreak) {
    document.documentElement.style.setProperty('--accent', '#66bb6a');
    document.documentElement.style.setProperty('--accent-glow', 'rgba(102, 187, 106, 0.45)');
    const badge = document.getElementById('longBreakBadge');
    badge.textContent = s(lang, 'longBreakTitle');
    badge.style.display = 'inline-block';
  }

  // Apply localised text
  document.getElementById('overlayTitle').textContent = s(lang, 'title');
  document.getElementById('overlayRule').innerHTML =
    s(lang, 'rule').replace('{dist}', `<span class="highlight">${cfg.distanceMetres}</span>`);
  document.getElementById('dismissBtn').textContent = s(lang, 'skip');

  // Custom message
  if (cfg.overlayMessage) {
    const el = document.getElementById('customMsg');
    el.textContent = cfg.overlayMessage;
    el.style.display = 'block';
  }

  // Exercise
  if (cfg.exercise) {
    document.getElementById('exerciseLabel').textContent = s(lang, 'exercise_label');
    document.getElementById('exerciseText').textContent  = cfg.exercise;
    document.getElementById('exerciseBox').style.display = 'block';
  }

  // Skip button visibility
  if (cfg.dismissible) {
    document.getElementById('dismissBtn').style.display = 'inline-block';
  }

  // Play start tone
  if (cfg.soundEnabled) playTone(880, 0.3);

  // Countdown ring
  const durationSec = Math.round(cfg.breakDurationMs / 1000);
  const progressEl  = document.getElementById('progressCircle');
  const countdownEl = document.getElementById('countdown');

  progressEl.style.strokeDasharray  = CIRCUMFERENCE;
  progressEl.style.strokeDashoffset = 0;

  let remaining = durationSec;

  function tick() {
    remaining = Math.max(0, remaining - 1);
    countdownEl.textContent = remaining;
    progressEl.style.strokeDashoffset = CIRCUMFERENCE * (1 - remaining / durationSec);
    if (remaining <= 0) clearInterval(timer);
  }

  const timer = setInterval(tick, 1000);
  tick();

  // Signals from main process
  window.overlay.onEndSound(() => {
    if (cfg.soundEnabled) playTone(440, 0.3);
  });

  window.overlay.onFadeOut(() => {
    clearInterval(timer);
    document.getElementById('overlay').classList.add('fading-out');
  });

  // Dismiss button
  document.getElementById('dismissBtn').addEventListener('click', () => {
    clearInterval(timer);
    document.getElementById('overlay').classList.add('fading-out');
    setTimeout(() => window.overlay.dismiss(), 180);
  });
}

init();
