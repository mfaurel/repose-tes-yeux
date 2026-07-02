# Repose Tes Yeux

![Build](../../actions/workflows/electron-build.yml/badge.svg)

> *"Repose tes yeux"* — French for **"Rest your eyes"**

Une application légère dans la barre des tâches Windows qui vous rappelle d'appliquer la **règle des 20/20/20** :

> Toutes les **20 minutes**, faites une pause de **20 secondes** pour regarder quelque chose à au moins **20 mètres**.

Aucun droit administrateur requis. Un seul `.exe`, aucune installation.

---

## Pourquoi cette règle ?

Fixer un écran force vos yeux à accommoder en permanence à courte distance. Avec le temps, cela provoque fatigue, sécheresse et douleurs oculaires — la **fatigue visuelle numérique**. La règle des 20/20/20 donne à vos muscles oculaires le relâchement dont ils ont besoin.

---

## Fonctionnalités

- **Rappel automatique** — overlay plein écran sur tous les moniteurs à l'échéance
- **Entièrement paramétrable** — intervalle, durée, distance, message, langue
- **Icône dans la barre des tâches** — compteur temps restant, menu contextuel complet
- **Démarrage automatique avec Windows** — sans droits admin
- **Mode strict ou souple** — pause obligatoire ou bouton « Passer »
- **Plage « Ne pas déranger »** — aucune interruption sur le créneau horaire de votre choix
- **Sons optionnels** — signal au début et à la fin de chaque pause
- **Raccourcis clavier globaux** — `Ctrl+Alt+P` (arrêt/reprise) et `Ctrl+Alt+B` (pause immédiate), configurables
- **Exercices guidés** — exercice oculaire ou d'étirement affiché aléatoirement à chaque pause (bibliothèque extensible)
- **Thème clair / sombre** — détection automatique du thème Windows, ou choix manuel
- **Grande pause** — pause longue automatique toutes les N pauses courtes
- **Heure de fin de journée** — ajustement automatique de l'intervalle pour finir à l'heure cible
- **Rappels de posture** — notification Toast périodique indépendante des pauses oculaires
- **Statistiques cumulées** — historique par jour avec export CSV / JSON
- **Animations** — fondu entrant et sortant sur l'overlay (200–300 ms)
- **Multi-moniteurs** — l'overlay recouvre tous les écrans simultanément
- **Instance unique** — un seul processus à la fois
- **Deux langues incluses** — français (par défaut) et anglais

---

## Prérequis

| Composant | Version minimale |
|---|---|
| Windows | 10 (64-bit) ou 11 |

Aucun droit administrateur n'est nécessaire ni à l'installation ni à l'exécution. L'application Electron embarque son propre runtime.

---


### Menu contextuel (clic droit sur l'icône)

| Option | Action |
|---|---|
| Arrêter temporairement / Reprendre | Suspend ou reprend le minuteur (`Ctrl+Alt+P`) |
| Faire une pause | Déclenche immédiatement une pause (`Ctrl+Alt+B`) |
| Paramètres… | Ouvre le panneau de configuration |
| Statistiques | Affiche l'historique des pauses |
| Quitter | Ferme l'application |

---

## Paramètres

Tous les paramètres sont sauvegardés dans `%APPDATA%\Electron\repose-tes-yeux\settings.json` 

| Paramètre | Défaut | Description |
|---|---|---|
| `workIntervalMinutes` | `20` | Durée de travail entre deux pauses (1–120 min) |
| `breakDurationSeconds` | `20` | Durée de la pause (5–300 s) |
| `distanceMetres` | `20` | Distance recommandée affichée sur l'overlay |
| `overlayMessage` | *(vide)* | Message personnalisé ; vide = message par défaut |
| `language` | `fr-FR` | Langue (`fr-FR` ou `en-GB`) |
| `launchAtStartup` | `false` | Lancement automatique au démarrage de Windows |
| `overlayDismissible` | `true` | Affiche le bouton « Passer » |
| `soundEnabled` | `true` | Sons au début et à la fin de chaque pause |
| `doNotDisturbStart` | *(vide)* | Début de la plage Ne pas déranger (`HH:MM`) |
| `doNotDisturbEnd` | *(vide)* | Fin de la plage Ne pas déranger (`HH:MM`) |
| `longBreakEvery` | `0` | Grande pause toutes les N pauses (0 = désactivé) |
| `longBreakDurationSeconds` | `300` | Durée de la grande pause (s) |
| `postureReminderMinutes` | `0` | Rappel de posture toutes les N minutes (0 = désactivé) |
| `endOfDayTarget` | *(vide)* | Heure cible de fin de session (`HH:MM`) |
| `shortcutPause` | `Ctrl+Alt+P` | Raccourci arrêt/reprise |
| `shortcutBreak` | `Ctrl+Alt+B` | Raccourci pause immédiate |
| `exercisesEnabled` | `true` | Afficher un exercice pendant la pause |
| `theme` | `auto` | Thème : `auto` / `dark` / `light` |


---

## Publier une release Electron (overlay plein écran)

L'application Electron se trouve dans `electron-app/`. Elle produit un overlay toujours au premier plan, visible par-dessus les applications plein écran et sur tous les moniteurs.

### Prérequis

- [Node.js 20+](https://nodejs.org/) et npm

### Lancer en développement

```powershell
cd electron-app
npm install
npm start
```

Clic droit sur l'icône de la barre système → **Tester le rappel maintenant** pour déclencher l'overlay sans attendre 20 minutes.

### Tests

```powershell
cd electron-app
npm test
```

### Créer une release GitHub

**Étapes pour publier :**

1. Mettez à jour la version dans `electron-app/package.json` :

```json
{ "version": "2.1.0" }
```

2. Committez ce changement :

```powershell
git add electron-app/package.json
git commit -m "chore: bump electron version to 2.1.0"
```

3. Créez et poussez un tag `electron-vX.Y.Z` :

```powershell
git tag electron-v2.1.0
git push origin electron-v2.1.0
```

GitHub Actions déclenche alors le workflow **Electron — Release** qui :
- Crée la release GitHub avec les notes de version
- Compile l'app en parallèle sur Windows, macOS et Linux
- Attache tous les fichiers à la release automatiquement

La release apparaît sur la page [Releases](../../releases) du dépôt. Les utilisateurs Windows téléchargent `ReposeTesYeux-Portable-x.y.z.exe` et le lancent directement, sans installation.

> **Vérifier l'avancement** — onglet [Actions](../../actions/workflows/electron-release.yml) du dépôt.

---

## Architecture

### Variante Electron (v2.x — active)

```
electron-app/
├── main.js              — processus principal : minuterie, tray, IPC, raccourcis globaux
├── preload.js           — pont context-bridge entre main et renderers
├── exercises.json       — bibliothèque d'exercices (œil + étirement), extensible
├── src/
│   ├── i18n.js          — chaînes localisées fr-FR / en-GB
│   ├── settings.js      — lecture/écriture JSON dans %APPDATA%
│   └── stats.js         — statistiques persistantes par jour
└── renderer/
    ├── index.html       — overlay plein écran (pause)
    ├── renderer.js      — logique overlay : countdown, exercices, animations
    ├── styles.css        — thème dark/light, animations fade
    ├── settings.html    — fenêtre de configuration
    ├── settings-renderer.js
    ├── settings.css
    ├── stats.html       — fenêtre statistiques (historique + export)
    └── stats-renderer.js
```

---

## Licence

[MIT](LICENSE) — libre d'utilisation, de modification et de distribution.
