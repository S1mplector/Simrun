using System;
using System.Collections.Generic;
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
    private readonly List<Renderable> _debugRenderables = new();
    private Transform? _playerCapsuleTransform;
    private const float CapsuleRadius = 0.5f;
    private const float CapsuleHalfHeight = 0.9f;

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

        Console.WriteLine("Controls: W/A/S/D move, Space jump, F toggle sprint, C toggle mouse capture, G toggle debug draw, R reset movement, Q/Esc quit.");
        Console.WriteLine($"Level: {_services.Levels.FindById(run.LevelId)?.Name ?? run.LevelId}");
        Console.WriteLine("Follow the yellow cube to finish the level.");

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
            UpdateDebugTransforms(run);
            _scene.ShowDebug = _input.DebugDraw;
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

        var facing = new Simrun.Domain.ValueObjects.Vector3(
            level.GoalPosition.X - level.SpawnPoint.X,
            0f,
            level.GoalPosition.Z - level.SpawnPoint.Z);
        _cameraRig.FaceDirection(facing);

        foreach (var collider in level.Colliders)
        {
            AddPlatformVisual(collider, new Vector3(0.2f, 0.5f, 0.8f));
        }

        AddPathHint(level);
        AddScenicProps(level);

        foreach (var collider in level.Colliders)
        {
            var colliderMesh = Mesh.CreateCube(1f);
            var colliderMat = new Material { Albedo = new Vector3(0.8f, 0.1f, 0.8f) };
            var colliderTransform = new Transform
            {
                Position = collider.Center.ToEngine(),
                Scale = new Vector3(collider.HalfSize.X * 2f, collider.HalfSize.Y * 2f, collider.HalfSize.Z * 2f)
            };
            var renderable = new Renderable(colliderMesh, colliderMat, colliderTransform, isDebug: true);
            _scene.Add(renderable);
            _debugRenderables.Add(renderable);
        }

        var capsuleMesh = Mesh.CreateCube(1f);
        var capsuleMat = new Material { Albedo = new Vector3(0.2f, 0.7f, 1f) };
        _playerCapsuleTransform = new Transform
        {
            Scale = new Vector3(CapsuleRadius * 2f, CapsuleHalfHeight * 2f, CapsuleRadius * 2f)
        };
        var capsuleRenderable = new Renderable(capsuleMesh, capsuleMat, _playerCapsuleTransform, isDebug: true);
        _scene.Add(capsuleRenderable);
        _debugRenderables.Add(capsuleRenderable);
    }

    private void AddPlatformVisual(Simrun.Domain.Entities.CollisionBox collider, Vector3 color)
    {
        var mesh = Mesh.CreateCube(1f);
        var mat = new Material { Albedo = color };
        var transform = new Transform
        {
            Position = collider.Center.ToEngine(),
            Scale = new Vector3(collider.HalfSize.X * 2f, collider.HalfSize.Y * 2f, collider.HalfSize.Z * 2f)
        };
        _scene.Add(new Renderable(mesh, mat, transform));
    }

    private void AddScenicProps(LevelDefinition level)
    {
        var crateMesh = Mesh.CreateCube(1f);
        var crateMat = new Material { Albedo = new Vector3(0.6f, 0.35f, 0.2f), Roughness = 0.4f };

        var rampMesh = Mesh.CreateCube(1f);
        var rampMat = new Material { Albedo = new Vector3(0.3f, 0.4f, 0.7f), Roughness = 0.2f };

        var ground = Mesh.CreateQuad(1f);
        var groundMat = new Material { Albedo = new Vector3(0.15f, 0.2f, 0.22f), Roughness = 0.8f };
        var groundTransform = new Transform
        {
            Position = new Vector3(0f, level.FloorHeight - 0.5f, 0f),
            Scale = new Vector3(400f, 1f, 400f)
        };
        _scene.Add(new Renderable(ground, groundMat, groundTransform));

        // Crates near spawn
        _scene.Add(new Renderable(crateMesh, crateMat, new Transform { Position = level.SpawnPoint.Add(new Simrun.Domain.ValueObjects.Vector3(2f, 0.5f, 2f)).ToEngine() }));
        _scene.Add(new Renderable(crateMesh, crateMat, new Transform { Position = level.SpawnPoint.Add(new Simrun.Domain.ValueObjects.Vector3(-3f, 0.5f, -1f)).ToEngine(), Scale = new Vector3(1.5f, 1.5f, 1.5f) }));

        // A ramp up to a platform mid-way
        var midPoint = level.SpawnPoint.Add(new Simrun.Domain.ValueObjects.Vector3(10f, 2f, 0f));
        var ramp = new Transform
        {
            Position = midPoint.ToEngine(),
            Scale = new Vector3(8f, 1f, 3f),
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.35f)
        };
        _scene.Add(new Renderable(rampMesh, rampMat, ramp));

        var midPlatform = new Transform
        {
            Position = level.SpawnPoint.Add(new Simrun.Domain.ValueObjects.Vector3(16f, 3f, 0f)).ToEngine(),
            Scale = new Vector3(6f, 0.8f, 6f)
        };
        _scene.Add(new Renderable(crateMesh, new Material { Albedo = new Vector3(0.2f, 0.6f, 0.5f), Roughness = 0.3f }, midPlatform));

        // Decorative pillars near goal
        var pillarMat = new Material { Albedo = new Vector3(0.7f, 0.7f, 0.75f), Roughness = 0.5f };
        _scene.Add(new Renderable(crateMesh, pillarMat, new Transform { Position = level.GoalPosition.Add(new Simrun.Domain.ValueObjects.Vector3(3f, 2f, 3f)).ToEngine(), Scale = new Vector3(1.2f, 4f, 1.2f) }));
        _scene.Add(new Renderable(crateMesh, pillarMat, new Transform { Position = level.GoalPosition.Add(new Simrun.Domain.ValueObjects.Vector3(-3f, 2f, -2f)).ToEngine(), Scale = new Vector3(1.2f, 5f, 1.2f) }));
    }

    private void AddPathHint(LevelDefinition level)
    {
        var hintMesh = Mesh.CreateCube(1f);
        var mat = new Material { Albedo = new Vector3(0.9f, 0.4f, 0.1f) };
        var toGoal = new Simrun.Domain.ValueObjects.Vector3(
            level.GoalPosition.X - level.SpawnPoint.X,
            level.GoalPosition.Y - level.SpawnPoint.Y,
            level.GoalPosition.Z - level.SpawnPoint.Z);
        var distance = toGoal.Magnitude();
        var direction = toGoal.Normalize();
        var mid = level.SpawnPoint.Add(direction.Scale(distance * 0.25f));

        var transform = new Transform
        {
            Position = mid.ToEngine(),
            Scale = new Vector3(2f, 0.5f, 6f)
        };
        _scene.Add(new Renderable(hintMesh, mat, transform));
    }

    private void UpdateDebugTransforms(RunState run)
    {
        if (_playerCapsuleTransform is not null)
        {
            _playerCapsuleTransform.Position = run.Player.Position.ToEngine();
        }
    }
}
