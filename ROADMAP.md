# Roadmap — Repose Tes Yeux

Les items sont classés par priorité décroissante au sein de chaque section.  
Les contributions sont les bienvenues — ouvrir une issue avant de commencer un item majeur.

---

## v2.x — UX & accessibilité

- [ ] **Statistiques globales cumulés** - Calcul de statistiques  
- [ ] "Mettre en pause" à renommer en "Arreter temporairement" car confusion et renommer "Pause maintenant" à "Faire une pause"
- [ ] **Raccourcis clavier globaux** — `Ctrl+Alt+P` pour pause/reprise, `Ctrl+Alt+B` pour break immédiat, configurables dans les paramètres (hook clavier bas niveau via `RegisterHotKey`)
- [ ] **Animations d'ouverture/fermeture** — fondu entrant/sortant de l'overlay (fade-in 300 ms, fade-out 200 ms) ; option de désactivation pour les utilisateurs sensibles aux animations
- [ ] **Exercices guidés** — affichage d'un exercice oculaire ou d'étirement aléatoire pendant la pause (bibliothèque d'exercices configurable, extensible via fichier JSON externe)
- [ ] **Thème clair / sombre** — détection du thème Windows (`UxTheme`) et application d'un schéma de couleurs cohérent sur l'overlay et le formulaire de paramètres
- [ ] **Rappels de posture** — variante de pause courte dédiée à la posture (dos, poignets) ; message et fréquence indépendants de la règle 20/20/20
- [ ] **Export des statistiques** — export CSV ou JSON de l'historique complet depuis `StatsForm` (date, nombre de pauses, durée cumulée)
- [ ] **Notifications Toast natives** — migration des balloon tips vers l'API Windows Toast (`Windows.UI.Notifications`) pour un rendu moderne sur Windows 10/11
- [ ] **Prévoir une pause plus longue** toutes les x pauses
- [ ] Synchroniser les pauses pour que la dernière pause arrive à 11h55 (configurable) en comptant les temps de pause pour aller manger.

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
