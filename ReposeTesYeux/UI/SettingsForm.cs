using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;
using ReposeTesYeux.Startup;

namespace ReposeTesYeux.UI;

public class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly SettingsStore _store;
    private readonly StartupManager _startupManager;

    private NumericUpDown _workIntervalInput = null!;
    private NumericUpDown _breakDurationInput = null!;
    private NumericUpDown _distanceInput = null!;
    private TextBox _messageInput = null!;
    private ComboBox _languageInput = null!;
    private CheckBox _startupInput = null!;
    private CheckBox _dismissibleInput = null!;
    private CheckBox _soundInput = null!;
    private TextBox _dndStartInput = null!;
    private TextBox _dndEndInput = null!;

    public event Action<AppSettings>? SettingsSaved;

    public SettingsForm(AppSettings settings, SettingsStore store, StartupManager startupManager)
    {
        _settings = settings;
        _store = store;
        _startupManager = startupManager;
        BuildUI();
    }

    private void BuildUI()
    {
        Text = Strings.Get("settings_title");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Width = 440;
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

        int row = 0;

        _workIntervalInput = new NumericUpDown { Minimum = 1, Maximum = 120, Value = _settings.WorkIntervalMinutes, Width = 80 };
        AddRow(layout, row++, "settings_work_interval", _workIntervalInput);

        _breakDurationInput = new NumericUpDown { Minimum = 5, Maximum = 300, Value = _settings.BreakDurationSeconds, Width = 80 };
        AddRow(layout, row++, "settings_break_duration", _breakDurationInput);

        _distanceInput = new NumericUpDown { Minimum = 1, Maximum = 100, Value = _settings.DistanceMetres, Width = 80 };
        AddRow(layout, row++, "settings_distance", _distanceInput);

        _messageInput = new TextBox { Text = _settings.OverlayMessage, Width = 160 };
        AddRow(layout, row++, "settings_message", _messageInput);

        _languageInput = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        _languageInput.Items.AddRange(new object[] { "fr-FR", "en-GB" });
        _languageInput.SelectedItem = _settings.Language;
        if (_languageInput.SelectedIndex < 0) _languageInput.SelectedIndex = 0;
        AddRow(layout, row++, "settings_language", _languageInput);

        _startupInput = new CheckBox { Checked = _settings.LaunchAtStartup, AutoSize = true };
        AddRow(layout, row++, "settings_startup", _startupInput);

        _dismissibleInput = new CheckBox { Checked = _settings.OverlayDismissible, AutoSize = true };
        AddRow(layout, row++, "settings_dismissible", _dismissibleInput);

        _soundInput = new CheckBox { Checked = _settings.SoundEnabled, AutoSize = true };
        AddRow(layout, row++, "settings_sound", _soundInput);

        _dndStartInput = new TextBox { Text = _settings.DoNotDisturbStart, Width = 80, PlaceholderText = "08:00" };
        AddRow(layout, row++, "settings_dnd_start", _dndStartInput);

        _dndEndInput = new TextBox { Text = _settings.DoNotDisturbEnd, Width = 80, PlaceholderText = "09:00" };
        AddRow(layout, row++, "settings_dnd_end", _dndEndInput);

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
        _settings.DoNotDisturbStart = _dndStartInput.Text.Trim();
        _settings.DoNotDisturbEnd = _dndEndInput.Text.Trim();

        _store.Save(_settings);

        if (_settings.LaunchAtStartup != wasStartup)
            _startupManager.SetLaunchAtStartup(_settings.LaunchAtStartup);

        Strings.SetLanguage(_settings.Language);
        SettingsSaved?.Invoke(_settings);
        Close();
    }
}
