using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;
using ReposeTesYeux.Startup;
using ReposeTesYeux.Timer;
using ReposeTesYeux.UI;

namespace ReposeTesYeux;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        using var mutex = new System.Threading.Mutex(true, "ReposeTesYeux_SingleInstance", out bool isNew);
        if (!isNew)
            return;

        var store = new SettingsStore();
        var settings = store.Load();
        Strings.SetLanguage(settings.Language);

        var startupManager = new StartupManager();
        using var timer = new EyeTimer(settings);
        using var tray = new TrayController(timer, settings, store, startupManager);

        timer.Start();
        Application.Run();
    }
}
