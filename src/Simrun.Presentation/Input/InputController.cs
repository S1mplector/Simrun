using System;
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

    public PlayerInput Poll()
    {
        var jump = false;

        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.W:
                    _forward = true;
                    _backward = false;
                    break;
                case ConsoleKey.S:
                    _backward = true;
                    _forward = false;
                    break;
                case ConsoleKey.A:
                    _left = true;
                    _right = false;
                    break;
                case ConsoleKey.D:
                    _right = true;
                    _left = false;
                    break;
                case ConsoleKey.F:
                    _sprint = !_sprint;
                    break;
                case ConsoleKey.C:
                    CaptureMouse = !CaptureMouse;
                    Console.WriteLine($"Mouse capture {(CaptureMouse ? "ON" : "OFF")}");
                    break;
                case ConsoleKey.Spacebar:
                    jump = true;
                    break;
                case ConsoleKey.R:
                    _forward = _backward = _left = _right = false;
                    break;
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
            }
        }

        var forward = (_forward ? 1f : 0f) + (_backward ? -1f : 0f);
        var strafe = (_right ? 1f : 0f) + (_left ? -1f : 0f);

        return new PlayerInput(strafe, forward, jump, _sprint);
    }
}
