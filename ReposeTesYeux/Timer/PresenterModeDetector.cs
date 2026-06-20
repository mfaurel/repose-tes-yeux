using System.Runtime.InteropServices;

namespace ReposeTesYeux.Timer;

public static class PresenterModeDetector
{
    [DllImport("shell32.dll")]
    private static extern int SHQueryUserNotificationState(out int pquns);

    // QUERY_USER_NOTIFICATION_STATE enum values (Windows SDK)
    private const int QUNS_RUNNING_D3D_FULL_SCREEN = 3;
    private const int QUNS_PRESENTATION_MODE = 4;

    public static bool IsPresenting()
    {
        try
        {
            int hr = SHQueryUserNotificationState(out int state);
            if (hr != 0) return false;
            return state is QUNS_PRESENTATION_MODE or QUNS_RUNNING_D3D_FULL_SCREEN;
        }
        catch
        {
            return false;
        }
    }
}
