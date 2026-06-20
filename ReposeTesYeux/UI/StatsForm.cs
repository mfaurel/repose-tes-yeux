using System.Drawing.Drawing2D;
using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;

namespace ReposeTesYeux.UI;

public class StatsForm : Form
{
    public StatsForm(BreakHistory history)
    {
        Text = Strings.Get("stats_title");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Width = 440;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(20);
        BackColor = Color.FromArgb(245, 247, 250);

        var layout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Width = 400,
            Padding = new Padding(4),
        };

        layout.Controls.Add(new Label
        {
            Text = string.Format(Strings.Get("stats_breaks_today"), history.GetToday()),
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.FromArgb(30, 80, 180),
            Margin = new Padding(0, 0, 0, 2),
        });

        layout.Controls.Add(new Label
        {
            Text = string.Format(Strings.Get("stats_breaks_week"), history.GetWeekTotal()),
            Font = new Font("Segoe UI", 11),
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2),
        });

        layout.Controls.Add(new Label
        {
            Text = string.Format(Strings.Get("stats_breaks_month"), history.GetMonthTotal()),
            Font = new Font("Segoe UI", 11),
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12),
        });

        var chartLabel = new Label
        {
            Text = Strings.Get("stats_chart_label"),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 100, 130),
            Margin = new Padding(0, 0, 0, 4),
        };
        layout.Controls.Add(chartLabel);

        var chart = new BreakBarChart(history.GetLastNDays(28))
        {
            Width = 400,
            Height = 120,
            Margin = new Padding(0, 0, 0, 16),
        };
        layout.Controls.Add(chart);

        var closeBtn = new Button
        {
            Text = Strings.Get("stats_close"),
            DialogResult = DialogResult.OK,
            AutoSize = true,
        };
        layout.Controls.Add(closeBtn);

        Controls.Add(layout);
        AcceptButton = closeBtn;
    }

    private class BreakBarChart : Panel
    {
        private readonly IReadOnlyDictionary<DateOnly, int> _data;

        public BreakBarChart(IReadOnlyDictionary<DateOnly, int> data)
        {
            _data = data;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(235, 238, 245);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const int marginLeft = 28;
            const int marginRight = 8;
            const int marginTop = 8;
            const int marginBottom = 28;

            int chartW = Width - marginLeft - marginRight;
            int chartH = Height - marginTop - marginBottom;

            const int days = 28;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var values = Enumerable.Range(0, days)
                .Select(i => _data.TryGetValue(today.AddDays(-(days - 1 - i)), out var v) ? v : 0)
                .ToArray();

            int maxVal = Math.Max(1, values.Max());

            // Y-axis line
            using var axisPen = new Pen(Color.FromArgb(180, 190, 210), 1f);
            g.DrawLine(axisPen, marginLeft, marginTop, marginLeft, marginTop + chartH);
            g.DrawLine(axisPen, marginLeft, marginTop + chartH, marginLeft + chartW, marginTop + chartH);

            // Y-axis label (max)
            using var labelFont = new Font("Segoe UI", 6.5f);
            var maxStr = maxVal.ToString();
            var maxSize = g.MeasureString(maxStr, labelFont);
            g.DrawString(maxStr, labelFont, Brushes.Gray,
                marginLeft - maxSize.Width - 2, marginTop - maxSize.Height / 2);

            float barWidth = (float)chartW / days;
            float gap = Math.Max(1f, barWidth * 0.18f);

            for (int i = 0; i < days; i++)
            {
                float barH = values[i] == 0 ? 0 : Math.Max(2f, (float)values[i] / maxVal * chartH);
                float x = marginLeft + i * barWidth + gap / 2;
                float y = marginTop + chartH - barH;
                float w = barWidth - gap;

                bool isToday = i == days - 1;
                int alpha = isToday ? 255 : (int)(90 + 130.0 * (i + 1) / days);
                var barColor = isToday
                    ? Color.FromArgb(alpha, 30, 110, 210)
                    : Color.FromArgb(alpha, 60, 140, 220);

                using var brush = new SolidBrush(barColor);
                if (barH > 0)
                    g.FillRectangle(brush, x, y, w, barH);

                // Day label: every 7 bars and today
                if (isToday || i % 7 == 0)
                {
                    var date = today.AddDays(-(days - 1 - i));
                    var label = isToday ? "auj." : date.ToString("d/M");
                    var sz = g.MeasureString(label, labelFont);
                    g.DrawString(label, labelFont, Brushes.Gray,
                        x + w / 2 - sz.Width / 2,
                        marginTop + chartH + 3);
                }
            }
        }
    }
}
