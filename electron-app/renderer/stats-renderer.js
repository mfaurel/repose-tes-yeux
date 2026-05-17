const STRINGS = {
  'fr-FR': { label: 'Pauses aujourd\'hui', hint: 'Appuyez sur Échap pour fermer' },
  'en-GB': { label: 'Breaks today',        hint: 'Press Escape to close' },
};

async function init() {
  const [{ breaksToday }, settings] = await Promise.all([
    window.statsApi.get(),
    window.settingsApi.get(),
  ]);

  const lang = settings.language ?? 'fr-FR';
  const str  = STRINGS[lang] ?? STRINGS['fr-FR'];

  document.getElementById('label').textContent = str.label;
  document.getElementById('count').textContent = breaksToday;
  document.getElementById('hint').textContent  = str.hint;
}

document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape') window.close();
});

init();
