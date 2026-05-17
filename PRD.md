# PRD — Repose Tes Yeux

## Problem Statement

People who spend long hours in front of a computer screen are at risk of digital eye strain (fatigue oculaire numérique). The 20/20/20 rule is a simple, evidence-based technique to reduce this strain: every 20 minutes, look at something at least 20 metres away for 20 seconds. However, when focused on work, users forget to apply it. There is no lightweight, no-admin-rights Windows tool that quietly reminds them at the right moment and gets out of the way.

## Solution

**Repose Tes Yeux** is a lightweight Windows desktop application that runs in the system tray without requiring administrator privileges. It tracks screen time and, at a configurable interval (default: 20 minutes), displays a prominent break overlay prompting the user to look into the distance for a configurable duration (default: 20 seconds). All timing, messaging, and behaviour parameters are fully configurable through a simple settings panel. The app can optionally start automatically with Windows using the current user's registry (`HKCU`), requiring no elevated permissions.

## User Stories

1. As a computer user, I want to be reminded every 20 minutes to look away from my screen, so that I can reduce digital eye strain without having to remember myself.
2. As a computer user, I want a visual overlay to appear on my screen when it is time to take a break, so that I cannot miss the reminder even when I am focused on work.
3. As a computer user, I want the overlay to display a countdown of the remaining break time, so that I know exactly when I can return to my work.
4. As a computer user, I want the overlay to show the 20/20/20 rule explanation in plain language, so that I understand why I am being asked to pause.
5. As a computer user, I want to be able to dismiss or skip a break reminder, so that I can finish a critical thought before pausing.
6. As a computer user, I want the app to resume the timer automatically after each break, so that I do not have to manually restart it.
7. As a user with specific needs, I want to configure the work interval duration (e.g. 15, 20, 25, 30 minutes), so that I can adapt the rule to my personal workflow.
8. As a user with specific needs, I want to configure the break duration (e.g. 10, 20, 30 seconds), so that I can take longer or shorter pauses as needed.
9. As a user with specific needs, I want to configure the recommended viewing distance shown in the overlay (e.g. 6 metres, 20 metres), so that the app is relevant to my physical environment.
10. As a user with specific needs, I want to configure the reminder message displayed on the overlay, so that the message resonates with me personally.
11. As a user who works in different languages, I want to choose the display language of the app, so that instructions and messages are in my preferred language.
12. As a user who starts my computer frequently, I want the app to launch automatically at Windows startup, so that eye-care protection is always active without manual effort.
13. As a user who does not have administrator rights on my workstation, I want the app to install and run without requiring elevated privileges, so that I can use it in a corporate environment.
14. As a user who does not want to be disturbed during presentations or video calls, I want to pause the timer temporarily, so that the overlay does not appear at an embarrassing moment.
15. As a user who wants control, I want to be able to skip the current break countdown early, so that I can return to work when I am ready.
16. As a user who wants control, I want to be able to skip to the next break early (trigger a break now), so that I can take an unscheduled pause.
17. As a user who multitasks, I want the app to live in the system tray without occupying the taskbar, so that it does not clutter my workspace.
18. As a user, I want to see from the tray icon how much time is left before the next break, so that I can be mentally prepared.
19. As a user, I want to access settings, pause, skip, and quit actions from the tray icon's context menu, so that I can control the app without opening a separate window.
20. As a user, I want my settings to be saved automatically and persisted across restarts, so that I do not have to reconfigure the app each time I open it.
21. As a user who has an ultrawide or multi-monitor setup, I want the break overlay to cover all my screens, so that I am not tempted to keep working on a secondary monitor.
22. As a user who wants a gentle nudge rather than a hard block, I want to configure whether the overlay is dismissible or mandatory, so that I can choose a strict or lenient mode.
23. As a user who likes statistics, I want to see how many breaks I have taken today, so that I can track my adherence to the 20/20/20 rule.
24. As a user who finds sound helpful, I want an optional audio chime when a break starts and ends, so that I am alerted even when my screen is not in view.
25. As a user who finds sound distracting, I want to mute the audio notifications, so that my work environment is not disrupted.
26. As a user, I want a "do not disturb" schedule (e.g. never remind between 12:00 and 13:00), so that the app respects my lunch break or planned away time.
27. As an IT administrator, I want the app to be deployable as a portable executable with no installation step, so that it can be distributed to users without a software-installation workflow.

