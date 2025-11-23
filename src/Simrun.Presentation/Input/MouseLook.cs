using System;
using System.Runtime.InteropServices;

namespace Simrun.Presentation.Input;

/// <summary>
/// Raw-ish mouse polling using Win32 to get relative deltas. Re-centers the cursor when capture is enabled.
/// </summary>
internal sealed class MouseLook
{
    private const int SmCxScreen = 0;
    private const int SmCyScreen = 1;

    private POINT _last;
    private bool _initialized;

    public LookInput Poll(bool capture, float sensitivity = 0.0025f)
    {
        if (!GetCursorPos(out var current))
        {
            return new LookInput(0f, 0f);
        }

        if (!_initialized)
        {
            _initialized = true;
            _last = capture ? CenterCursor(current) : current;
            return new LookInput(0f, 0f);
        }

        var deltaX = current.X - _last.X;
        var deltaY = current.Y - _last.Y;

        if (capture)
        {
            _last = CenterCursor(current);
        }
        else
        {
            _last = current;
        }

        return new LookInput(deltaX * sensitivity, deltaY * sensitivity);
    }

    private static POINT CenterCursor(POINT current)
    {
        var center = new POINT { X = GetSystemMetrics(SmCxScreen) / 2, Y = GetSystemMetrics(SmCyScreen) / 2 };
        if (current.X != center.X || current.Y != center.Y)
        {
            SetCursorPos(center.X, center.Y);
        }
        return center;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
