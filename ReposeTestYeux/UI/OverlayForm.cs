using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;

namespace ReposeTesYeux.UI;

public class OverlayForm : Form
{
    private readonly AppSettings _settings;
    private readonly System.Windows.Forms.Timer _uiTimer;
    private TimeSpan _remaining;

    private Label _messageLabel = null!;
    private Label _instructionLabel = null!;
    private Label _countdownLabel = null!;
    private ProgressBar _progressBar = null!;
    private Button? _skipButton;

    public event Action? SkipRequested;

    public OverlayForm(AppSettings settings, TimeSpan initialRemaining)
    {
        _settings = settings;
        _remaining = initialRemaining;

        BuildUI();
        UpdateDisplay();

        _uiTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _uiTimer.Tick += OnUiTick;
        _uiTimer.Start();
    }

    private void BuildUI()
    {
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(20, 30, 48);
        ShowInTaskbar = false;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = _settings.OverlayDismissible ? 5 : 4,
            ColumnCount = 1,
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _messageLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 48, FontStyle.Bold),
            ForeColor = Color.FromArgb(230, 240, 255),
            BackColor = Color.Transparent,
        };

        _instructionLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 20),
            ForeColor = Color.FromArgb(180, 200, 230),
            BackColor = Color.Transparent,
        };

        _countdownLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 64, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 200, 255),
            BackColor = Color.Transparent,
        };

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = _settings.BreakDurationSeconds,
            Value = _settings.BreakDurationSeconds,
            Style = ProgressBarStyle.Continuous,
            Height = 12,
        };

        var ruleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14, FontStyle.Italic),
            ForeColor = Color.FromArgb(120, 150, 180),
            BackColor = Color.Transparent,
            Text = Strings.Get("overlay_rule"),
        };

        panel.Controls.Add(_messageLabel, 0, 0);
        panel.Controls.Add(_instructionLabel, 0, 1);
        panel.Controls.Add(_countdownLabel, 0, 2);
        panel.Controls.Add(_progressBar, 0, 3);

        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));

        if (_settings.OverlayDismissible)
        {
            _skipButton = new Button
            {
                Text = Strings.Get("overlay_skip"),
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(180, 200, 230),
                BackColor = Color.FromArgb(40, 60, 90),
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Anchor = AnchorStyles.None,
            };
            _skipButton.FlatAppearance.BorderColor = Color.FromArgb(100, 130, 180);
            _skipButton.Click += (_, _) => SkipRequested?.Invoke();

            var skipPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
            };
            skipPanel.Controls.Add(_skipButton);

            panel.Controls.Add(skipPanel, 0, 4);
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        }

        Controls.Add(panel);
    }

    private void UpdateDisplay()
    {
        var message = string.IsNullOrWhiteSpace(_settings.OverlayMessage)
            ? Strings.Get("overlay_default_message")
            : _settings.OverlayMessage;

        _messageLabel.Text = message;
        _instructionLabel.Text = string.Format(
            Strings.Get("overlay_instruction"),
            _settings.DistanceMetres,
            _settings.BreakDurationSeconds);
        _countdownLabel.Text = string.Format(Strings.Get("overlay_countdown"), (int)_remaining.TotalSeconds);
        _progressBar.Value = Math.Max(0, Math.Min(_progressBar.Maximum, (int)_remaining.TotalSeconds));
    }

    public void UpdateRemaining(TimeSpan remaining)
    {
        _remaining = remaining;
        if (InvokeRequired)
            Invoke(UpdateDisplay);
        else
            UpdateDisplay();
    }

    public void Close(bool timerExpired)
    {
        _uiTimer.Stop();
        if (InvokeRequired)
            Invoke(Close);
        else
            Close();
    }

    private void OnUiTick(object? sender, EventArgs e)
    {
        if (_remaining.TotalSeconds > 0)
        {
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            UpdateDisplay();
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _uiTimer.Stop();
        _uiTimer.Dispose();
        base.OnFormClosed(e);
    }
}
