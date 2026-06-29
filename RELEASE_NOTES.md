# Release Notes — Repose Tes Yeux

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
| .NET Desktop Runtime | 8.0 |

Aucun droit administrateur requis.
