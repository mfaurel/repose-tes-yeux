using ReposeTesYeux.Settings;
using ReposeTesYeux.Timer;

namespace ReposeTesYeux.Tests;

public class BreakKindTests : IDisposable
{
    private readonly FakeClock _clock;
    private readonly AppSettings _settings;
    private EyeTimer _timer;

    public BreakKindTests()
    {
        _clock = new FakeClock();
        _settings = new AppSettings
        {
            WorkIntervalMinutes = 20,
            BreakDurationSeconds = 20,
            LongBreakEnabled = true,
            LongBreakEveryN = 3,
            LongBreakDurationSeconds = 90,
            EndOfDayEnabled = false,
            EndOfDayHours = 7,
        };
        _timer = new EyeTimer(_settings, _clock);
    }

    public void Dispose() => _timer.Dispose();

    [Fact]
    public void TriggerBreakNow_AlwaysFiresRegularBreak()
    {
        _timer.Start();
        BreakKind? kind = null;
        _timer.OnBreakStart += k => kind = k;
        _timer.TriggerBreakNow();
        Assert.Equal(BreakKind.Regular, kind);
    }

    [Fact]
    public void LongBreak_FiresAfterNBreaks()
    {
        _settings.LongBreakEveryN = 3;
        _timer.Start();

        var kinds = new List<BreakKind>();
        _timer.OnBreakStart += k => kinds.Add(k);

        // Simulate 3 work intervals expiring via clock + tick
        for (int i = 0; i < 3; i++)
        {
            _clock.Advance(TimeSpan.FromMinutes(21)); // past work interval
            SimulateTick();
            if (_timer.State == TimerState.Break)
            {
                _timer.SkipBreak();
            }
        }

        Assert.Equal(3, kinds.Count);
        Assert.Equal(BreakKind.Regular, kinds[0]);
        Assert.Equal(BreakKind.Regular, kinds[1]);
        Assert.Equal(BreakKind.Long, kinds[2]);
    }

    [Fact]
    public void LongBreak_Disabled_NeverFires()
    {
        _settings.LongBreakEnabled = false;
        _timer.Start();

        var kinds = new List<BreakKind>();
        _timer.OnBreakStart += k => kinds.Add(k);

        for (int i = 0; i < 5; i++)
        {
            _clock.Advance(TimeSpan.FromMinutes(21));
            SimulateTick();
            if (_timer.State == TimerState.Break)
                _timer.SkipBreak();
        }

        Assert.All(kinds, k => Assert.Equal(BreakKind.Regular, k));
    }

    [Fact]
    public void EndOfDay_FiresAfterThreshold()
    {
        _settings.EndOfDayEnabled = true;
        _settings.EndOfDayHours = 1;
        _timer.Start();

        BreakKind? endOfDayKind = null;
        _timer.OnBreakStart += k => { if (k == BreakKind.EndOfDay) endOfDayKind = k; };

        // Advance 61 minutes of work (past the 1-hour threshold)
        for (int s = 0; s < 3700; s++)
        {
            _clock.AdvanceSeconds(1);
            SimulateTick();
            if (_timer.State == TimerState.Break)
                _timer.SkipBreak();
        }

        Assert.Equal(BreakKind.EndOfDay, endOfDayKind);
    }

    [Fact]
    public void EndOfDay_Disabled_NeverFires()
    {
        _settings.EndOfDayEnabled = false;
        _settings.LongBreakEnabled = false;
        _timer.Start();

        var kinds = new List<BreakKind>();
        _timer.OnBreakStart += k => kinds.Add(k);

        for (int s = 0; s < 3700; s++)
        {
            _clock.AdvanceSeconds(1);
            SimulateTick();
            if (_timer.State == TimerState.Break)
                _timer.SkipBreak();
        }

        Assert.DoesNotContain(BreakKind.EndOfDay, kinds);
    }

    // Directly invoke the private Tick via the timer's exposed state machine
    // by advancing the clock so remaining <= 0 and calling TriggerBreakNow is not needed.
    // Instead we use reflection to call the private Tick method.
    private void SimulateTick()
    {
        var method = typeof(EyeTimer).GetMethod("Tick",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(_timer, new object?[] { null });
    }
}
