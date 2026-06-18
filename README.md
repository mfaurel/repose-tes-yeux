# Repose Tes Yeux

![Build](../../actions/workflows/build.yml/badge.svg)

> *"Repose tes yeux"* — French for **"Rest your eyes"**

A lightweight Windows tray application that reminds you to apply the **règle des 20/20/20** :

> Toutes les **20 minutes**, faites une pause de **20 secondes** pour regarder quelque chose à au moins **20 mètres**.

Aucun droit administrateur requis. Un seul `.exe`, aucune installation.

---

## Pourquoi cette règle ?

Fixer un écran force vos yeux à accommoder en permanence à courte distance. Avec le temps, cela provoque fatigue, sécheresse et douleurs oculaires — la **fatigue visuelle numérique**. La règle des 20/20/20 donne à vos muscles oculaires le relâchement dont ils ont besoin.

---

## Fonctionnalités

- **Rappel automatique** — une notification discrète apparaît en bas à droite de chaque écran à l'échéance
- **Entièrement paramétrable** — intervalle, durée, distance, message, langue
- **Icône dans la barre des tâches** — compteur temps restant, menu contextuel complet
- **Démarrage automatique avec Windows** — via la clé de registre `HKCU` (sans droits admin)
- **Mode strict ou souple** — pause obligatoire ou bouton "Passer"
- **Plage "Ne pas déranger"** — aucune interruption sur le créneau horaire de votre choix
- **Sons optionnels** — signal au début et à la fin de chaque pause
- **Statistiques du jour** — nombre de pauses effectuées
- **Multi-moniteurs** — l'overlay recouvre tous les écrans simultanément
- **Instance unique** — un seul processus à la fois, même si l'exe est lancé deux fois
- **Deux langues incluses** — français (par défaut) et anglais

---

## Prérequis

