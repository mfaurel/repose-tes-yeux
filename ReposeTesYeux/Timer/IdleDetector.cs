using System.Runtime.InteropServices;

namespace ReposeTesYeux.Timer;

public static class IdleDetector
{
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public static TimeSpan GetIdleTime()
    {
        var info = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>() };
        if (!GetLastInputInfo(ref info))
            return TimeSpan.Zero;
        uint tickDiff = unchecked((uint)Environment.TickCount - info.dwTime);
        return TimeSpan.FromMilliseconds(tickDiff);
    }

    public static bool IsIdle(int thresholdMinutes) =>
        GetIdleTime() >= TimeSpan.FromMinutes(thresholdMinutes);
}
