using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SniperPlugin.Utils;

public static class MouseLock
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool ClipCursor(ref RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClipCursor(IntPtr lpRect); // pass IntPtr.Zero to release

    public static void LockAt(Vector2 screenPos, int radiusPx = 3)
    {
        int r = Math.Max(1, radiusPx);
        var rect = new RECT
        {
            Left = (int)screenPos.X - r,
            Top = (int)screenPos.Y - r,
            Right = (int)screenPos.X + r,
            Bottom = (int)screenPos.Y + r
        };

        ClipCursor(ref rect);
    }

    public static void Unlock()
    {
        ClipCursor(IntPtr.Zero);
    }
}
