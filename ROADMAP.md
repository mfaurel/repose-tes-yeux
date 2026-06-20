# Roadmap — Repose Tes Yeux

Les items sont classés par priorité décroissante au sein de chaque section.  
Les contributions sont les bienvenues — ouvrir une issue avant de commencer un item majeur.

---

## v1.x — Stabilisation & UX

- [x] **Mode strict configurable depuis l'UI** — `overlayDismissible` exposé dans le panneau de paramètres
- [x] **Compte à rebours visible dans la barre des tâches** — icône systray mise à jour chaque minute (minutes restantes) / chaque seconde pendant une pause
- [x] **Notification sonore personnalisable** — sélecteur de fichier `.wav` dans les paramètres, fallback sur le bip système
- [x] **Historique des pauses** — compteur quotidien persisté entre les redémarrages (`%APPDATA%\ReposeTesYeux\history.json`), total hebdomadaire dans les statistiques
- [x] **Localisation supplémentaire** — espagnol (`es-ES`) et allemand (`de-DE`) ajoutés
- [x] **Démarrage réduit dans le systray** — aucune fenêtre au démarrage, l'app démarre directement dans la barre des tâches
- [x] **Pause plus longue tous les N arrêts** — pause longue configurable (durée, message, fréquence) avec message d'étirement
- [x] **Pause de fin de journée** — rappel configurable après N heures d'activité cumulées

---

## v2.0 — Fonctionnalités avancées

- [x] **Changement d'icône dans la barre des tâches** — icône œil anti-aliasée 32×16 px avec iris et pupille, rendu 32×32 réduit à 16×16 HQ
- [x] **Détection d'inactivité** — `IdleDetector` via `GetLastInputInfo` ; timer suspendu si inactif ≥ N minutes, reprise avec intervalle de travail remis à zéro
- [x] **Profils de travail** — `ProfileStore` (profiles.json) + sous-menu systray « Profils » avec bascule, création et suppression
- [x] **Statistiques hebdomadaires / mensuelles** — graphique GDI+ 28 jours dans `StatsForm` + totaux hebdo/mensuel
- [ ] **Intégration agenda** — skip automatique des pauses pendant les plages occupées du calendrier (Outlook / Google Calendar via API)
- [x] **Overlay adaptatif** — fond d'écran via `SystemParametersInfo(SPI_GETDESKWALLPAPER)` teinté sombre en arrière-plan de l'overlay
- [x] **Mode présentateur** — `PresenterModeDetector` via `SHQueryUserNotificationState` ; pauses différées 30 s tant que le mode présentation est actif

---

## v2.x — Multi-plateforme & distribution

- [x] **Prévenir** — info-bulle système (balloon tip) depuis la barre des tâches, 5 min avant la pause (délai configurable) ; clé `BreakWarningEnabled` + `BreakWarningMinutes` dans les paramètres
- [x] **Transparence** — opacité de l'overlay configurable de 30 % à 100 % via `OverlayOpacityPercent` (défaut 95 %)
- [x] **Mise à jour automatique** — `UpdateChecker` vérifie les releases GitHub au démarrage et affiche une info-bulle si une version plus récente existe ; clé `AutoUpdateEnabled`
- [x] **CLI** — `ReposeTesYeux --status`, `--break-now`, `--pause`, `--version`, `--help` via named pipe `\\.\pipe\ReposeTesYeux`
- [ ] **Convergence WinForms / Electron** — uniformiser les fonctionnalités entre les deux variantes ou choisir une architecture unifiée
- [ ] **Déploiement MSIX** — package Microsoft Store pour une distribution sans droits admin en entreprise ; nécessite un `.wapproj` et la signature de code
- [ ] **Support Linux natif** — daemon systemd + notification D-Bus pour la variante Electron

---

## Idées à l'étude (pas encore planifiées)

- Intégration avec des services de bien-être (Garmin Connect, Apple Health via iCloud…)
- Mode paire — synchroniser les pauses entre plusieurs collègues
- Plugin VS Code / JetBrains — rappel intégré directement dans l'IDE
- Tableau de bord web personnel (statistiques longue durée)