| Composant | Version minimale |
|---|---|
| Windows | 10 (64-bit) ou 11 |
| .NET Desktop Runtime | 8.0 ([télécharger](https://dotnet.microsoft.com/download/dotnet/8.0)) |

Aucun droit administrateur n'est nécessaire ni à l'installation ni à l'exécution.

---

## Utilisation

Voir aussi [INSTALLATION.md](INSTALLATION.md) pour un guide complet incluant la publication de release et les instructions par OS.

1. Télécharger `ReposeTesYeux.exe` depuis les [Actions CI](../../actions/workflows/build.yml) (artefact `ReposeTesYeux-win-x64`) ou depuis les [Releases](../../releases)
2. Double-cliquer pour lancer — l'icône apparaît dans la barre système
3. Le minuteur démarre immédiatement (intervalle par défaut : 20 minutes)
4. À l'échéance, une notification apparaît en bas à droite de chaque écran

### Menu contextuel (clic droit sur l'icône)

| Option | Action |
|---|---|
| Mettre en pause / Reprendre | Suspend ou reprend le minuteur |
| Pause maintenant | Déclenche immédiatement une pause |
| Paramètres… | Ouvre le panneau de configuration |
| Statistiques | Affiche le nombre de pauses du jour |
| Quitter | Ferme l'application |

---

## Paramètres

Tous les paramètres sont sauvegardés dans `%APPDATA%\ReposeTesYeux\settings.json`.

| Paramètre | Défaut | Description |
|---|---|---|
| `workIntervalMinutes` | `20` | Durée de travail entre deux pauses (1–120 min) |
| `breakDurationSeconds` | `20` | Durée de la pause (5–300 s) |
| `distanceMetres` | `20` | Distance recommandée affichée sur l'overlay (1–100 m) |
| `overlayMessage` | *(vide)* | Message personnalisé ; laissez vide pour le message par défaut |
| `language` | `fr-FR` | Langue de l'interface (`fr-FR` ou `en-GB`) |
| `launchAtStartup` | `false` | Lancement automatique au démarrage de Windows |
| `overlayDismissible` | `true` | Affiche un bouton "Passer" sur l'overlay |
| `soundEnabled` | `true` | Sons au début et à la fin de chaque pause |
| `doNotDisturbStart` | *(vide)* | Début de la plage "Ne pas déranger" (format `HH:mm`) |
| `doNotDisturbEnd` | *(vide)* | Fin de la plage "Ne pas déranger" (format `HH:mm`) |

Exemple de fichier de configuration :

```json
{
  "workIntervalMinutes": 25,
  "breakDurationSeconds": 20,
  "distanceMetres": 20,
  "overlayMessage": "Regarde par la fenêtre !",
  "language": "fr-FR",
  "launchAtStartup": true,
  "overlayDismissible": false,
  "soundEnabled": true,
  "doNotDisturbStart": "12:00",
  "doNotDisturbEnd": "13:30"
}
```

---

## Déploiement sans installation (portable)

L'exe est **autonome** — copiez-le où vous voulez, sur une clé USB ou un répertoire réseau, et lancez-le. Les paramètres sont toujours écrits dans `%APPDATA%` de l'utilisateur courant.

Pour un déploiement en masse (parc informatique), distribuez simplement l'exe via votre outil habituel. Chaque utilisateur peut configurer ses propres préférences sans toucher aux préférences des autres.

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

### Créer une release GitHub

La release publie automatiquement les fichiers suivants sur GitHub via GitHub Actions :

| Fichier | Description |
|---|---|
| `ReposeTesYeux-Portable-x.y.z.exe` | **Télécharger et lancer directement** — aucune installation |
| `ReposeTesYeux-Setup-x.y.z.exe` | Installeur silencieux (NSIS) |
| `ReposeTesYeux-x.y.z-x64.dmg` | macOS Intel |
| `ReposeTesYeux-x.y.z-arm64.dmg` | macOS Apple Silicon |
| `ReposeTesYeux-x.y.z-x64.AppImage` | Linux |

**Étapes pour publier :**

1. Mettez à jour la version dans `electron-app/package.json` :

```json
{ "version": "1.2.3" }
```

2. Committez ce changement :

```powershell
git add electron-app/package.json
git commit -m "chore: bump electron version to 1.2.3"
```

3. Créez et poussez un tag `electron-vX.Y.Z` :

```powershell
git tag electron-v1.2.0
git push origin electron-v1.2.0
```

GitHub Actions déclenche alors le workflow **Electron — Release** qui :
- Crée la release GitHub avec les notes de version
- Compile l'app en parallèle sur Windows, macOS et Linux
- Attache tous les fichiers à la release automatiquement

La release apparaît sur la page [Releases](../../releases) du dépôt. Les utilisateurs Windows téléchargent `ReposeTesYeux-Portable-x.y.z.exe` et le lancent directement, sans installation.

> **Vérifier l'avancement** — onglet [Actions](../../actions/workflows/electron-release.yml) du dépôt.

---

## Développement

### Prérequis

- .NET 8 SDK (`winget install Microsoft.DotNet.SDK.8`)

### Compiler

```powershell
dotnet build ReposeTesYeux/ReposeTesYeux.csproj
```

### Lancer

```powershell
dotnet run --project ReposeTesYeux/ReposeTesYeux.csproj
```

### Tests

```powershell
dotnet test ReposeTestYeux.Tests/ReposeTestYeux.Tests.csproj
```

45 tests automatisés couvrent le moteur de minuterie, la persistance des paramètres et l'internationalisation.

### Publier un exe portable

```powershell
dotnet publish ReposeTesYeux/ReposeTesYeux.csproj -c Release -r win-x64 --self-contained false
```

L'exe se trouve ensuite dans `ReposeTesYeux/bin/Release/net8.0-windows/win-x64/publish/`.

---

## Architecture

```
ReposeTesYeux/
├── Timer/
│   ├── EyeTimer.cs         — machine à états (Idle → Working → Break)
│   ├── IClock.cs           — abstraction d'horloge (injectable pour les tests)
│   └── TimerState.cs       — énumération des états
├── Settings/
│   ├── AppSettings.cs      — modèle de configuration typé
│   └── SettingsStore.cs    — lecture/écriture JSON dans %APPDATA%
├── Startup/
│   └── StartupManager.cs   — clé de registre HKCU (sans droits admin)
├── I18n/
│   └── Strings.cs          — chaînes localisées fr-FR / en-GB
└── UI/
    ├── OverlayForm.cs       — fenêtre plein écran affichée pendant la pause
    ├── SettingsForm.cs      — panneau de configuration
    ├── StatsForm.cs         — statistiques du jour
    └── TrayController.cs   — icône et menu de la barre système

ReposeTesYeux.Tests/
├── EyeTimerTests.cs        — 21 tests (transitions, pause, Ne-pas-déranger…)
├── SettingsStoreTests.cs   — 8 tests (valeurs par défaut, JSON corrompu, clamping…)
├── AppSettingsTests.cs     — 9 tests (WithDefaults, clamping, DND window)
├── StringsTests.cs         — 5 tests (sélection de langue, fallback, clé inconnue)
└── FakeClock.cs            — horloge simulée pour les tests déterministes
```

---

## Roadmap

Voir [ROADMAP.md](ROADMAP.md) pour les fonctionnalités prévues.

---

## Licence

MIT — libre d'utilisation, de modification et de distribution.
