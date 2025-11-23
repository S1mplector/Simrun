using Simrun.Application.Models;
using Simrun.Presentation;

var services = GameBootstrapper.Build();

var run = services.StartRun.Start();
var activeLevel = services.Levels.FindById(run.LevelId);

Console.WriteLine($"Simrun scaffold ready. Active level: {activeLevel?.Name ?? run.LevelId}");
Console.WriteLine($"Spawned at {Format(run.Player.Position)}");

var demoInputs = new[]
{
    new PlayerInput(1f, 0.3f, Jump: false),
    new PlayerInput(0.8f, 0.8f, Jump: false, Sprint: true),
    new PlayerInput(0f, 1f, Jump: false),
    new PlayerInput(0.2f, 1f, Jump: true),
    new PlayerInput(0.4f, 1f, Jump: false)
};

const float deltaSeconds = 0.016f; // 60 fps fixed-step demo

for (var i = 0; i < 180; i++)
{
    var input = demoInputs[i % demoInputs.Length];
    run = services.TickRun.Tick(input, deltaSeconds);

    Console.WriteLine(
        $"t={run.Elapsed.TotalSeconds,6:0.00}s pos={Format(run.Player.Position)} " +
        $"vel={Format(run.Player.Velocity)} grounded={run.Player.IsGrounded}");

    if (run.Completed)
    {
        Console.WriteLine($"Goal reached in {run.Elapsed.TotalSeconds:0.00}s!");
        break;
    }
}

if (!run.Completed)
{
    Console.WriteLine("Demo loop finished without reaching the goal. Tune physics/levels and run again.");
}

static string Format(Simrun.Domain.ValueObjects.Vector3 vector) => $"({vector.X:0.00}, {vector.Y:0.00}, {vector.Z:0.00})";
