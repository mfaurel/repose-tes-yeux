# Release Notes — Repose Tes Yeux

---

## v3.1.0 — 2 juillet 2026 — Verrouillage & présentation

### Nouvelles fonctionnalités

- **Pause suspendue si ordinateur verrouillé** — la minuterie est mise en pause automatiquement lorsque l'ordinateur est verrouillé, et aucune pause n'est déclenchée pendant ce temps
- **Reprise automatique au déverrouillage** — le décompte du temps de travail repart dès que l'ordinateur est déverrouillé (sauf si l'utilisateur avait déjà mis l'application en pause manuellement)
- **Statut Teams « En présentation »** — ce statut est désormais traité comme « Ne pas déranger » pour le skip automatique des pauses

---

## v3.0.0 — 29 juin 2026 — Intégrations & engagement

Cette version apporte trois axes majeurs : les notifications avant pause, l'intégration des agendas et de Teams pour éviter les interruptions pendant les réunions, et la gamification pour encourager l'assiduité.

### Nouvelles fonctionnalités

#### Notification avant pause
- **Alerte configurable** — une notification ballon apparaît dans la zone systray N secondes avant la prochaine pause (0 à 60 s, configurable dans la section Minuterie des paramètres)
- Utilise `tray.displayBalloon()` pour s'afficher directement depuis l'icône sans fenêtre

#### Intégration agenda (skip automatique des réunions)
- **Fichier .ics** — lecture d'un fichier iCalendar local (Outlook sync, Google Calendar, etc.) avec décodage RFC 5545 (dates UTC / flottantes / journées entières, événements TRANSPARENT et CANCELLED ignorés)
- **Outlook COM** — interrogation de l'agenda Outlook actif via PowerShell COM sans token OAuth ; détecte toutes les réunions incluant les récurrences
- Si une réunion est en cours au moment d'une pause, celle-ci est ignorée et la minuterie repart

#### Intégration Teams (mode Ne pas déranger)
- **Statut DND Teams** — si le statut Teams de l'utilisateur est « Ne pas déranger », la pause est ignorée et une notification explique le report
- Détection via `%APPDATA%\Microsoft\Teams\storage.json` (Teams classique) avec repli sur l'état Focus Assist Windows (nouveau Teams)

#### Gamification
- **Compteur de jours actifs** — compteur du nombre total de jours où au moins une pause a été effectuée (affiché dans la fenêtre Statistiques)
- **8 badges progressifs** : Premier regard (1ère pause) → Journée active (5 pauses/jour) → Régulier (3 jours) → Assidu (7 jours) → Habitué (30 jours) → Centenaire (100 pauses) → Marathonien (500) → Légende (1 000)
- Les badges non obtenus sont affichés en mode verrouillé pour motiver leur déverrouillage

#### Graphique 30 jours
- **Histogramme** dans la fenêtre Statistiques : une barre par jour sur les 30 derniers jours, la hauteur représentant le nombre de pauses
- La barre du jour en cours est mise en évidence ; les jours sans pause affichent une barre résiduelle

#### Exercices grande pause
- **Bibliothèque longue pause** — 8 nouvelles activités dédiées aux grandes pauses (sortir prendre l'air, tour du bureau, étirement complet, respiration, regard à l'horizon…) remplacent les exercices courts pendant les grandes pauses configurées

### Paramètres ajoutés

| Clé | Type | Défaut | Description |
|---|---|---|---|
| `breakWarningSeconds` | entier | `0` | Secondes avant la pause pour la notification préalable (0 = désactivé) |
| `teamsDndEnabled` | bool | `false` | Activer la détection du statut DND Teams |
| `calendarEnabled` | bool | `false` | Activer la détection de réunion via agenda |
| `calendarType` | string | `ics` | Source de l'agenda : `ics` ou `outlook` |
| `calendarIcsPath` | string | `""` | Chemin vers le fichier .ics local |

---

## v2.1.0 — 29 juin 2026 — Electron : UX & accessibilité

Cette version complète la roadmap v2.x pour la variante Electron. Toutes les fonctionnalités d'accessibilité, de confort et de personnalisation avancées sont désormais disponibles.

### Nouvelles fonctionnalités

#### Exercices guidés
- **Bibliothèque d'exercices** — à chaque pause, un exercice oculaire ou d'étirement est affiché aléatoirement dans l'overlay (8 exercices oculaires + 6 d'étirement)
- **Extensible** — le fichier `exercises.json` peut être modifié pour ajouter ou personnaliser les exercices, en français et en anglais

#### Animations d'interface
- **Fondu entrant** — l'overlay s'ouvre avec un fondu de 300 ms (était 500 ms, plus fluide)
- **Fondu sortant** — fermeture animée en 200 ms, déclenchée aussi bien par expiration du timer que par le bouton « Passer »

#### Pauses avancées
- **Grande pause** — une pause longue (durée configurable, défaut 5 min) peut être déclenchée automatiquement toutes les N pauses courtes ; l'icône systray devient verte pendant une grande pause
- **Heure de fin de journée** — un horaire cible (ex. 11 h 55) peut être configuré ; l'intervalle de travail est ajusté dynamiquement pour que la dernière pause arrive exactement à l'heure souhaitée
- **Rappels de posture** — notification Toast Windows périodique (dos, épaules, poignets) indépendante des pauses oculaires ; fréquence configurable ou désactivable

