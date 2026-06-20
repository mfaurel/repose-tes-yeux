using System.IO.Pipes;
using System.Runtime.InteropServices;
using ReposeTesYeux.I18n;
using ReposeTesYeux.Settings;
using ReposeTesYeux.Startup;
using ReposeTesYeux.Timer;
using ReposeTesYeux.UI;

namespace ReposeTesYeux;

static class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);
    private const int AttachParentProcess = -1;

    [STAThread]
    static void Main()
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

        if (args.Length > 0)
        {
            AttachConsole(AttachParentProcess);
            RunCliCommand(args[0]);
            return;
        }

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
        var history = new BreakHistory();
        var profileStore = new ProfileStore();

        if (!profileStore.Contains(settings.ActiveProfileName))
            settings.ActiveProfileName = ProfileStore.DefaultProfileKey;

        using var timer = new EyeTimer(settings);
        using var cliServer = new CliServer(timer);
        using var tray = new TrayController(timer, settings, store, startupManager, history, profileStore);

        cliServer.Start();
        timer.Start();
        Application.Run();
    }

    private static void RunCliCommand(string command)
    {
        if (command is "--version" or "-v")
        {
            Console.WriteLine(UpdateChecker.CurrentVersion);
            return;
        }
        if (command is "--help" or "-h")
        {
            Console.WriteLine("Usage: ReposeTesYeux [--status|--break-now|--pause|--version]");
            Console.WriteLine("  --status     Show current timer state");
            Console.WriteLine("  --break-now  Trigger an immediate break");
            Console.WriteLine("  --pause      Toggle pause/resume");
            Console.WriteLine("  --version    Show version");
            return;
        }

        try
        {
            using var pipe = new NamedPipeClientStream(".", CliServer.PipeName, PipeDirection.InOut);
            pipe.Connect(2000);
            using var sw = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
            using var sr = new StreamReader(pipe, leaveOpen: true);
            sw.WriteLine(command);
            pipe.WaitForPipeDrain();
            var response = sr.ReadLine();
            Console.WriteLine(response);
        }
        catch (TimeoutException)
        {
            Console.Error.WriteLine("Error: ReposeTesYeux is not running.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
