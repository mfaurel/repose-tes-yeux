using ReposeTesYeux.Settings;

namespace ReposeTesYeux.Timer;

public class EyeTimer : IDisposable
{
    private readonly IClock _clock;
    private readonly System.Threading.Timer _ticker;

    private TimerState _state = TimerState.Idle;
    private DateTime _phaseEnd;
    private TimeSpan _remainingAtPause;
    private int _breaksToday;
    private DateTime _lastBreakDate;

    // Long break tracking
    private int _breakCountSinceLongBreak;

    // End-of-day tracking
    private int _totalWorkSecondsToday;
    private DateTime _workSecondsDate;
    private bool _endOfDayFiredToday;

    public event Action<TimeSpan>? OnTick;
    public event Action<BreakKind>? OnBreakStart;
    public event Action? OnBreakEnd;
    public event Action<TimerState>? OnStateChanged;

    public TimerState State => _state;
    public int BreaksToday => _breaksToday;
    public int TotalWorkSecondsToday => _totalWorkSecondsToday;

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
        _remainingAtPause = _phaseEnd - _clock.UtcNow;
        _ticker.Change(Timeout.Infinite, Timeout.Infinite);
        ChangeState(TimerState.Paused);
    }

    public void Resume()
    {
        if (_state != TimerState.Paused)
            return;
        _phaseEnd = _clock.UtcNow + _remainingAtPause;
        _ticker.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
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
        BeginBreakPhase(BreakKind.Regular);
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

    private void BeginBreakPhase(BreakKind kind)
    {
        if (IsInDoNotDisturb())
        {
            BeginWorkPhase();
            return;
        }

        int durationSeconds = kind == BreakKind.Regular
            ? _settings.BreakDurationSeconds
            : _settings.LongBreakDurationSeconds;

        _phaseEnd = _clock.UtcNow + TimeSpan.FromSeconds(durationSeconds);
        _ticker.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        ChangeState(TimerState.Break);
        IncrementBreaksToday();
        OnBreakStart?.Invoke(kind);
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

        if (_state == TimerState.Working)
            AccumulateWorkSeconds();

        if (remaining <= TimeSpan.Zero)
        {
            if (_state == TimerState.Working)
            {
                var kind = DetermineNextBreakKind();
                BeginBreakPhase(kind);
            }
            else if (_state == TimerState.Break)
            {
                EndBreak();
            }
            return;
        }

        OnTick?.Invoke(remaining);
    }

    private BreakKind DetermineNextBreakKind()
    {
        if (_settings.EndOfDayEnabled && !_endOfDayFiredToday &&
            _totalWorkSecondsToday >= _settings.EndOfDayHours * 3600)
        {
            _endOfDayFiredToday = true;
            return BreakKind.EndOfDay;
        }

        if (_settings.LongBreakEnabled && _breakCountSinceLongBreak + 1 >= _settings.LongBreakEveryN)
        {
            _breakCountSinceLongBreak = 0;
            return BreakKind.Long;
        }

        _breakCountSinceLongBreak++;
        return BreakKind.Regular;
    }

    private void AccumulateWorkSeconds()
    {
        var today = _clock.UtcNow.Date;
        if (_workSecondsDate != today)
        {
            _totalWorkSecondsToday = 0;
            _workSecondsDate = today;
            _endOfDayFiredToday = false;
        }
        _totalWorkSecondsToday++;
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
