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
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(24);

        var layout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Padding = new Padding(8),
        };

        layout.Controls.Add(new Label
        {
            Text = string.Format(Strings.Get("stats_breaks_today"), history.GetToday()),
            Font = new Font("Segoe UI", 16),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4),
        });

        layout.Controls.Add(new Label
        {
            Text = string.Format(Strings.Get("stats_breaks_week"), history.GetWeekTotal()),
            Font = new Font("Segoe UI", 11),
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16),
        });

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
}
