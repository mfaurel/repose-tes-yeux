using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;
using ReposeTesYeux.Startup;

namespace ReposeTesYeux.UI;

public class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly SettingsStore _store;
    private readonly StartupManager _startupManager;
    private readonly ProfileStore _profileStore;

    private NumericUpDown _workIntervalInput = null!;
    private NumericUpDown _breakDurationInput = null!;
    private NumericUpDown _distanceInput = null!;
    private TextBox _messageInput = null!;
    private ComboBox _languageInput = null!;
    private CheckBox _startupInput = null!;
    private CheckBox _dismissibleInput = null!;
    private CheckBox _soundInput = null!;
    private TextBox _customSoundInput = null!;
    private TextBox _dndStartInput = null!;
    private TextBox _dndEndInput = null!;
    private CheckBox _longBreakEnabledInput = null!;
    private NumericUpDown _longBreakEveryInput = null!;
    private NumericUpDown _longBreakDurationInput = null!;
    private TextBox _longBreakMessageInput = null!;
    private CheckBox _endOfDayEnabledInput = null!;
    private NumericUpDown _endOfDayHoursInput = null!;
    private CheckBox _inactivityEnabledInput = null!;
    private NumericUpDown _inactivityThresholdInput = null!;
    private CheckBox _presenterModeInput = null!;
    private CheckBox _adaptiveOverlayInput = null!;
    private NumericUpDown _overlayOpacityInput = null!;
    private CheckBox _breakWarningEnabledInput = null!;
    private NumericUpDown _breakWarningMinutesInput = null!;
    private CheckBox _autoUpdateEnabledInput = null!;

    public event Action<AppSettings>? SettingsSaved;

    public SettingsForm(AppSettings settings, SettingsStore store, StartupManager startupManager, ProfileStore profileStore)
    {
        _settings = settings;
        _store = store;
        _startupManager = startupManager;
        _profileStore = profileStore;
        BuildUI();
    }

    private void BuildUI()
    {
        Text = Strings.Get("settings_title");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Width = 460;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        BackColor = Color.FromArgb(245, 247, 250);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(8),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        int row = 0;

        // ── General ───────────────────────────────────────────────────────────
        _workIntervalInput = new NumericUpDown { Minimum = 1, Maximum = 120, Value = _settings.WorkIntervalMinutes, Width = 80 };
        AddRow(layout, row++, "settings_work_interval", _workIntervalInput);

        _breakDurationInput = new NumericUpDown { Minimum = 5, Maximum = 300, Value = _settings.BreakDurationSeconds, Width = 80 };
        AddRow(layout, row++, "settings_break_duration", _breakDurationInput);

        _distanceInput = new NumericUpDown { Minimum = 1, Maximum = 100, Value = _settings.DistanceMetres, Width = 80 };
        AddRow(layout, row++, "settings_distance", _distanceInput);

        _messageInput = new TextBox { Text = _settings.OverlayMessage, Width = 160 };
        AddRow(layout, row++, "settings_message", _messageInput);

        _languageInput = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        _languageInput.Items.AddRange(new object[] { "fr-FR", "en-GB", "es-ES", "de-DE" });
        _languageInput.SelectedItem = _settings.Language;
        if (_languageInput.SelectedIndex < 0) _languageInput.SelectedIndex = 0;
        AddRow(layout, row++, "settings_language", _languageInput);

        _startupInput = new CheckBox { Checked = _settings.LaunchAtStartup, AutoSize = true };
        AddRow(layout, row++, "settings_startup", _startupInput);

        _dismissibleInput = new CheckBox { Checked = _settings.OverlayDismissible, AutoSize = true };
        AddRow(layout, row++, "settings_dismissible", _dismissibleInput);

        // ── Sound ─────────────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        _soundInput = new CheckBox { Checked = _settings.SoundEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_sound", _soundInput);

        var soundRow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        _customSoundInput = new TextBox { Text = _settings.CustomSoundPath, Width = 120, PlaceholderText = "…" };
        var browseBtn = new Button { Text = Strings.Get("settings_browse"), AutoSize = true };
        browseBtn.Click += OnBrowseSound;
        soundRow.Controls.AddRange(new Control[] { _customSoundInput, browseBtn });
        AddRow(layout, row++, "settings_custom_sound", soundRow);

        // ── Do Not Disturb ────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        _dndStartInput = new TextBox { Text = _settings.DoNotDisturbStart, Width = 80, PlaceholderText = "08:00" };
        AddRow(layout, row++, "settings_dnd_start", _dndStartInput);

        _dndEndInput = new TextBox { Text = _settings.DoNotDisturbEnd, Width = 80, PlaceholderText = "09:00" };
        AddRow(layout, row++, "settings_dnd_end", _dndEndInput);

        // ── Long breaks ───────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        _longBreakEnabledInput = new CheckBox { Checked = _settings.LongBreakEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_long_break_enabled", _longBreakEnabledInput);

        _longBreakEveryInput = new NumericUpDown { Minimum = 2, Maximum = 20, Value = _settings.LongBreakEveryN, Width = 80 };
        AddRow(layout, row++, "settings_long_break_every", _longBreakEveryInput);

        _longBreakDurationInput = new NumericUpDown { Minimum = 30, Maximum = 600, Value = _settings.LongBreakDurationSeconds, Width = 80 };
        AddRow(layout, row++, "settings_long_break_duration", _longBreakDurationInput);

        _longBreakMessageInput = new TextBox { Text = _settings.LongBreakMessage, Width = 160 };
        AddRow(layout, row++, "settings_long_break_message", _longBreakMessageInput);

        // ── End of day ────────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        _endOfDayEnabledInput = new CheckBox { Checked = _settings.EndOfDayEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_end_of_day_enabled", _endOfDayEnabledInput);

        _endOfDayHoursInput = new NumericUpDown { Minimum = 1, Maximum = 24, Value = _settings.EndOfDayHours, Width = 80 };
        AddRow(layout, row++, "settings_end_of_day_hours", _endOfDayHoursInput);

        // ── Inactivity & Presenter mode ───────────────────────────────────────
        AddSeparator(layout, row++);

        _inactivityEnabledInput = new CheckBox { Checked = _settings.InactivityDetectionEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_inactivity_enabled", _inactivityEnabledInput);

        _inactivityThresholdInput = new NumericUpDown { Minimum = 1, Maximum = 60, Value = _settings.InactivityThresholdMinutes, Width = 80 };
        AddRow(layout, row++, "settings_inactivity_threshold", _inactivityThresholdInput);

        _presenterModeInput = new CheckBox { Checked = _settings.SuspendInPresenterMode, AutoSize = true };
        AddRow(layout, row++, "settings_presenter_mode", _presenterModeInput);

        _adaptiveOverlayInput = new CheckBox { Checked = _settings.AdaptiveOverlayEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_adaptive_overlay", _adaptiveOverlayInput);

        _overlayOpacityInput = new NumericUpDown { Minimum = 30, Maximum = 100, Value = _settings.OverlayOpacityPercent, Width = 80 };
        AddRow(layout, row++, "settings_overlay_opacity", _overlayOpacityInput);

        // ── Break warning ─────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        _breakWarningEnabledInput = new CheckBox { Checked = _settings.BreakWarningEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_break_warning_enabled", _breakWarningEnabledInput);

        _breakWarningMinutesInput = new NumericUpDown { Minimum = 1, Maximum = 30, Value = _settings.BreakWarningMinutes, Width = 80 };
        AddRow(layout, row++, "settings_break_warning_minutes", _breakWarningMinutesInput);

        // ── Misc ──────────────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        _autoUpdateEnabledInput = new CheckBox { Checked = _settings.AutoUpdateEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_auto_update_enabled", _autoUpdateEnabledInput);

        // ── Buttons ───────────────────────────────────────────────────────────
        AddSeparator(layout, row++);

        var btnPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Dock = DockStyle.Fill,
        };

        var saveBtn = new Button { Text = Strings.Get("settings_save"), DialogResult = DialogResult.None, AutoSize = true };
        saveBtn.Click += OnSave;
        var cancelBtn = new Button { Text = Strings.Get("settings_cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };

        btnPanel.Controls.AddRange(new Control[] { cancelBtn, saveBtn });
        layout.Controls.Add(btnPanel, 0, row);
        layout.SetColumnSpan(btnPanel, 2);

        Controls.Add(layout);
        CancelButton = cancelBtn;
    }

    private static void AddRow(TableLayoutPanel layout, int row, string labelKey, Control control)
    {
        var label = new Label
        {
            Text = Strings.Get(labelKey),
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Padding = new Padding(0, 6, 0, 0),
        };
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static void AddSeparator(TableLayoutPanel layout, int row)
    {
        var sep = new Panel { Height = 1, Dock = DockStyle.Fill, BackColor = Color.FromArgb(210, 215, 225), Margin = new Padding(0, 6, 0, 6) };
        layout.Controls.Add(sep, 0, row);
        layout.SetColumnSpan(sep, 2);
    }

    private void OnBrowseSound(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = Strings.Get("settings_custom_sound"),
            Filter = "Fichiers audio (*.wav)|*.wav|Tous les fichiers (*.*)|*.*",
            CheckFileExists = true,
        };
        if (!string.IsNullOrEmpty(_customSoundInput.Text) && File.Exists(_customSoundInput.Text))
            dialog.InitialDirectory = Path.GetDirectoryName(_customSoundInput.Text);

        if (dialog.ShowDialog() == DialogResult.OK)
            _customSoundInput.Text = dialog.FileName;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        var wasStartup = _settings.LaunchAtStartup;

        _settings.WorkIntervalMinutes = (int)_workIntervalInput.Value;
        _settings.BreakDurationSeconds = (int)_breakDurationInput.Value;
        _settings.DistanceMetres = (int)_distanceInput.Value;
        _settings.OverlayMessage = _messageInput.Text.Trim();
        _settings.Language = _languageInput.SelectedItem?.ToString() ?? "fr-FR";
        _settings.LaunchAtStartup = _startupInput.Checked;
        _settings.OverlayDismissible = _dismissibleInput.Checked;
        _settings.SoundEnabled = _soundInput.Checked;
        _settings.CustomSoundPath = _customSoundInput.Text.Trim();
        _settings.DoNotDisturbStart = _dndStartInput.Text.Trim();
        _settings.DoNotDisturbEnd = _dndEndInput.Text.Trim();
        _settings.LongBreakEnabled = _longBreakEnabledInput.Checked;
        _settings.LongBreakEveryN = (int)_longBreakEveryInput.Value;
        _settings.LongBreakDurationSeconds = (int)_longBreakDurationInput.Value;
        _settings.LongBreakMessage = _longBreakMessageInput.Text.Trim();
        _settings.EndOfDayEnabled = _endOfDayEnabledInput.Checked;
        _settings.EndOfDayHours = (int)_endOfDayHoursInput.Value;
        _settings.InactivityDetectionEnabled = _inactivityEnabledInput.Checked;
        _settings.InactivityThresholdMinutes = (int)_inactivityThresholdInput.Value;
        _settings.SuspendInPresenterMode = _presenterModeInput.Checked;
        _settings.AdaptiveOverlayEnabled = _adaptiveOverlayInput.Checked;
        _settings.OverlayOpacityPercent = (int)_overlayOpacityInput.Value;
        _settings.BreakWarningEnabled = _breakWarningEnabledInput.Checked;
        _settings.BreakWarningMinutes = (int)_breakWarningMinutesInput.Value;
        _settings.AutoUpdateEnabled = _autoUpdateEnabledInput.Checked;

        _store.Save(_settings);

        // Keep the active profile in sync
        if (_profileStore.Contains(_settings.ActiveProfileName))
            _profileStore.Save(_settings.ActiveProfileName, _settings);

        if (_settings.LaunchAtStartup != wasStartup)
            _startupManager.SetLaunchAtStartup(_settings.LaunchAtStartup);

        Strings.SetLanguage(_settings.Language);
        SettingsSaved?.Invoke(_settings);
        Close();
    }
}
