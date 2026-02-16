using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ExileCore;

namespace SniperPlugin.Utils;

public static class WindowFocus
{
    [DllImport("user32.dll")]
    private static extern void SwitchToThisWindow(IntPtr hWnd);

    private static readonly string processName = "PathOfExile";

    public static void FocusPoEWindow()
    {
        try
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process != null) SwitchToThisWindow(process.MainWindowHandle);
        }
        catch (Exception ex)
        {
            DebugWindow.LogError($"[WindowFocus] Focus Failed: {ex.Message}");
        }
    }
}