using System.Media;
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
    private AppSettings _settings;

    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;
    private ToolStripMenuItem _pauseResumeItem = null!;

    private readonly List<OverlayForm> _overlays = new();

    public TrayController(EyeTimer timer, AppSettings settings, SettingsStore store, StartupManager startupManager)
    {
        _timer = timer;
        _settings = settings;
        _store = store;
        _startupManager = startupManager;

        _menu = BuildMenu();
        _trayIcon = new NotifyIcon
        {
            Icon = BuildIcon(),
            Text = Strings.Get("app_name"),
            ContextMenuStrip = _menu,
            Visible = true,
        };

        _timer.OnStateChanged += OnStateChanged;
        _timer.OnTick += OnTick;
        _timer.OnBreakStart += ShowOverlays;
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
    }

    private void ShowOverlays()
    {
        foreach (var screen in Screen.AllScreens)
        {
            var overlay = new OverlayForm(_settings, TimeSpan.FromSeconds(_settings.BreakDurationSeconds), screen);
            overlay.SkipRequested += () => _timer.SkipBreak();
            _overlays.Add(overlay);
            overlay.Show();
        }

        if (_settings.SoundEnabled)
            SystemSounds.Exclamation.Play();

        // Wire tick to update overlays
        _timer.OnTick += UpdateOverlays;
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

        if (_settings.SoundEnabled)
            SystemSounds.Asterisk.Play();

        foreach (var o in _overlays.ToList())
        {
            if (!o.IsDisposed)
                o.Invoke(o.Close);
        }
        _overlays.Clear();
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
        new StatsForm(_timer.BreaksToday).ShowDialog();
    }

    private static Icon BuildIcon()
    {
        // Create a simple programmatic icon (eye symbol)
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.FillEllipse(Brushes.DodgerBlue, 1, 4, 14, 8);
        g.FillEllipse(Brushes.White, 4, 5, 8, 6);
        g.FillEllipse(Brushes.DarkBlue, 5, 6, 5, 4);
        g.FillEllipse(Brushes.White, 6, 6, 2, 2);
        return Icon.FromHandle(bmp.GetHicon());
    }

    public void Dispose()
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _menu.Dispose();
    }
}
