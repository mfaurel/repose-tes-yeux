using System.IO.Pipes;

namespace ReposeTesYeux.Timer;

public sealed class CliServer : IDisposable
{
    public const string PipeName = "ReposeTesYeux";

    private readonly EyeTimer _timer;
    private readonly CancellationTokenSource _cts = new();

    public CliServer(EyeTimer timer) => _timer = timer;

    public void Start() => Task.Run(ListenLoopAsync);

    private async Task ListenLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    PipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                await pipe.WaitForConnectionAsync(_cts.Token);
                using var sr = new StreamReader(pipe, leaveOpen: true);
                using var sw = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
                var cmd = await sr.ReadLineAsync();
                sw.WriteLine(HandleCommand(cmd?.Trim()));
                pipe.WaitForPipeDrain();
                pipe.Disconnect();
            }
            catch (OperationCanceledException) { break; }
            catch { /* pipe errors: ignore and retry */ }
        }
    }

    private string HandleCommand(string? command) => command switch
    {
        "--status" => _timer.State.ToString(),
        "--break-now" => TriggerBreak(),
        "--pause" => TogglePause(),
        _ => $"unknown: {command}",
    };

    private string TriggerBreak()
    {
        _timer.TriggerBreakNow();
        return "ok";
    }

    private string TogglePause()
    {
        if (_timer.State == TimerState.Paused)
            _timer.Resume();
        else
            _timer.Pause();
        return _timer.State.ToString();
    }

    public void Dispose() { _cts.Cancel(); _cts.Dispose(); }
}
