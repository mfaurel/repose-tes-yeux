using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;
using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;
using ReposeTesYeux.Startup;
using ReposeTesYeux.Timer;

namespace ReposeTesYeux.UI;

public class TrayController : IDisposable
{
    private readonly EyeTimer _timer;
    private readonly SettingsStore _store;
    private readonly StartupManager _startupManager;
    private readonly BreakHistory _history;
    private readonly ProfileStore _profileStore;
    private AppSettings _settings;

    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;
    private ToolStripMenuItem _pauseResumeItem = null!;
    private ToolStripMenuItem _profilesMenu = null!;

    private readonly List<OverlayForm> _overlays = new();

    // Dynamic icon tracking
    private int _lastIconValue = -1;
    private bool _lastIconIsBreak;
    private IntPtr _lastIconHandle = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    public TrayController(EyeTimer timer, AppSettings settings, SettingsStore store, StartupManager startupManager, BreakHistory history, ProfileStore profileStore)
    {
        _timer = timer;
        _settings = settings;
        _store = store;
        _startupManager = startupManager;
        _history = history;
        _profileStore = profileStore;

        _menu = BuildMenu();
        _trayIcon = new NotifyIcon
        {
            Icon = BuildStaticIcon(),
            Text = Strings.Get("app_name"),
            ContextMenuStrip = _menu,
            Visible = true,
        };

        _timer.OnStateChanged += OnStateChanged;
        _timer.OnTick += OnTick;
        _timer.OnBreakStart += OnBreakStart;
        _timer.OnBreakEnd += CloseOverlays;
        _timer.OnAutoSuspended += OnAutoSuspended;
        _timer.OnAutoResumed += OnAutoResumed;
        _timer.OnBreakWarning += OnBreakWarning;

        if (_settings.AutoUpdateEnabled)
            _ = CheckForUpdateAsync();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        _pauseResumeItem = new ToolStripMenuItem(Strings.Get("menu_pause"));
        _pauseResumeItem.Click += (_, _) => TogglePauseResume();

        var breakNowItem = new ToolStripMenuItem(Strings.Get("menu_break_now"));
        breakNowItem.Click += (_, _) => _timer.TriggerBreakNow();

        _profilesMenu = BuildProfilesMenu();

        var settingsItem = new ToolStripMenuItem(Strings.Get("menu_settings"));
        settingsItem.Click += (_, _) => OpenSettings();

        var statsItem = new ToolStripMenuItem(Strings.Get("menu_stats"));
        statsItem.Click += (_, _) => OpenStats();

        var quitItem = new ToolStripMenuItem(Strings.Get("menu_quit"));
        quitItem.Click += (_, _) => Application.Exit();

        menu.Items.AddRange(new ToolStripItem[]
        {
            _pauseResumeItem,
            breakNowItem,
            new ToolStripSeparator(),
            _profilesMenu,
            new ToolStripSeparator(),
            settingsItem,
            statsItem,
            new ToolStripSeparator(),
            quitItem,
        });

        return menu;
    }

    private ToolStripMenuItem BuildProfilesMenu()
    {
        var profilesItem = new ToolStripMenuItem(Strings.Get("menu_profiles"));
        RefreshProfilesSubmenu(profilesItem);
        profilesItem.DropDownOpening += (_, _) => RefreshProfilesSubmenu(profilesItem);
        return profilesItem;
    }

    private void RefreshProfilesSubmenu(ToolStripMenuItem profilesItem)
    {
        profilesItem.DropDownItems.Clear();

        foreach (var name in _profileStore.Names)
        {
            var item = new ToolStripMenuItem(name)
            {
                Checked = name == _settings.ActiveProfileName,
            };
            var capturedName = name;
            item.Click += (_, _) => SwitchProfile(capturedName);
            profilesItem.DropDownItems.Add(item);
        }

        profilesItem.DropDownItems.Add(new ToolStripSeparator());

        var saveAsItem = new ToolStripMenuItem(Strings.Get("menu_profile_save_as"));
        saveAsItem.Click += (_, _) => SaveCurrentAsProfile();
        profilesItem.DropDownItems.Add(saveAsItem);

        if (_settings.ActiveProfileName != ProfileStore.DefaultProfileKey)
        {
            var deleteItem = new ToolStripMenuItem(Strings.Get("menu_profile_delete"));
            deleteItem.Click += (_, _) => DeleteCurrentProfile();
            profilesItem.DropDownItems.Add(deleteItem);
        }
    }

    private void SwitchProfile(string name)
    {
        var profile = _profileStore.Get(name);
        profile.ActiveProfileName = name;
        _settings = profile;
        _store.Save(_settings);
        _timer.UpdateSettings(_settings);
        Strings.SetLanguage(_settings.Language);
    }

    private void SaveCurrentAsProfile()
    {
        var name = ShowInputDialog(
            Strings.Get("profile_name_prompt"),
            Strings.Get("profile_new_title"),
            _settings.ActiveProfileName);

        if (string.IsNullOrWhiteSpace(name))
            return;

        _settings.ActiveProfileName = name;
        _profileStore.Save(name, _settings);
        _store.Save(_settings);
    }

