# Repose Tes Yeux

> *"Repose tes yeux"* — French for **"Rest your eyes"**

A lightweight Windows tray application that reminds you to apply the **règle des 20/20/20** :

> Toutes les **20 minutes**, faites une pause de **20 secondes** pour regarder quelque chose à au moins **20 mètres**.

Aucun droit administrateur requis. Un seul `.exe`, aucune installation.

---

## Pourquoi cette règle ?

Fixer un écran force vos yeux à accommoder en permanence à courte distance. Avec le temps, cela provoque fatigue, sécheresse et douleurs oculaires — la **fatigue visuelle numérique**. La règle des 20/20/20 donne à vos muscles oculaires le relâchement dont ils ont besoin.

---

## Fonctionnalités

- **Rappel automatique** — une fenêtre plein écran apparaît à l'échéance, sur tous vos moniteurs
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

1. Télécharger `ReposeTesYeux.exe` depuis les [Releases](../../releases)
2. Double-cliquer pour lancer — l'icône apparaît dans la barre système
3. Le minuteur démarre immédiatement (intervalle par défaut : 20 minutes)
4. À l'échéance, l'overlay s'affiche sur tous vos écrans

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

## Développement

### Prérequis

- .NET 8 SDK (`winget install Microsoft.DotNet.SDK.8`)

### Compiler

```powershell
dotnet build ReposeTestYeux/ReposeTestYeux.csproj
```

### Lancer

```powershell
dotnet run --project ReposeTestYeux/ReposeTestYeux.csproj
```

### Tests

```powershell
dotnet test ReposeTestYeux.Tests/ReposeTestYeux.Tests.csproj
```

26 tests automatisés couvrent le moteur de minuterie et la persistance des paramètres.

### Publier un exe portable

```powershell
dotnet publish ReposeTestYeux/ReposeTestYeux.csproj -c Release -r win-x64 --self-contained false
```

L'exe se trouve ensuite dans `ReposeTestYeux/bin/Release/net8.0-windows/win-x64/publish/`.

---

## Architecture

```
ReposeTestYeux/
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

ReposeTestYeux.Tests/
├── EyeTimerTests.cs        — 18 tests (transitions, pause, Ne-pas-déranger…)
├── SettingsStoreTests.cs   — 8 tests (valeurs par défaut, JSON corrompu, clamping…)
└── FakeClock.cs            — horloge simulée pour les tests déterministes
```

---

## Licence

MIT — libre d'utilisation, de modification et de distribution.
