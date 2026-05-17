using ReposeTesYeux.Timer;

namespace ReposeTesYeux.Tests;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
    public TimeOnly LocalTimeNow => TimeOnly.FromDateTime(UtcNow.ToLocalTime());

    public void Advance(TimeSpan duration) => UtcNow += duration;
    public void AdvanceMinutes(double minutes) => Advance(TimeSpan.FromMinutes(minutes));
    public void AdvanceSeconds(double seconds) => Advance(TimeSpan.FromSeconds(seconds));
}