#### Raccourcis clavier globaux
- **`Ctrl+Alt+P`** — arrêter/reprendre le timer sans ouvrir le systray
- **`Ctrl+Alt+B`** — déclencher une pause immédiatement
- Les deux raccourcis sont personnalisables dans les paramètres (champ texte, format Electron standard)

#### Thème clair / sombre
- Détection automatique du thème Windows via `nativeTheme` + `@media (prefers-color-scheme)`
- Option manuelle dans les paramètres : Automatique / Sombre / Clair
- Appliqué à l'overlay, à la fenêtre de paramètres et à la fenêtre de statistiques

#### Statistiques enrichies
- **Historique persistant** — les pauses sont enregistrées par jour dans `stats.json` (dossier `userData`) ; les données survivent aux redémarrages
- **Fenêtre redessinée** — affichage d'aujourd'hui, du total toutes périodes, et de l'historique des 7 derniers jours
- **Export** — boutons CSV et JSON avec boîte de dialogue de sauvegarde (`dialog.showSaveDialog`)

#### Renommage des libellés
- « Mettre en pause » → **« Arrêter temporairement »** (évite la confusion avec les pauses oculaires)
- « Pause maintenant » → **« Faire une pause »**

### Paramètres ajoutés

| Clé | Type | Défaut | Description |
|---|---|---|---|
| `longBreakEvery` | entier | `0` | Grande pause toutes les N pauses (0 = désactivé) |
| `longBreakDurationSeconds` | entier | `300` | Durée de la grande pause (s) |
| `postureReminderMinutes` | entier | `0` | Rappel de posture toutes les N minutes (0 = désactivé) |
| `endOfDayTarget` | HH:MM | `""` | Heure cible de fin de session |
| `shortcutPause` | string | `Ctrl+Alt+P` | Raccourci arrêt/reprise |
| `shortcutBreak` | string | `Ctrl+Alt+B` | Raccourci pause immédiate |
| `exercisesEnabled` | bool | `true` | Afficher un exercice pendant la pause |
| `theme` | string | `auto` | Thème : `auto` / `dark` / `light` |

---

## v2.0.0 — 29 juin 2026

Cette version apporte les fonctionnalités avancées planifiées dans la roadmap v2.0, en complément des améliorations UX de la série v1.x.

### Nouvelles fonctionnalités

#### Expérience visuelle
- **Icône œil dans la barre des tâches** — icône anti-aliasée 32×16 px avec iris et pupille, rendue en 32×32 réduit à 16×16 HQ
- **Overlay adaptatif** — le fond d'écran Windows est récupéré via `SystemParametersInfo(SPI_GETDESKWALLPAPER)` et teinté sombrement en arrière-plan de l'overlay
- **Transparence configurable** — opacité de l'overlay réglable de 30 % à 100 % via `OverlayOpacityPercent` (défaut 95 %)

#### Automatisation & intelligence
- **Détection d'inactivité** — `IdleDetector` via `GetLastInputInfo` ; le timer se suspend si l'utilisateur est inactif ≥ N minutes et repart à zéro à la reprise
- **Mode présentateur** — `PresenterModeDetector` via `SHQueryUserNotificationState` ; les pauses sont différées de 30 s tant que le mode présentation est actif
- **Mise à jour automatique** — `UpdateChecker` vérifie les releases GitHub au démarrage et notifie si une version plus récente est disponible

#### Personnalisation
- **Profils de travail** — `ProfileStore` (profiles.json) avec sous-menu systray « Profils » : bascule, création et suppression à chaud
- **Avertissement avant pause** — info-bulle système (balloon tip) envoyée N minutes avant la pause ; `BreakWarningEnabled` + `BreakWarningMinutes` configurables

#### Statistiques
- **Statistiques hebdomadaires / mensuelles** — graphique GDI+ sur 28 jours dans `StatsForm` avec totaux hebdo et mensuel

#### Intégration système
- **CLI** — commandes `--status`, `--break-now`, `--pause`, `--version` via named pipe `\\.\pipe\ReposeTesYeux`

---

## v1.x — Stabilisation & UX

### Améliorations apportées

- **Mode strict configurable depuis l'UI** — `overlayDismissible` exposé dans le panneau de paramètres
- **Compte à rebours dans la barre des tâches** — icône systray mise à jour chaque minute (minutes restantes) et chaque seconde pendant une pause
- **Notification sonore personnalisable** — sélecteur de fichier `.wav` dans les paramètres, fallback sur le bip système
- **Historique des pauses** — compteur quotidien persisté entre les redémarrages (`%APPDATA%\ReposeTesYeux\history.json`), total hebdomadaire dans les statistiques
- **Localisation supplémentaire** — espagnol (`es-ES`) et allemand (`de-DE`) ajoutés (en plus du français et de l'anglais)
- **Démarrage réduit dans le systray** — aucune fenêtre à l'ouverture, démarrage direct dans la barre des tâches ; `LaunchAtStartup` activé par défaut via la clé registre `HKCU\...\Run`
- **Pause longue configurable** — pause longue tous les N cycles courts, avec durée, message et fréquence réglables
- **Pause de fin de journée** — rappel configurable déclenché après N heures d'activité cumulées

---

## Compatibilité

| Composant | Version minimale |
|---|---|
| Windows | 10 (64-bit) ou 11 |
| .NET Desktop Runtime | 8.0 (variante WinForms) |
| Electron | 31+ (variante Electron) |

Aucun droit administrateur requis.
