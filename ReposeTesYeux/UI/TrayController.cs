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
    private AppSettings _settings;

    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;
    private ToolStripMenuItem _pauseResumeItem = null!;

    private readonly List<OverlayForm> _overlays = new();

    // Dynamic icon tracking
    private int _lastIconValue = -1;
    private bool _lastIconIsBreak;
    private IntPtr _lastIconHandle = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    public TrayController(EyeTimer timer, AppSettings settings, SettingsStore store, StartupManager startupManager, BreakHistory history)
    {
        _timer = timer;
        _settings = settings;
        _store = store;
        _startupManager = startupManager;
        _history = history;

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
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        _pauseResumeItem = new ToolStripMenuItem(Strings.Get("menu_pause"));
        _pauseResumeItem.Click += (_, _) => TogglePauseResume();

        var breakNowItem = new ToolStripMenuItem(Strings.Get("menu_break_now"));
        breakNowItem.Click += (_, _) => _timer.TriggerBreakNow();

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
            settingsItem,
            statsItem,
            new ToolStripSeparator(),
            quitItem,
        });

        return menu;
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
    }

    private void OnTick(TimeSpan remaining)
    {
        var formatted = remaining.TotalHours >= 1
            ? remaining.ToString(@"h\:mm\:ss")
            : remaining.ToString(@"m\:ss");

        _trayIcon.Text = _timer.State == TimerState.Break
            ? string.Format(Strings.Get("tray_tooltip_break"), formatted)
            : string.Format(Strings.Get("tray_tooltip_working"), formatted);

        UpdateCountdownIcon(remaining);
    }

    private void OnBreakStart(BreakKind kind)
    {
        _history.Increment();

        var (message, instruction, duration) = BuildBreakContent(kind);

        foreach (var screen in Screen.AllScreens)
        {
            var overlay = new OverlayForm(_settings.OverlayDismissible, duration, screen, message, instruction);
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
        var form = new SettingsForm(_settings, _store, _startupManager);
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
        using var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            var bgColor = isBreak ? Color.FromArgb(220, 120, 20) : Color.FromArgb(30, 120, 220);
            g.FillEllipse(new SolidBrush(bgColor), 0, 0, 15, 15);

            var text = value.ToString();
            float fontSize = value >= 10 ? 5.5f : 7.5f;
            using var font = new Font("Arial", fontSize, FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, Brushes.White, new RectangleF(0, 0, 16, 16), sf);
        }

        SwapIcon(bmp.GetHicon());
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

    private static Icon BuildStaticIcon()
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.FillEllipse(Brushes.DodgerBlue, 1, 4, 14, 8);
        g.FillEllipse(Brushes.White, 4, 5, 8, 6);
        g.FillEllipse(Brushes.DarkBlue, 5, 6, 5, 4);
        g.FillEllipse(Brushes.White, 6, 6, 2, 2);
        return Icon.FromHandle(bmp.GetHicon());
    }

    private static IntPtr BuildStaticIconHandle()
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.FillEllipse(Brushes.DodgerBlue, 1, 4, 14, 8);
        g.FillEllipse(Brushes.White, 4, 5, 8, 6);
        g.FillEllipse(Brushes.DarkBlue, 5, 6, 5, 4);
        g.FillEllipse(Brushes.White, 6, 6, 2, 2);
        return bmp.GetHicon();
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
