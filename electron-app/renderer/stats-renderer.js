const STRINGS = {
  'fr-FR': {
    pageTitle:  'Statistiques',
    lblToday:   "Aujourd'hui",
    lblTotal:   'Total',
    pauseSingular: 'pause',
    pausePlural:   'pauses',
    subToday:   (n, sec) => `${n} pause${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    subTotal:   (n, sec) => `${n} pause${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    lblHistory: 'Historique (7 derniers jours)',
    colDate:    'Date',
    colBreaks:  'Pauses',
    colDuration:'Durée',
    noHistory:  'Aucune donnée',
    lblExport:  'Exporter :',
    hint:       'Appuyez sur Échap pour fermer',
  },
  'en-GB': {
    pageTitle:  'Statistics',
    lblToday:   'Today',
    lblTotal:   'All time',
    pauseSingular: 'break',
    pausePlural:   'breaks',
    subToday:   (n, sec) => `${n} break${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    subTotal:   (n, sec) => `${n} break${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    lblHistory: 'History (last 7 days)',
    colDate:    'Date',
    colBreaks:  'Breaks',
    colDuration:'Duration',
    noHistory:  'No data yet',
    lblExport:  'Export:',
    hint:       'Press Escape to close',
  },
};

function fmtDuration(sec) {
  if (!sec) return '0 min';
  const h = Math.floor(sec / 3600);
  const m = Math.floor((sec % 3600) / 60);
  const s = sec % 60;
  if (h > 0) return `${h} h ${m} min`;
  if (m > 0) return `${m} min${s > 0 ? ` ${s} s` : ''}`;
  return `${s} s`;
}

function fmtDate(iso) {
  const d = new Date(iso + 'T00:00:00');
  return d.toLocaleDateString(undefined, { day: '2-digit', month: '2-digit' });
}

async function init() {
  const [all, settings] = await Promise.all([
    window.statsApi.getAll(),
    window.settingsApi.get(),
  ]);

  const lang = settings.language ?? 'fr-FR';
  const S = STRINGS[lang] ?? STRINGS['fr-FR'];

  document.getElementById('pageTitle').textContent = S.pageTitle;
  document.getElementById('lblToday').textContent  = S.lblToday;
  document.getElementById('lblTotal').textContent  = S.lblTotal;
  document.getElementById('lblHistory').textContent = S.lblHistory;
  document.getElementById('lblExport').textContent  = S.lblExport;
  document.getElementById('hint').textContent       = S.hint;

  // Today stats
  const today = new Date().toISOString().slice(0, 10);
  const todayEntry = (all.history || []).find(e => e.date === today);
  const breaksToday = todayEntry ? todayEntry.breaks : 0;
  const pauseSecToday = todayEntry ? todayEntry.totalPauseSec : 0;
  document.getElementById('countToday').textContent = breaksToday;
  document.getElementById('subToday').textContent   = fmtDuration(pauseSecToday);

  // Total stats
  document.getElementById('countTotal').textContent = all.totalBreaks || 0;
  document.getElementById('subTotal').textContent   = fmtDuration(all.totalPauseSec || 0);

  // History: last 7 days (most recent first)
  const history = [...(all.history || [])]
    .sort((a, b) => b.date.localeCompare(a.date))
    .slice(0, 7);

  const container = document.getElementById('historyContainer');
  if (history.length === 0) {
    document.getElementById('noHistory').textContent = S.noHistory;
  } else {
    document.getElementById('noHistory').remove();
    const table = document.createElement('table');
    table.className = 'history-table';
    table.innerHTML = `
      <thead>
        <tr>
          <th>${S.colDate}</th>
          <th style="text-align:right">${S.colBreaks}</th>
          <th style="text-align:right">${S.colDuration}</th>
        </tr>
      </thead>
    `;
    const tbody = document.createElement('tbody');
    for (const entry of history) {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${fmtDate(entry.date)}</td>
        <td>${entry.breaks}</td>
        <td>${fmtDuration(entry.totalPauseSec)}</td>
      `;
      tbody.appendChild(tr);
    }
    table.appendChild(tbody);
    container.appendChild(table);
  }

  // Export buttons
  document.getElementById('exportCsv').addEventListener('click', async () => {
    await window.statsApi.export('csv');
  });
  document.getElementById('exportJson').addEventListener('click', async () => {
    await window.statsApi.export('json');
  });
}

document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape') window.close();
});

init();
