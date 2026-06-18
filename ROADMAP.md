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

- [ ] **Changement d'icône dans la barre des tâches** — avoir un œil bleu plutôt qu'un carré jaune (icône statique de départ)
- [ ] **Détection d'inactivité** — suspendre le timer si la souris et le clavier sont inactifs depuis N minutes (l'utilisateur est déjà en pause)
- [ ] **Profils de travail** — plusieurs configurations nommées (ex. "Bureau", "Visioconférence") commutables depuis le menu systray
- [ ] **Statistiques hebdomadaires / mensuelles** — graphique de respect de la règle 20/20/20 dans `StatsForm`
- [ ] **Intégration agenda** — skip automatique des pauses pendant les plages occupées du calendrier (Outlook / Google Calendar via API)
- [ ] **Overlay adaptatif** — fond de l'overlay calqué sur l'image de fond d'écran (ambiance naturelle) pour réduire le stress visuel
- [ ] **Mode présentateur** — détecter la mise en miroir d'écran ou le mode présentation Windows et suspendre automatiquement

---

## v2.x — Multi-plateforme & distribution

- [ ] **Convergence WinForms / Electron** — uniformiser les fonctionnalités entre les deux variantes ou choisir une architecture unifiée
- [ ] **Mise à jour automatique** — intégrer `electron-updater` (Electron) et un mécanisme équivalent pour la variante WinForms
- [ ] **Déploiement MSIX** — package Microsoft Store pour une distribution sans droits admin en entreprise
- [ ] **Support Linux natif** — daemon systemd + notification D-Bus pour la variante Electron
- [ ] **CLI** — interface en ligne de commande (`repose-tes-yeux --status`, `--break-now`) pour automatisation et scripts

---

## Idées à l'étude (pas encore planifiées)

- Intégration avec des services de bien-être (Garmin Connect, Apple Health via iCloud…)
- Mode paire — synchroniser les pauses entre plusieurs collègues
- Plugin VS Code / JetBrains — rappel intégré directement dans l'IDE
- Tableau de bord web personnel (statistiques longue durée)
