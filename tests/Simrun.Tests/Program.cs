using System;
using System.Collections.Generic;
using Simrun.Application.Models;
using Simrun.Application.Ports;
using Simrun.Application.UseCases;
using Simrun.Domain.Entities;
using Simrun.Domain.ValueObjects;
using Simrun.Infrastructure.Levels;
using Simrun.Infrastructure.Persistence;
using Simrun.Infrastructure.Simulation;

var tests = new List<(string Name, Action Body)>
{
    ("Physics applies ground acceleration", Physics_AppliesGroundAcceleration),
    ("Physics applies jump impulse", Physics_AppliesJumpImpulse),
    ("TickRun completes when goal reached", TickRun_CompletesWhenWithinGoal),
    ("TickRun accumulates elapsed time", TickRun_AccumulatesElapsed),
};

var failures = 0;

foreach (var (name, body) in tests)
{
    try
    {
        body();
        Console.WriteLine($"[PASS] {name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"[FAIL] {name}: {ex.Message}");
    }
}

Console.WriteLine(failures == 0
    ? $"All {tests.Count} tests passed."
    : $"{failures} of {tests.Count} tests failed.");

return failures;

static void Physics_AppliesGroundAcceleration()
{
    var level = CreateFlatLevel();
    var physics = new NaivePhysicsEngine();
    var player = new PlayerState(Vector3.Zero, Vector3.Zero, isGrounded: true);
    var input = new PlayerInput(0f, 1f, Jump: false);

    var next = physics.Step(level, player, input, deltaSeconds: 0.016f);

    AssertTrue(next.Position.Z > player.Position.Z, "player moved forward");
    AssertTrue(next.IsGrounded, "player remains grounded");
    AssertAlmost(next.Position.Y, level.FloorHeight, 1e-3f, "player stays on floor height");
}

static void Physics_AppliesJumpImpulse()
{
    var level = CreateFlatLevel();
    var physics = new NaivePhysicsEngine();
    var player = new PlayerState(Vector3.Zero, Vector3.Zero, isGrounded: true);
    var input = new PlayerInput(0f, 0f, Jump: true);

    var next = physics.Step(level, player, input, deltaSeconds: 0.016f);

    AssertTrue(next.Velocity.Y > 0f, "jump adds upward velocity");
    AssertTrue(!next.IsGrounded, "player leaves the ground");
}

static void TickRun_CompletesWhenWithinGoal()
{
    var level = new LevelDefinition(
        id: "complete-soon",
        name: "Goal at spawn",
        spawnPoint: Vector3.Zero,
        goalPosition: new Vector3(0.1f, 0f, 0f),
        goalRadius: 2f,
        floorHeight: 0f,
        movement: MovementSettings.Default);

    var services = CreateServices(level);
    var run = services.StartRun.Start();

    run = services.TickRun.Tick(new PlayerInput(0f, 0f, Jump: false), 0.016f);

    AssertTrue(run.Completed, "run marked completed");
    AssertTrue(run.CompletedAt is not null, "completion timestamp set");
}

static void TickRun_AccumulatesElapsed()
{
    var level = CreateFlatLevel();
    var services = CreateServices(level);
    var run = services.StartRun.Start();

    run = services.TickRun.Tick(new PlayerInput(0f, 1f, Jump: false), 0.05f);
    run = services.TickRun.Tick(new PlayerInput(0f, 1f, Jump: false), 0.05f);

    AssertAlmost((float)run.Elapsed.TotalSeconds, 0.10f, 1e-3f, "elapsed time accumulated");
}

static (StartRunHandler StartRun, TickRunHandler TickRun) CreateServices(LevelDefinition level)
{
    var levels = new InMemoryLevelRepository(new[] { level });
    var runStore = new InMemoryRunStateStore();
    var physics = new NaivePhysicsEngine();
    var time = new FixedTimeProvider();

    var startRun = new StartRunHandler(levels, runStore, time);
    var tickRun = new TickRunHandler(levels, runStore, physics, time);

    return (startRun, tickRun);
}

static LevelDefinition CreateFlatLevel() =>
    new(
        id: "flat",
        name: "Flat Test",
        spawnPoint: Vector3.Zero,
        goalPosition: new Vector3(10f, 0f, 10f),
        goalRadius: 1f,
        floorHeight: 0f,
        movement: MovementSettings.Default);

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertAlmost(float actual, float expected, float tolerance, string message)
{
    if (Math.Abs(actual - expected) > tolerance)
    {
        throw new InvalidOperationException($"{message}. Expected {expected} ± {tolerance} but was {actual}.");
    }
}

file sealed class FixedTimeProvider : ITimeProvider
{
    private DateTimeOffset _now = DateTimeOffset.UnixEpoch;
    public DateTimeOffset Now => _now;
    public void AdvanceSeconds(double seconds) => _now = _now.AddSeconds(seconds);
}
