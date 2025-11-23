using System;
using Simrun.Application.Models;
using Simrun.Application.Ports;
using Simrun.Domain.Entities;

namespace Simrun.Application.UseCases;

public sealed class TickRunHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IRunStateStore _runStateStore;
    private readonly IPhysicsEngine _physicsEngine;
    private readonly ITimeProvider _timeProvider;

    public TickRunHandler(
        ILevelRepository levelRepository,
        IRunStateStore runStateStore,
        IPhysicsEngine physicsEngine,
        ITimeProvider timeProvider)
    {
        _levelRepository = levelRepository;
        _runStateStore = runStateStore;
        _physicsEngine = physicsEngine;
        _timeProvider = timeProvider;
    }

    public RunState Tick(PlayerInput input, float deltaSeconds)
    {
        if (deltaSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time must be greater than zero.");
        }

        var run = _runStateStore.LoadCurrent() ?? throw new InvalidOperationException("No active run has been started.");
        var level = _levelRepository.FindById(run.LevelId) ?? throw new InvalidOperationException($"Unable to load level '{run.LevelId}'.");

        var nextPlayer = _physicsEngine.Step(level, run.Player, input, deltaSeconds);
        var updated = run.WithPlayer(nextPlayer).Advance(TimeSpan.FromSeconds(deltaSeconds));

        if (level.IsGoalReached(nextPlayer.Position))
        {
            updated = updated.MarkCompleted(_timeProvider.Now);
        }

        _runStateStore.Save(updated);
        return updated;
    }
}