    private static string? ShowInputDialog(string prompt, string title, string defaultValue)
    {
        using var form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterScreen,
            Width = 360,
            Height = 140,
        };
        var label = new Label { Text = prompt, Left = 16, Top = 16, Width = 320, AutoSize = true };
        var textBox = new TextBox { Text = defaultValue, Left = 16, Top = 40, Width = 312 };
        var okBtn = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 180, Top = 72, Width = 72 };
        var cancelBtn = new Button { Text = "Annuler", DialogResult = DialogResult.Cancel, Left = 260, Top = 72, Width = 72 };
        form.Controls.AddRange(new Control[] { label, textBox, okBtn, cancelBtn });
        form.AcceptButton = okBtn;
        form.CancelButton = cancelBtn;
        return form.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : null;
    }

    private void DeleteCurrentProfile()
    {
        var name = _settings.ActiveProfileName;
        var confirm = MessageBox.Show(
            string.Format(Strings.Get("profile_delete_confirm"), name),
            Strings.Get("app_name"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return;

        _profileStore.Delete(name);
        SwitchProfile(ProfileStore.DefaultProfileKey);
    }

    private void TogglePauseResume()
    {
        if (_timer.State == TimerState.Paused)
        {
            _timer.Resume();
            _pauseResumeItem.Text = Strings.Get("menu_pause");
        }
        else
        {
            _timer.Pause();
            _pauseResumeItem.Text = Strings.Get("menu_resume");
        }
    }

    private void OnStateChanged(TimerState state)
    {
        if (state == TimerState.Paused)
        {
            _trayIcon.Text = Strings.Get("tray_tooltip_paused");
            _pauseResumeItem.Text = Strings.Get("menu_resume");
            SetStaticIcon();
        }
        else if (state == TimerState.Idle)
        {
            SetStaticIcon();
        }
        else if (state == TimerState.Working)
        {
            _pauseResumeItem.Text = Strings.Get("menu_pause");
        }
    }

    private void OnAutoSuspended()
    {
        _trayIcon.Text = _settings.SuspendInPresenterMode && ReposeTesYeux.Timer.PresenterModeDetector.IsPresenting()
            ? Strings.Get("tray_tooltip_presenter")
            : Strings.Get("tray_tooltip_auto_suspended");
        SetStaticIcon();
    }

    private void OnAutoResumed()
    {
        _pauseResumeItem.Text = Strings.Get("menu_pause");
    }

    private void OnTick(TimeSpan remaining)
    {
        if (_timer.IsAutoSuspended)
            return;

        var formatted = remaining.TotalHours >= 1
            ? remaining.ToString(@"h\:mm\:ss")
            : remaining.ToString(@"m\:ss");

        _trayIcon.Text = _timer.State == TimerState.Break
            ? string.Format(Strings.Get("tray_tooltip_break"), formatted)
            : string.Format(Strings.Get("tray_tooltip_working"), formatted);

        UpdateCountdownIcon(remaining);
    }

    private void OnBreakWarning(TimeSpan remaining)
    {
        if (!_settings.BreakWarningEnabled)
            return;
        var minutes = (int)Math.Ceiling(remaining.TotalMinutes);
        _trayIcon.ShowBalloonTip(
            6000,
            Strings.Get("balloon_warning_title"),
            string.Format(Strings.Get("balloon_warning_text"), minutes),
            ToolTipIcon.Info);
    }

    private async Task CheckForUpdateAsync()
    {
        var latest = await UpdateChecker.CheckAsync();
        if (latest is null)
            return;
        _trayIcon.ShowBalloonTip(
            8000,
            Strings.Get("balloon_update_title"),
            string.Format(Strings.Get("balloon_update_text"), latest),
            ToolTipIcon.Info);
    }

    private void OnBreakStart(BreakKind kind)
    {
        _history.Increment();

        var (message, instruction, duration) = BuildBreakContent(kind);
        var opacity = _settings.OverlayOpacityPercent / 100.0;

        foreach (var screen in Screen.AllScreens)
        {
            var overlay = new OverlayForm(_settings.OverlayDismissible, duration, screen, message, instruction, _settings.AdaptiveOverlayEnabled, opacity);
            overlay.SkipRequested += () => _timer.SkipBreak();
            _overlays.Add(overlay);
            overlay.Show();
        }

        PlaySound(isStart: true);
        _timer.OnTick += UpdateOverlays;
    }

    private (string message, string instruction, int durationSeconds) BuildBreakContent(BreakKind kind)
    {
        return kind switch
        {
            BreakKind.Long => (
                string.IsNullOrWhiteSpace(_settings.LongBreakMessage)
                    ? Strings.Get("overlay_long_break_message")
                    : _settings.LongBreakMessage,
                string.Format(Strings.Get("overlay_long_break_instruction"), _settings.LongBreakDurationSeconds),
                _settings.LongBreakDurationSeconds
            ),
            BreakKind.EndOfDay => (
                Strings.Get("overlay_end_of_day_message"),
                string.Format(Strings.Get("overlay_end_of_day_instruction"), _settings.EndOfDayHours),
                _settings.LongBreakDurationSeconds
            ),
            _ => (
                string.IsNullOrWhiteSpace(_settings.OverlayMessage)
                    ? Strings.Get("overlay_default_message")
                    : _settings.OverlayMessage,
                string.Format(Strings.Get("overlay_instruction"), _settings.DistanceMetres, _settings.BreakDurationSeconds),
                _settings.BreakDurationSeconds
            ),
        };
    }

    private void UpdateOverlays(TimeSpan remaining)
    {
        foreach (var o in _overlays)
        {
            if (!o.IsDisposed)
                o.UpdateRemaining(remaining);
        }
    }

    private void CloseOverlays()
    {
        _timer.OnTick -= UpdateOverlays;
        PlaySound(isStart: false);

        foreach (var o in _overlays.ToList())
        {
            if (!o.IsDisposed)
                o.Invoke(o.Close);
        }
        _overlays.Clear();
    }

    private void PlaySound(bool isStart)
    {
        if (!_settings.SoundEnabled)
            return;

        if (!string.IsNullOrEmpty(_settings.CustomSoundPath) && File.Exists(_settings.CustomSoundPath))
        {
            try
            {
                using var player = new SoundPlayer(_settings.CustomSoundPath);
                player.Play();
                return;
            }
            catch { }
        }

        (isStart ? SystemSounds.Exclamation : SystemSounds.Asterisk).Play();
    }

    private void OpenSettings()
    {
        var form = new SettingsForm(_settings, _store, _startupManager, _profileStore);
        form.SettingsSaved += newSettings =>
        {
            _settings = newSettings;
            _timer.UpdateSettings(newSettings);
        };
        form.ShowDialog();
    }

    private void OpenStats()
    {
        new StatsForm(_history).ShowDialog();
    }

    // ── Icon management ────────────────────────────────────────────────────────

    private void UpdateCountdownIcon(TimeSpan remaining)
    {
        bool isBreak = _timer.State == TimerState.Break;
        int value = isBreak
            ? Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds))
            : Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));

        if (value == _lastIconValue && isBreak == _lastIconIsBreak)
            return;

        _lastIconValue = value;
        _lastIconIsBreak = isBreak;
        SetDynamicIcon(value, isBreak);
    }

    private void SetDynamicIcon(int value, bool isBreak)
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var bgColor = isBreak ? Color.FromArgb(220, 120, 20) : Color.FromArgb(30, 120, 220);
            using var bgBrush = new SolidBrush(bgColor);
            g.FillEllipse(bgBrush, 1, 1, 30, 30);

            var text = value.ToString();
            float fontSize = value >= 10 ? 11f : 14f;
            using var font = new Font("Arial", fontSize, FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, Brushes.White, new RectangleF(0, 0, 32, 32), sf);
        }

        // Scale down to 16x16 for tray
        using var icon16 = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(icon16))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(bmp, 0, 0, 16, 16);
        }

        SwapIcon(icon16.GetHicon());
    }

    private void SetStaticIcon()
    {
        _lastIconValue = -1;
        SwapIcon(BuildStaticIconHandle());
    }

    private void SwapIcon(IntPtr hNewIcon)
    {
        var prevHandle = _lastIconHandle;
        _lastIconHandle = hNewIcon;
        _trayIcon.Icon = Icon.FromHandle(hNewIcon);
        if (prevHandle != IntPtr.Zero)
            DestroyIcon(prevHandle);
    }

    private static Icon BuildStaticIcon() => Icon.FromHandle(BuildStaticIconHandle());

    private static IntPtr BuildStaticIconHandle()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // Outer eye shape (white sclera)
            using var scleraBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            g.FillEllipse(scleraBrush, 2, 9, 28, 14);

            // Iris
            using var irisBrush = new SolidBrush(Color.FromArgb(30, 120, 220));
            g.FillEllipse(irisBrush, 10, 8, 14, 14);

            // Pupil
            using var pupilBrush = new SolidBrush(Color.FromArgb(10, 40, 80));
            g.FillEllipse(pupilBrush, 13, 11, 7, 7);

            // Highlight
            g.FillEllipse(Brushes.White, 14, 12, 3, 3);

            // Eye outline
            using var outlinePen = new Pen(Color.FromArgb(20, 80, 160), 1.2f);
            g.DrawEllipse(outlinePen, 2, 9, 28, 14);
        }

        // Scale to 16x16
        using var icon16 = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(icon16))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(bmp, 0, 0, 16, 16);
        }

        return icon16.GetHicon();
    }

    public void Dispose()
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _menu.Dispose();
        if (_lastIconHandle != IntPtr.Zero)
            DestroyIcon(_lastIconHandle);
    }
}
