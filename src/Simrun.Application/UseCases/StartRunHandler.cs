using System;
using Simrun.Application.Ports;
using Simrun.Domain.Entities;

namespace Simrun.Application.UseCases;

public sealed class StartRunHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IRunStateStore _runStateStore;
    private readonly ITimeProvider _timeProvider;

    public StartRunHandler(ILevelRepository levelRepository, IRunStateStore runStateStore, ITimeProvider timeProvider)
    {
        _levelRepository = levelRepository;
        _runStateStore = runStateStore;
        _timeProvider = timeProvider;
    }

    public RunState Start(string? levelId = null)
    {
        var level = levelId is null
            ? _levelRepository.FindFirst()
            : _levelRepository.FindById(levelId);

        if (level is null)
        {
            throw new InvalidOperationException("No levels are available to start a run.");
        }

        var run = RunState.Start(level.Id, level.SpawnPoint, _timeProvider.Now);
        _runStateStore.Save(run);
        return run;
    }
}
