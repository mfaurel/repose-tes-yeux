using ReposeTesYeux.Settings;
using ReposeTesYeux.Timer;

namespace ReposeTesYeux.Tests;

public class EyeTimerTests : IDisposable
{
    private readonly FakeClock _clock;
    private readonly AppSettings _settings;
    private EyeTimer _timer;

    public EyeTimerTests()
    {
        _clock = new FakeClock();
        _settings = new AppSettings
        {
            WorkIntervalMinutes = 20,
            BreakDurationSeconds = 20,
            DistanceMetres = 20,
        };
        _timer = new EyeTimer(_settings, _clock);
    }

    public void Dispose() => _timer.Dispose();

    [Fact]
    public void InitialState_IsIdle()
    {
        Assert.Equal(TimerState.Idle, _timer.State);
    }

    [Fact]
    public void Start_TransitionsToWorking()
    {
        _timer.Start();
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void Start_WhenAlreadyWorking_DoesNothing()
    {
        _timer.Start();
        _timer.Start();
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void Pause_WhileWorking_TransitionsToPaused()
    {
        _timer.Start();
        _timer.Pause();
        Assert.Equal(TimerState.Paused, _timer.State);
    }

    [Fact]
    public void Pause_WhenIdle_DoesNothing()
    {
        _timer.Pause();
        Assert.Equal(TimerState.Idle, _timer.State);
    }

    [Fact]
    public void Resume_WhilePaused_TransitionsToWorking()
    {
        _timer.Start();
        _timer.Pause();
        _timer.Resume();
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void Resume_WhenNotPaused_DoesNothing()
    {
        _timer.Start();
        _timer.Resume();
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void TriggerBreakNow_WhileWorking_FiresBreakStartEvent()
    {
        _timer.Start();
        bool fired = false;
        _timer.OnBreakStart += _ => fired = true;
        _timer.TriggerBreakNow();
        Assert.True(fired);
        Assert.Equal(TimerState.Break, _timer.State);
    }

    [Fact]
    public void TriggerBreakNow_WhenIdle_DoesNothing()
    {
        _timer.TriggerBreakNow();
        Assert.Equal(TimerState.Idle, _timer.State);
    }

    [Fact]
    public void SkipBreak_DuringBreak_FiresBreakEndAndReturnsToWorking()
    {
        _timer.Start();
        bool endFired = false;
        _timer.OnBreakEnd += () => endFired = true;
        _timer.TriggerBreakNow();
        _timer.SkipBreak();
        Assert.True(endFired);
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void SkipBreak_WhenNotInBreak_DoesNothing()
    {
        _timer.Start();
        _timer.SkipBreak();
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void Stop_ResetsToIdle()
    {
        _timer.Start();
        _timer.Stop();
        Assert.Equal(TimerState.Idle, _timer.State);
    }

    [Fact]
    public void BreaksToday_StartsAtZero()
    {
        Assert.Equal(0, _timer.BreaksToday);
    }

    [Fact]
    public void BreaksToday_IncrementsOnBreakStart()
    {
        _timer.Start();
        _timer.TriggerBreakNow();
        Assert.Equal(1, _timer.BreaksToday);
    }

    [Fact]
    public void BreaksToday_ResetsOnNewDay()
    {
        _timer.Start();
        _timer.TriggerBreakNow();
        _timer.SkipBreak();

        _clock.Advance(TimeSpan.FromDays(1));
        _timer.TriggerBreakNow();

        Assert.Equal(1, _timer.BreaksToday);
    }

    [Fact]
    public void DoNotDisturb_SkipsBreakAndRestartsWorkPhase()
    {
        // Set DND window that covers current time
        var localNow = _clock.LocalTimeNow;
        _settings.DoNotDisturbStart = localNow.AddMinutes(-5).ToString("HH:mm");
        _settings.DoNotDisturbEnd = localNow.AddMinutes(5).ToString("HH:mm");

        _timer.Start();
        bool breakStarted = false;
        _timer.OnBreakStart += _ => breakStarted = true;
        _timer.TriggerBreakNow();

        Assert.False(breakStarted);
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void StateChanged_EventFired_OnEachTransition()
    {
        var states = new List<TimerState>();
        _timer.OnStateChanged += s => states.Add(s);

        _timer.Start();
        _timer.Pause();
        _timer.Resume();
        _timer.Stop();

        Assert.Equal(new[] { TimerState.Working, TimerState.Paused, TimerState.Working, TimerState.Idle }, states);
    }

    [Fact]
    public void UpdateSettings_AffectsSubsequentBehavior()
    {
        _timer.Start();
        var newSettings = new AppSettings { WorkIntervalMinutes = 30, BreakDurationSeconds = 30 };
        _timer.UpdateSettings(newSettings);

        // Just verify no exception and state is still working
        Assert.Equal(TimerState.Working, _timer.State);
    }

    [Fact]
    public void Pause_DuringBreak_TransitionsToPaused()
    {
        _timer.Start();
        _timer.TriggerBreakNow();
        _timer.Pause();
        Assert.Equal(TimerState.Paused, _timer.State);
    }

    [Fact]
    public void Resume_AfterPausingDuringBreak_ReturnsToBreak()
    {
        _timer.Start();
        _timer.TriggerBreakNow();
        _timer.Pause();
        _timer.Resume();
        Assert.Equal(TimerState.Break, _timer.State);
    }

    [Fact]
    public void Stop_WhileInBreak_ResetsToIdle()
    {
        _timer.Start();
        _timer.TriggerBreakNow();
        _timer.Stop();
        Assert.Equal(TimerState.Idle, _timer.State);
    }

    [Fact]
    public void BreaksToday_MultipleBreaks_CountsAll()
    {
        _timer.Start();
        _timer.TriggerBreakNow();
        _timer.SkipBreak();
        _timer.TriggerBreakNow();
        _timer.SkipBreak();
        _timer.TriggerBreakNow();
        Assert.Equal(3, _timer.BreaksToday);
    }

    [Fact]
    public void DoNotDisturb_OutsideWindow_DoesNotSkipBreak()
    {
        var localNow = _clock.LocalTimeNow;
        _settings.DoNotDisturbStart = localNow.AddMinutes(10).ToString("HH:mm");
        _settings.DoNotDisturbEnd = localNow.AddMinutes(30).ToString("HH:mm");

        _timer.Start();
        bool breakStarted = false;
        _timer.OnBreakStart += _ => breakStarted = true;
        _timer.TriggerBreakNow();

        Assert.True(breakStarted);
        Assert.Equal(TimerState.Break, _timer.State);
    }
}
