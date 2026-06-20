using System.Runtime.InteropServices;
using System.Text;
using ReposeTesYeux.I18n;

namespace ReposeTesYeux.UI;

public class OverlayForm : Form
{
    private const int ToastWidth = 380;
    private const int ToastMargin = 16;

    private readonly int _durationSeconds;
    private readonly bool _dismissible;
    private readonly System.Windows.Forms.Timer _uiTimer;
    private TimeSpan _remaining;

    private Label _messageLabel = null!;
    private Label _instructionLabel = null!;
    private Label _countdownLabel = null!;
    private ProgressBar _progressBar = null!;

    public event Action? SkipRequested;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);
    private const int SPI_GETDESKWALLPAPER = 0x0073;

    public OverlayForm(bool dismissible, int durationSeconds, Screen screen, string message, string instruction, bool adaptiveOverlay = false, double opacity = 0.95)
    {
        _dismissible = dismissible;
        _durationSeconds = durationSeconds;
        _remaining = TimeSpan.FromSeconds(durationSeconds);

        BuildUI(message, instruction, adaptiveOverlay);
        PositionOnScreen(screen);
        UpdateDisplay();
        Opacity = Math.Clamp(opacity, 0.3, 1.0);

        _uiTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _uiTimer.Tick += OnUiTick;
        _uiTimer.Start();
    }

    private void BuildUI(string message, string instruction, bool adaptiveOverlay)
    {
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.FromArgb(22, 32, 50);
        Width = ToastWidth;
        AutoSize = false;

        if (adaptiveOverlay)
            ApplyAdaptiveBackground();

        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = _dismissible ? 5 : 4,
            Padding = new Padding(16, 12, 16, 12),
            BackColor = Color.Transparent,
        };
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var ruleLabel = new Label
        {
            Text = Strings.Get("overlay_rule"),
            AutoSize = true,
            Font = new Font("Segoe UI", 8, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 140, 200),
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 4),
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.Controls.Add(ruleLabel, 0, 0);

        _messageLabel = new Label
        {
            Text = message,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(230, 240, 255),
            BackColor = Color.Transparent,
            Margin = new Padding(0, 2, 0, 2),
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.Controls.Add(_messageLabel, 0, 1);

        _instructionLabel = new Label
        {
            Text = instruction,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 185, 220),
            BackColor = Color.Transparent,
            Margin = new Padding(0, 2, 0, 6),
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.Controls.Add(_instructionLabel, 0, 2);

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = _durationSeconds,
            Value = _durationSeconds,
            Style = ProgressBarStyle.Continuous,
            Height = 6,
            Margin = new Padding(0, 0, 0, 6),
        };
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        outer.Controls.Add(_progressBar, 0, 3);

        if (_dismissible)
        {
            var bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _countdownLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            };
            bottomRow.Controls.Add(_countdownLabel, 0, 0);

            var skipBtn = new Button
            {
                Text = Strings.Get("overlay_skip"),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(160, 185, 220),
                BackColor = Color.FromArgb(38, 55, 82),
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            };
            skipBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 90, 130);
            skipBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 78, 110);
            skipBtn.Click += (_, _) => SkipRequested?.Invoke();
            bottomRow.Controls.Add(skipBtn, 1, 0);

            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.Controls.Add(bottomRow, 0, 4);
        }
        else
        {
            _countdownLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                BackColor = Color.Transparent,
            };
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.Controls.Add(_countdownLabel, 0, 4);
        }

        Controls.Add(outer);
        outer.PerformLayout();
        Height = outer.GetPreferredSize(new Size(ToastWidth, 0)).Height + 4;
    }

    private void ApplyAdaptiveBackground()
    {
        try
        {
            var wallpaperPath = GetWallpaperPath();
            if (string.IsNullOrEmpty(wallpaperPath) || !File.Exists(wallpaperPath))
                return;

            using var wallpaper = Image.FromFile(wallpaperPath);
            var tinted = CreateTintedBackground(wallpaper, ToastWidth, 200);
            BackgroundImage = tinted;
            BackgroundImageLayout = ImageLayout.Stretch;
        }
        catch { }
    }

    private static string? GetWallpaperPath()
    {
        var sb = new StringBuilder(260);
        return SystemParametersInfo(SPI_GETDESKWALLPAPER, 260, sb, 0) ? sb.ToString() : null;
    }

    private static Bitmap CreateTintedBackground(Image source, int width, int height)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.DrawImage(source, 0, 0, width, height);
        using var darkOverlay = new SolidBrush(Color.FromArgb(195, 22, 32, 50));
        g.FillRectangle(darkOverlay, 0, 0, width, height);
        return bmp;
    }

    private void PositionOnScreen(Screen screen)
    {
        var area = screen.WorkingArea;
        Left = area.Right - Width - ToastMargin;
        Top = area.Bottom - Height - ToastMargin;
    }

    private void UpdateDisplay()
    {
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
