# Installation & Release Guide — Repose Tes Yeux

## Sommaire

- [Publier une release](#publier-une-release)
  - [Version WinForms (.NET)](#version-winforms-net)
  - [Version Electron (multi-plateforme)](#version-electron-multi-plateforme)
- [Installer l'application](#installer-lapplication)
  - [Version WinForms](#version-winforms)
  - [Version Electron](#version-electron)

---

## Publier une release

Les releases se déclenchent automatiquement via GitHub Actions en **poussant un tag Git**.  
Il n'y a rien à faire sur l'interface GitHub — le workflow crée la release et attache les fichiers.
Ex :
  git tag v1.3.0
  git push origin v1.3.0

### Version WinForms (.NET)

Produit : `ReposeTesYeux.exe` (Windows uniquement, nécessite .NET 8 Runtime)

```bash
# 1. S'assurer que le code est propre et committé
git status

# 2. Créer le tag (convention : v1.2.3)
git tag v1.0.0

# 3. Pousser le tag — déclenche le workflow release.yml
git push origin v1.0.0
```

Le workflow `.github/workflows/release.yml` va alors :
1. Restaurer et tester le projet
2. Publier un `.exe` autonome (`--self-contained false`, taille réduite)
3. Créer la release GitHub avec le binaire en pièce jointe

### Version Electron (multi-plateforme)

Produit : installeur Windows, `.dmg` macOS, `.AppImage` Linux

```bash
# 1. S'assurer que le code est propre et committé
git status

# 2. Créer le tag (convention : electron-v1.2.3)
git tag electron-v1.0.0

# 3. Pousser le tag — déclenche le workflow electron-release.yml
git push origin electron-v1.0.0
```

Le workflow `.github/workflows/electron-release.yml` va alors :
1. Créer la release GitHub (vide)
2. Builder en parallèle sur Windows, macOS et Linux
3. Attacher tous les artefacts à la release

> **Conseil :** Suivre la progression des jobs dans l'onglet **Actions** du dépôt GitHub.  
> En cas d'échec sur une plateforme, les autres continuent (`fail-fast: false`).

---

## Installer l'application

### Version WinForms

**Prérequis :** [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installé sur Windows.

1. Aller sur la page [Releases](../../releases) du dépôt
2. Télécharger `ReposeTesYeux.exe` depuis la dernière release `v*`
3. Lancer le fichier — aucune installation requise
4. Une icône apparaît dans la barre des tâches systray
5. La règle 20/20/20 démarre automatiquement

### Version Electron

Pas de prérequis — l'application embarque son propre runtime.

| Plateforme | Fichier à télécharger | Instructions |
|---|---|---|
| Windows (portable) | `ReposeTesYeux-Portable-*.exe` | Télécharger et lancer directement |
| Windows (installeur) | `ReposeTesYeux-Setup-*.exe` | Double-cliquer pour installer |
| macOS Intel | `ReposeTesYeux-*-x64.dmg` | Ouvrir le `.dmg` et glisser dans Applications |
| macOS Apple Silicon | `ReposeTesYeux-*-arm64.dmg` | Ouvrir le `.dmg` et glisser dans Applications |
| Linux | `ReposeTesYeux-*-x64.AppImage` | `chmod +x ReposeTesYeux-*.AppImage` puis lancer |

1. Aller sur la page [Releases](../../releases) du dépôt
2. Choisir la dernière release `electron-v*`
3. Télécharger le fichier correspondant à votre OS
4. Lancer **Repose Tes Yeux** — une icône apparaît dans la barre système
5. Clic droit sur l'icône → **Tester le rappel maintenant** pour vérifier que tout fonctionne

> **Windows — avertissement SmartScreen**  
> L'application n'est pas signée numériquement. Windows peut afficher "Windows a protégé votre ordinateur".  
> Cliquer **Informations complémentaires** → **Exécuter quand même** pour continuer.

---

### Version Electron — lancement via script (Windows, sans .exe)

Alternative sans avertissement SmartScreen, à partir du code source.

**Prérequis :** [Node.js](https://nodejs.org/) installé (v18 ou supérieur recommandé).

1. Télécharger le dépôt (bouton **Code → Download ZIP** sur GitHub, ou `git clone`)
2. Décompresser si besoin
3. Double-cliquer sur **`Lancer.bat`** à la racine du projet

Le script installe les dépendances si nécessaire et démarre l'application.  
Une icône apparaît dans la barre système — clic droit → **Tester le rappel maintenant** pour vérifier.

---

## Résumé des conventions de tags

| Tag | Workflow déclenché | Artefact produit |
|---|---|---|
| `v1.0.0` | `release.yml` | `ReposeTesYeux.exe` (Windows, nécessite .NET 8) |
| `electron-v1.0.0` | `electron-release.yml` | Installeurs Windows + macOS + Linux |
