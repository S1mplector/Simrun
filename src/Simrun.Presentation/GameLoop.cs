using System;
using System.Diagnostics;
using System.Threading;
using Simrun.Application.Models;
using Simrun.Engine.Rendering;
using Simrun.Presentation.Camera;
using Simrun.Presentation.Input;

namespace Simrun.Presentation;

internal sealed class GameLoop
{
    private readonly GameServices _services;
    private readonly IRenderBackend _renderer;
    private readonly RenderSurface _surface;
    private readonly Scene _scene = new();
    private readonly Camera.CameraRig _cameraRig = new();
    private readonly Camera _camera = new();
    private readonly InputController _input = new();

    public GameLoop(GameServices services, IRenderBackend renderer, RenderSurface surface)
    {
        _services = services;
        _renderer = renderer;
        _surface = surface;
    }

    public void Run()
    {
        _renderer.Initialize(_surface);

        var run = _services.StartRun.Start();
        var sw = Stopwatch.StartNew();
        var last = sw.Elapsed;
        var printTimer = 0f;

        Console.WriteLine("Controls: W/A/S/D move, Space jump, Shift toggle sprint, R reset movement, Q/Esc quit.");

        while (true)
        {
            var now = sw.Elapsed;
            var deltaSeconds = (float)(now - last).TotalSeconds;
            last = now;

            if (deltaSeconds <= 0f)
            {
                Thread.Sleep(1);
                continue;
            }

            var input = _input.Poll();
            run = _services.TickRun.Tick(input, deltaSeconds);

            _cameraRig.Update(_camera, run.Player.Position, run.Player.Velocity);
            _renderer.Render(_scene, _camera);

            printTimer += deltaSeconds;
            if (printTimer >= 0.5f)
            {
                printTimer = 0f;
                Console.WriteLine(
                    $"t={run.Elapsed.TotalSeconds,6:0.00}s pos=({run.Player.Position.X:0.00},{run.Player.Position.Y:0.00},{run.Player.Position.Z:0.00}) " +
                    $"vel=({run.Player.Velocity.X:0.00},{run.Player.Velocity.Y:0.00},{run.Player.Velocity.Z:0.00}) " +
                    $"grounded={run.Player.IsGrounded}");
            }

            if (run.Completed)
            {
                Console.WriteLine($"Goal reached in {run.Elapsed.TotalSeconds:0.00}s!");
                break;
            }

            // simple frame pacing to avoid pegging CPU
            if (deltaSeconds < 1f / 120f)
            {
                Thread.Sleep(2);
            }
        }

        _renderer.Shutdown();
    }
}
