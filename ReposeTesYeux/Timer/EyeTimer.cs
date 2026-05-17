using ReposeTesYeux.Settings;

namespace ReposeTesYeux.Timer;

public class EyeTimer : IDisposable
{
    private readonly IClock _clock;
    private readonly System.Threading.Timer _ticker;

    private TimerState _state = TimerState.Idle;
    private DateTime _phaseEnd;
    private DateTime _pausedAt;
    private TimeSpan _remainingAtPause;
    private int _breaksToday;
    private DateTime _lastBreakDate;

    public event Action<TimeSpan>? OnTick;
    public event Action? OnBreakStart;
    public event Action? OnBreakEnd;
    public event Action<TimerState>? OnStateChanged;

    public TimerState State => _state;
    public int BreaksToday => _breaksToday;

    private AppSettings _settings;

    public EyeTimer(AppSettings settings, IClock? clock = null)
    {
        _settings = settings;
        _clock = clock ?? new SystemClock();
        _ticker = new System.Threading.Timer(Tick, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void UpdateSettings(AppSettings settings) => _settings = settings;

    public void Start()
    {
        if (_state != TimerState.Idle)
            return;
        BeginWorkPhase();
    }

    public void Pause()
    {
        if (_state is not (TimerState.Working or TimerState.Break))
            return;
        _pausedAt = _clock.UtcNow;
        _remainingAtPause = _phaseEnd - _pausedAt;
        _ticker.Change(Timeout.Infinite, Timeout.Infinite);
        ChangeState(TimerState.Paused);
    }

    public void Resume()
    {
        if (_state != TimerState.Paused)
            return;
        // Restore whichever phase we were in based on remaining time context
        _phaseEnd = _clock.UtcNow + _remainingAtPause;
        _ticker.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        // We can't know if we were in Working or Break phase from here, so we use a flag
        ChangeState(_remainingAtPause <= TimeSpan.FromSeconds(_settings.BreakDurationSeconds + 5)
            ? TimerState.Break
            : TimerState.Working);
    }

    public void SkipBreak()
    {
        if (_state != TimerState.Break)
            return;
        EndBreak();
    }

    public void TriggerBreakNow()
    {
        if (_state == TimerState.Idle)
            return;
        _ticker.Change(Timeout.Infinite, Timeout.Infinite);
        BeginBreakPhase();
    }

    public void Stop()
    {
        _ticker.Change(Timeout.Infinite, Timeout.Infinite);
        ChangeState(TimerState.Idle);
    }

    private void BeginWorkPhase()
    {
        _phaseEnd = _clock.UtcNow + TimeSpan.FromMinutes(_settings.WorkIntervalMinutes);
        _ticker.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        ChangeState(TimerState.Working);
    }

    private void BeginBreakPhase()
    {
        if (IsInDoNotDisturb())
        {
            BeginWorkPhase();
            return;
        }

        _phaseEnd = _clock.UtcNow + TimeSpan.FromSeconds(_settings.BreakDurationSeconds);
        _ticker.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        ChangeState(TimerState.Break);
        IncrementBreaksToday();
        OnBreakStart?.Invoke();
    }

    private void EndBreak()
    {
        OnBreakEnd?.Invoke();
        BeginWorkPhase();
    }

    private void Tick(object? _)
    {
        if (_state is TimerState.Idle or TimerState.Paused)
            return;

        var remaining = _phaseEnd - _clock.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            if (_state == TimerState.Working)
                BeginBreakPhase();
            else if (_state == TimerState.Break)
                EndBreak();
            return;
        }

        OnTick?.Invoke(remaining);
    }

    private bool IsInDoNotDisturb()
    {
        var window = _settings.DoNotDisturbWindow;
        if (window is null)
            return false;

        var now = _clock.LocalTimeNow;
        var (start, end) = window.Value;

        return start <= end
            ? now >= start && now <= end
            : now >= start || now <= end;
    }

    private void IncrementBreaksToday()
    {
        var today = _clock.UtcNow.Date;
        if (_lastBreakDate != today)
        {
            _breaksToday = 0;
            _lastBreakDate = today;
        }
        _breaksToday++;
    }

    private void ChangeState(TimerState newState)
    {
        _state = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void Dispose()
    {
        _ticker.Dispose();
    }
}
