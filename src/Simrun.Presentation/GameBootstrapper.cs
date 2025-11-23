using Simrun.Application.Ports;
using Simrun.Application.UseCases;
using System;
using System.IO;
using Simrun.Infrastructure.Levels;
using Simrun.Infrastructure.Persistence;
using Simrun.Infrastructure.Simulation;
using Simrun.Infrastructure.Time;

namespace Simrun.Presentation;

public sealed record GameServices(
    ILevelRepository Levels,
    IRunStateStore RunStateStore,
    StartRunHandler StartRun,
    TickRunHandler TickRun);

public static class GameBootstrapper
{
    public static GameServices Build()
    {
        var levelRepository = CreateLevelRepository();
        var runStateStore = new InMemoryRunStateStore();
        var physicsEngine = new NaivePhysicsEngine();
        var timeProvider = new SystemTimeProvider();

        var startRun = new StartRunHandler(levelRepository, runStateStore, timeProvider);
        var tickRun = new TickRunHandler(levelRepository, runStateStore, physicsEngine, timeProvider);

        return new GameServices(levelRepository, runStateStore, startRun, tickRun);
    }

    private static ILevelRepository CreateLevelRepository()
    {
        var baseDir = AppContext.BaseDirectory;
        var levelsPath = Path.Combine(baseDir, "content", "levels");
        var fallback = LevelPresets.CreateDefault();

        return new FileSystemLevelRepository(levelsPath, fallback);
    }
}
