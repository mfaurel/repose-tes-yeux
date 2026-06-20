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
- [x] **Démarrage réduit dans le systray** — aucune fenêtre au démarrage, l'app démarre directement dans la barre des tâches ; `LaunchAtStartup` activé par défaut (clé registre `HKCU\...\Run` synchronisée à chaque lancement)
- [x] **Pause plus longue tous les N arrêts** — pause longue configurable (durée, message, fréquence) avec message d'étirement
- [x] **Pause de fin de journée** — rappel configurable après N heures d'activité cumulées

---

## v2.0 — Fonctionnalités avancées

- [x] **Changement d'icône dans la barre des tâches** — icône œil anti-aliasée 32×16 px avec iris et pupille, rendu 32×32 réduit à 16×16 HQ
- [x] **Détection d'inactivité** — `IdleDetector` via `GetLastInputInfo` ; timer suspendu si inactif ≥ N minutes, reprise avec intervalle de travail remis à zéro
- [x] **Profils de travail** — `ProfileStore` (profiles.json) + sous-menu systray « Profils » avec bascule, création et suppression
- [x] **Statistiques hebdomadaires / mensuelles** — graphique GDI+ 28 jours dans `StatsForm` + totaux hebdo/mensuel
- [x] **Overlay adaptatif** — fond d'écran via `SystemParametersInfo(SPI_GETDESKWALLPAPER)` teinté sombre en arrière-plan de l'overlay
- [x] **Mode présentateur** — `PresenterModeDetector` via `SHQueryUserNotificationState` ; pauses différées 30 s tant que le mode présentation est actif
- [x] **Prévenir** — info-bulle système (balloon tip) 5 min avant la pause (délai configurable) ; `BreakWarningEnabled` + `BreakWarningMinutes`
- [x] **Transparence** — opacité de l'overlay configurable de 30 % à 100 % via `OverlayOpacityPercent` (défaut 95 %)
- [x] **Mise à jour automatique** — `UpdateChecker` vérifie les releases GitHub au démarrage et notifie si une version plus récente existe
- [x] **CLI** — `ReposeTesYeux --status`, `--break-now`, `--pause`, `--version` via named pipe `\\.\pipe\ReposeTesYeux`
- [ ] **Intégration agenda** — skip automatique des pauses pendant les plages occupées (Outlook / Google Calendar via API)

---

## v2.x — UX & accessibilité

- [ ] **Raccourcis clavier globaux** — `Ctrl+Alt+P` pour pause/reprise, `Ctrl+Alt+B` pour break immédiat, configurables dans les paramètres (hook clavier bas niveau via `RegisterHotKey`)
- [ ] **Animations d'ouverture/fermeture** — fondu entrant/sortant de l'overlay (fade-in 300 ms, fade-out 200 ms) ; option de désactivation pour les utilisateurs sensibles aux animations
- [ ] **Exercices guidés** — affichage d'un exercice oculaire ou d'étirement aléatoire pendant la pause (bibliothèque d'exercices configurable, extensible via fichier JSON externe)
- [ ] **Thème clair / sombre** — détection du thème Windows (`UxTheme`) et application d'un schéma de couleurs cohérent sur l'overlay et le formulaire de paramètres
- [ ] **Rappels de posture** — variante de pause courte dédiée à la posture (dos, poignets) ; message et fréquence indépendants de la règle 20/20/20
- [ ] **Export des statistiques** — export CSV ou JSON de l'historique complet depuis `StatsForm` (date, nombre de pauses, durée cumulée)
- [ ] **Notifications Toast natives** — migration des balloon tips vers l'API Windows Toast (`Windows.UI.Notifications`) pour un rendu moderne sur Windows 10/11

---

## v3.x — Multi-plateforme & distribution

- [ ] **Convergence WinForms / Electron** — uniformiser les fonctionnalités entre les deux variantes ou choisir une architecture unifiée (Avalonia UI est un bon candidat pour un portage natif multi-plateforme)
- [ ] **Déploiement MSIX** — package Microsoft Store pour une distribution sans droits admin en entreprise ; nécessite un `.wapproj` et la signature de code
- [ ] **Support macOS natif** — menu-bar app en Swift/SwiftUI ou portage Avalonia ; notification `NSUserNotification` / `UNUserNotificationCenter`
- [ ] **Support Linux natif** — daemon systemd + notification D-Bus (`notify-send` / `libnotify`) pour la variante Electron ou portage Avalonia
- [ ] **Package Winget / Scoop** — publication dans les dépôts `winget-pkgs` et `scoop-extras` pour une installation en une ligne
- [ ] **CI/CD GitHub Actions** — build, tests, packaging MSIX et publication de release automatisés sur push de tag `v*`

---

## v3.x — Intégrations & productivité

- [ ] **Webhook sortant** — appel HTTP configurable à chaque début/fin de pause (intégration Slack, Teams, Zapier, scripts personnalisés)
- [ ] **Plugin VS Code** — extension affichant un bandeau discret dans l'éditeur et synchronisant l'état pause avec l'instance systray via le pipe CLI
- [ ] **API REST locale** — serveur HTTP minimal (`http://localhost:PORT/status`, `/break-now`) en complément du pipe nommé, pour une intégration avec des outils tiers (scripts, tableaux de bord)
- [ ] **Mode Pomodoro** — cycle travail/pause court (25 min / 5 min) alternatif à la règle 20/20/20, activable par profil
- [ ] **Intégration Garmin Connect / Apple Health** — export des pauses comme activité « repos » vers les API de bien-être (nécessite un intermédiaire cloud ou une app mobile companion)

---

## Idées à l'étude (pas encore planifiées)

- **Mode paire** — synchroniser les pauses entre plusieurs collègues sur le même réseau local (broadcast UDP ou serveur central léger)
- **Tableau de bord web personnel** — statistiques longue durée accessibles depuis un navigateur (backend minimal, données stockées localement)
- **Détection de visage** — suspension automatique si la webcam ne détecte plus de visage devant l'écran (option vie privée : traitement local uniquement, aucune image transmise)
- **Gamification** — système de streaks et badges pour encourager le respect des pauses, visible dans `StatsForm`
- **Plugin JetBrains** — équivalent du plugin VS Code pour Rider / IntelliJ / WebStorm
- **Mode kiosque entreprise** — déploiement silencieux avec paramètres verrouillés via GPO ou fichier de config centralisé
