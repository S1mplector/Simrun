using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Simrun.Application.Models;

namespace Simrun.Presentation.Input;

/// <summary>
/// Minimal console-based input mapper. Uses toggle-style controls because the console API lacks key-up events.
/// W/S set forward/backward, A/D set strafe, F toggles sprint, C toggles mouse capture, Space triggers jump on the current frame.
/// Press R to reset movement axes, Q/Esc to quit.
/// </summary>
internal sealed class InputController
{
    private bool _forward;
    private bool _backward;
    private bool _left;
    private bool _right;
    private bool _sprint;
    public bool CaptureMouse { get; private set; }
    public bool DebugDraw { get; private set; }
    private readonly HashSet<ConsoleKey> _previousPressed = new();

    public PlayerInput Poll()
    {
        var jump = false;

        var pressed = PollKeys();

        bool WasPressed(ConsoleKey key) => _previousPressed.Contains(key);
        bool IsPressed(ConsoleKey key) => pressed.Contains(key);
        bool IsToggled(ConsoleKey key) => IsPressed(key) && !WasPressed(key);

        _forward = IsPressed(ConsoleKey.W);
        _backward = IsPressed(ConsoleKey.S);
        _left = IsPressed(ConsoleKey.A);
        _right = IsPressed(ConsoleKey.D);
        _sprint = IsPressed(ConsoleKey.F) || _sprint; // sticky unless reset with R

        if (IsToggled(ConsoleKey.F)) _sprint = !_sprint;
        if (IsToggled(ConsoleKey.C))
        {
            CaptureMouse = !CaptureMouse;
            Console.WriteLine($"Mouse capture {(CaptureMouse ? "ON" : "OFF")}");
        }
        if (IsToggled(ConsoleKey.G))
        {
            DebugDraw = !DebugDraw;
            Console.WriteLine($"Debug draw {(DebugDraw ? "ON" : "OFF")}");
        }
        if (IsToggled(ConsoleKey.R))
        {
            _forward = _backward = _left = _right = false;
            _sprint = false;
        }
        if (IsToggled(ConsoleKey.Spacebar))
        {
            jump = true;
        }
        if (IsToggled(ConsoleKey.Q) || IsToggled(ConsoleKey.Escape))
        {
            Environment.Exit(0);
        }

        _previousPressed.Clear();
        foreach (var key in pressed)
        {
            _previousPressed.Add(key);
        }

        var forward = (_forward ? 1f : 0f) + (_backward ? -1f : 0f);
        var strafe = (_right ? 1f : 0f) + (_left ? -1f : 0f);

        return new PlayerInput(strafe, forward, jump, _sprint);
    }

    private static HashSet<ConsoleKey> PollKeys()
    {
        var keys = new[]
        {
            ConsoleKey.W, ConsoleKey.A, ConsoleKey.S, ConsoleKey.D,
            ConsoleKey.Spacebar, ConsoleKey.F, ConsoleKey.C, ConsoleKey.G,
            ConsoleKey.R, ConsoleKey.Q, ConsoleKey.Escape
        };

        var pressed = new HashSet<ConsoleKey>();
        foreach (var key in keys)
        {
            if (IsKeyDown(key))
            {
                pressed.Add(key);
            }
        }
        return pressed;
    }

    private static bool IsKeyDown(ConsoleKey key)
    {
        var code = (int)key;
        var state = GetAsyncKeyState(code);
        return (state & 0x8000) != 0;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
