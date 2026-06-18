# Roadmap — Repose Tes Yeux

Les items sont classés par priorité décroissante au sein de chaque section.  
Les contributions sont les bienvenues — ouvrir une issue avant de commencer un item majeur.

---

## v1.x — Stabilisation & UX

- [ ] **Mode strict configurable depuis l'UI** — exposer le champ `overlayDismissible` dans le panneau de paramètres (actuellement persisté en JSON mais absent du formulaire)
- [ ] **Compte à rebours visible dans la barre des tâches** — mettre à jour l'icône systray avec le temps restant (ex. overlay numérique ou arc de progression)
- [ ] **Notification sonore personnalisable** — permettre à l'utilisateur de choisir un fichier audio `.wav` plutôt que le bip système
- [ ] **Historique des pauses** — persister le compteur quotidien entre les redémarrages (`%APPDATA%\ReposeTesYeux\history.json`)
- [ ] **Localisation supplémentaire** — ajouter l'espagnol (`es-ES`) et l'allemand (`de-DE`) à `Strings.cs`
- [ ] **Démarrage réduit dans le systray** — ne pas afficher de fenêtre au démarrage, démarrer directement dans la barre des tâches

---

## v2.0 — Fonctionnalités avancées

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
