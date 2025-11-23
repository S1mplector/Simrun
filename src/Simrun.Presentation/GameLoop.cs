using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Simrun.Engine.Interop;
using Simrun.Engine.Rendering;
using Simrun.Presentation.Cameras;
using Simrun.Presentation.Input;
using Simrun.Domain.Entities;

namespace Simrun.Presentation;

internal sealed class GameLoop
{
    private readonly GameServices _services;
    private readonly IRenderBackend _renderer;
    private readonly RenderSurface _surface;
    private readonly Scene _scene = new();
    private readonly CameraRig _cameraRig = new();
    private readonly Camera _camera = new();
    private readonly InputController _input = new();
    private readonly MouseLook _mouseLook = new();

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
        BootstrapLevelGeometry(run);
        var sw = Stopwatch.StartNew();
        var last = sw.Elapsed;
        var printTimer = 0f;

        Console.WriteLine("Controls: W/A/S/D move, Space jump, F toggle sprint, C toggle mouse capture, R reset movement, Q/Esc quit.");

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
            var look = _mouseLook.Poll(_input.CaptureMouse);
            run = _services.TickRun.Tick(input, deltaSeconds);

            _cameraRig.Update(_camera, run.Player.Position, run.Player.Velocity, look, deltaSeconds);
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

    private void BootstrapLevelGeometry(RunState run)
    {
        var level = _services.Levels.FindById(run.LevelId);
        if (level is null)
        {
            return;
        }

        var floor = Mesh.CreateQuad();
        var floorMat = new Material { Albedo = new Vector3(0.35f, 0.35f, 0.4f) };
        var floorTransform = new Transform
        {
            Position = new Vector3(0f, level.FloorHeight, 0f),
            Scale = new Vector3(200f, 1f, 200f)
        };
        _scene.Add(new Renderable(floor, floorMat, floorTransform));

        var goal = Mesh.CreateCube(level.GoalRadius * 2f);
        var goalMat = new Material { Albedo = new Vector3(1.0f, 0.9f, 0.2f) };
        var goalTransform = new Transform
        {
            Position = level.GoalPosition.ToEngine()
        };
        _scene.Add(new Renderable(goal, goalMat, goalTransform));
    }
}
