const STRINGS = {
  'fr-FR': {
    pageTitle:   'Statistiques',
    lblToday:    "Aujourd'hui",
    lblTotal:    'Total',
    subToday:    (n, sec) => `${n} pause${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    subTotal:    (n, sec) => `${n} pause${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    lblHistory:  'Historique (7 derniers jours)',
    colDate:     'Date',
    colBreaks:   'Pauses',
    colDuration: 'Durée',
    noHistory:   'Aucune donnée',
    lblExport:   'Exporter :',
    lblChart:    '30 derniers jours',
    lblStreak:   'Jours actifs',
    streakUnit:  (n) => n === 1 ? 'jour' : 'jours',
    lblBadges:   'Badges',
    badgeLang:   'fr',
    hint:        'Appuyez sur Échap pour fermer',
  },
  'en-GB': {
    pageTitle:   'Statistics',
    lblToday:    'Today',
    lblTotal:    'All time',
    subToday:    (n, sec) => `${n} break${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    subTotal:    (n, sec) => `${n} break${n !== 1 ? 's' : ''} · ${fmtDuration(sec)}`,
    lblHistory:  'History (last 7 days)',
    colDate:     'Date',
    colBreaks:   'Breaks',
    colDuration: 'Duration',
    noHistory:   'No data yet',
    lblExport:   'Export:',
    lblChart:    'Last 30 days',
    lblStreak:   'Active days',
    streakUnit:  (n) => n === 1 ? 'day' : 'days',
    lblBadges:   'Badges',
    badgeLang:   'en',
    hint:        'Press Escape to close',
  },
};

function renderChart(history, S) {
  const DAYS = 30;
  const today = new Date();
  const data = [];
  for (let i = DAYS - 1; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const dateStr = d.toISOString().slice(0, 10);
    const entry = (history || []).find(e => e.date === dateStr);
    data.push({ date: dateStr, breaks: entry ? entry.breaks : 0, isToday: i === 0 });
  }

  const W = 440, H = 90;
  const gap = 2;
  const barW = Math.floor((W - gap * (DAYS - 1)) / DAYS);
  const maxVal = Math.max(1, ...data.map(d => d.breaks));

  const svg = document.getElementById('chartSvg');
  svg.innerHTML = '';

  data.forEach((d, i) => {
    const h = d.breaks === 0 ? 2 : Math.max(4, Math.round((d.breaks / maxVal) * H));
    const x = i * (barW + gap);
    const y = H - h;
    const rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    rect.setAttribute('x', x);
    rect.setAttribute('y', y);
    rect.setAttribute('width', barW);
    rect.setAttribute('height', h);
    rect.setAttribute('rx', 2);
    let cls = 'chart-bar';
    if (d.breaks === 0) cls += ' zero';
    else if (d.isToday) cls += ' today';
    rect.setAttribute('class', cls);
    svg.appendChild(rect);
  });

  // Axis labels: first and last date
  const fmt = iso => {
    const [, m, dd] = iso.split('-');
    return `${dd}/${m}`;
  };
  document.getElementById('chartAxisStart').textContent = fmt(data[0].date);
  document.getElementById('chartAxisEnd').textContent   = fmt(data[DAYS - 1].date);
  document.getElementById('lblChart').textContent       = S.lblChart;
}

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
  const [all, settings, gamification] = await Promise.all([
    window.statsApi.getAll(),
    window.settingsApi.get(),
    window.statsApi.getGamification(),
  ]);

  const lang = settings.language ?? 'fr-FR';
  const S = STRINGS[lang] ?? STRINGS['fr-FR'];

  document.getElementById('pageTitle').textContent  = S.pageTitle;
  document.getElementById('lblToday').textContent   = S.lblToday;
  document.getElementById('lblTotal').textContent   = S.lblTotal;
  document.getElementById('lblHistory').textContent = S.lblHistory;
  document.getElementById('lblExport').textContent  = S.lblExport;
  document.getElementById('lblStreak').textContent  = S.lblStreak;
  document.getElementById('lblBadges').textContent  = S.lblBadges;
  document.getElementById('hint').textContent       = S.hint;

  renderChart(all.history, S);

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

  // Streak
  const streak = gamification.streak || 0;
  document.getElementById('streakValue').textContent = streak;
  document.getElementById('streakUnit').textContent  = S.streakUnit(streak);

  // Badges
  const badgeGrid = document.getElementById('badgeGrid');
  const isEn = S.badgeLang === 'en';
  const allBadges = [
    ...(gamification.earned || []).map(b => ({ ...b, earned: true })),
    ...(gamification.locked || []).map(b => ({ ...b, earned: false })),
  ];
  for (const badge of allBadges) {
    const div = document.createElement('div');
    div.className = 'badge-item' + (badge.earned ? '' : ' locked');
    const name = isEn ? badge.label_en : badge.label_fr;
    const desc = isEn ? badge.desc_en  : badge.desc_fr;
    div.innerHTML = `
      <span class="badge-name">${name}</span>
      <span class="badge-desc">${desc}</span>
    `;
    badgeGrid.appendChild(div);
  }
}

document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape') window.close();
});

init();
