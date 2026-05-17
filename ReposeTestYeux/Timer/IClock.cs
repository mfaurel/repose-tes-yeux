namespace ReposeTesYeux.Timer;

public interface IClock
{
    DateTime UtcNow { get; }
    TimeOnly LocalTimeNow { get; }
}

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public TimeOnly LocalTimeNow => TimeOnly.FromDateTime(DateTime.Now);
}
