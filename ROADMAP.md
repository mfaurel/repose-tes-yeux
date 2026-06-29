# Roadmap — Repose Tes Yeux

Les items sont classés par priorité décroissante au sein de chaque section.  
Les contributions sont les bienvenues — ouvrir une issue avant de commencer un item majeur.

---

## v2.x — UX & accessibilité

- [x] **Statistiques globales cumulés** - Historique persistant par jour, total toutes périodes, fenêtre stats redessinée (fenêtre 480×520)
- [x] "Mettre en pause" renommé en "Arrêter temporairement" et "Pause maintenant" renommé en "Faire une pause"
- [x] **Raccourcis clavier globaux** — `Ctrl+Alt+P` pour pause/reprise, `Ctrl+Alt+B` pour break immédiat, configurables dans les paramètres (`globalShortcut` Electron)
- [x] **Animations d'ouverture/fermeture** — fondu entrant 300 ms / fondu sortant 200 ms sur l'overlay ; classe `.fading-out` CSS
- [x] **Exercices guidés** — exercice oculaire ou d'étirement aléatoire affiché pendant la pause (`exercises.json` extensible)
- [x] **Thème clair / sombre** — détection du thème Windows via `nativeTheme` + `@media (prefers-color-scheme)` ; option manuelle (auto / sombre / clair) dans les paramètres
- [x] **Rappels de posture** — notification Toast native périodique (intervalle en minutes, configurable, désactivable)
- [x] **Export des statistiques** — export CSV ou JSON depuis la fenêtre Statistiques via `dialog.showSaveDialog`
- [x] **Notifications Toast natives** — `electron.Notification` pour les rappels de posture (API Windows Toast sur Windows 10/11)
- [x] **Prévoir une pause plus longue** toutes les x pauses — paramètres `longBreakEvery` + `longBreakDurationSeconds`, tray icon vert pendant grande pause
- [x] Synchroniser les pauses pour que la dernière pause arrive à l'heure cible — paramètre `endOfDayTarget` (HH:MM), ajustement dynamique de l'intervalle de travail

---

## v3.x — Multi-plateforme & distribution

- [ ] **Intégration agenda** — skip automatique des pauses pendant les plages occupées (Outlook / Google Calendar via API)
- [ ] **Convergence WinForms / Electron** — uniformiser les fonctionnalités entre les deux variantes ou choisir une architecture unifiée (Avalonia UI est un bon candidat pour un portage natif multi-plateforme)
- [ ] **Déploiement MSIX** — package Microsoft Store pour une distribution sans droits admin en entreprise ; nécessite un `.wapproj` et la signature de code
- [ ] **Support macOS natif** — menu-bar app en Swift/SwiftUI ou portage Avalonia ; notification `NSUserNotification` / `UNUserNotificationCenter`
- [ ] **Support Linux natif** — daemon systemd + notification D-Bus (`notify-send` / `libnotify`) pour la variante Electron ou portage Avalonia
- [ ] **Package Winget / Scoop** — publication dans les dépôts `winget-pkgs` et `scoop-extras` pour une installation en une ligne
- [ ] **CI/CD GitHub Actions** — build, tests, packaging MSIX et publication de release automatisés sur push de tag `v*`

---

## v3.x — Intégrations & productivité

- [ ] **Mode Pomodoro** — cycle travail/pause court (25 min / 5 min) alternatif à la règle 20/20/20, activable par profil

---

## Idées à l'étude (pas encore planifiées)

- **Mode paire** — synchroniser les pauses entre plusieurs collègues sur le même réseau local (broadcast UDP ou serveur central léger)
- **Gamification** — système de streaks et badges pour encourager le respect des pauses, visible dans `StatsForm`
- **Mode kiosque entreprise** — déploiement silencieux avec paramètres verrouillés via GPO ou fichier de config centralisé