## Implementation Decisions

### Module: Timer Engine
- Pure, UI-independent state machine with states: `IDLE`, `WORKING`, `BREAK`, `PAUSED`.
- Exposes: `start()`, `pause()`, `resume()`, `skip()`, `triggerBreakNow()`, `reset()`.
- Emits events: `onBreakStart`, `onBreakEnd`, `onTick(remainingMs)`.
- All durations (work interval, break duration) are injected at construction time from the Settings Store, making the engine fully testable without real timers (injectable clock).
- Transitions are deterministic; no side-effects inside this module.

### Module: Settings Store
- Reads/writes a JSON config file located at `%APPDATA%\ReposeTesYeux\settings.json` — no admin rights required.
- Schema includes: `workIntervalMinutes`, `breakDurationSeconds`, `distanceMetres`, `overlayMessage`, `language`, `launchAtStartup`, `overlayDismissible`, `soundEnabled`, `doNotDisturbStart`, `doNotDisturbEnd`, `strictMode`.
- Provides typed defaults for every field so the app works out-of-the-box with no config file present.
- Validates values on read; falls back to defaults on corruption.

### Module: Notification Overlay
- A full-screen, always-on-top window (one per monitor) rendered during the break phase.
- Displays: configurable message, countdown timer, recommended distance, a progress bar.
- In `strictMode=false`: shows a "Skip" button that fires `Timer.skip()`.
- In `strictMode=true`: no dismiss button; overlay closes automatically when break ends.
- Overlay appearance (background colour, font size, opacity) can be configured.

### Module: System Tray Controller
- Persistent tray icon showing time-to-next-break (tooltip and optional badge).
- Context menu items: **Pause / Resume**, **Take a break now**, **Settings…**, **Quit**.
- Delegates all actions to the Timer Engine and Settings Store.

### Module: Startup Manager
- Reads/writes `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` to register or unregister the app at login.
- No UAC prompt; operates entirely within the current user's registry hive.
- Called only when the `launchAtStartup` setting changes.

### Module: Settings UI
- A modal window (or tray-opened panel) presenting all configurable fields.
- Changes are applied and persisted immediately on confirm; no restart required except for startup registration.

### Cross-cutting: Internationalisation (i18n)
- All user-visible strings are externalised into locale resource files.
- Initial locales: `fr-FR` (primary), `en-GB`.
- Locale is selected from the `language` setting; falls back to `fr-FR`.

### Deployment
- Single portable `.exe` (no installer, no DLL dependencies outside the .NET/WinForms runtime that ships with Windows 10+).
- Target: .NET 8 WinForms (ships inbox on Windows 11; downloadable redistributable for Windows 10).

## Testing Decisions

**What makes a good test here:** tests should exercise observable behaviour through the public interface of each module, not the internal state or private methods. For the Timer Engine in particular, tests should inject a fake clock so they run in milliseconds rather than real minutes.

**Modules to test:**

- **Timer Engine** — highest priority. Test all state transitions: start → working → break → working cycle; pause/resume during work and during break; skip break; triggerBreakNow; do-not-disturb window suppresses break.
- **Settings Store** — test: default values when no file exists; round-trip write/read; corrupt file falls back to defaults; schema validation rejects out-of-range values.
- **Startup Manager** — test: registry key is written on enable; key is removed on disable; idempotent calls do not throw.

**Modules not tested with automated tests (UI/integration):**
- Notification Overlay, System Tray Controller, Settings UI — these are thin shells around the tested core modules and are best validated manually or with screenshot/snapshot tests if a UI testing framework is adopted later.

## Out of Scope

- macOS / Linux support.
- Cloud sync of settings or statistics.
- Integration with calendar apps (e.g. automatically pausing during a Teams meeting).
- Accessibility features beyond standard OS font scaling.
- A mobile companion app.
- Gamification or streak tracking beyond a daily break counter.
- Network or telemetry features of any kind.

## Further Notes

- The app must never require a UAC elevation prompt at any point: installation, first run, settings change, or startup registration.
- The overlay should not interfere with full-screen games or video players if the user is in leisure mode (future enhancement: detect full-screen exclusive mode and skip break).
- The name "Repose Tes Yeux" is French-first; the default language should be `fr-FR` but the entire UI must be translatable.
- The 20/20/20 rule defaults (20 min / 20 sec / 20 m) should be pre-filled but clearly labelled as configurable so users know they are not fixed.
