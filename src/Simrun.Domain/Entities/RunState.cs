using System;
using Simrun.Domain.ValueObjects;

namespace Simrun.Domain.Entities;

public sealed class RunState
{
    private RunState(
        string levelId,
        PlayerState player,
        DateTimeOffset startedAt,
        TimeSpan elapsed,
        bool completed,
        DateTimeOffset? completedAt,
        int deaths)
    {
        LevelId = levelId;
        Player = player;
        StartedAt = startedAt;
        Elapsed = elapsed;
        Completed = completed;
        CompletedAt = completedAt;
        Deaths = deaths;
    }

    public string LevelId { get; }
    public PlayerState Player { get; }
    public DateTimeOffset StartedAt { get; }
    public TimeSpan Elapsed { get; }
    public bool Completed { get; }
    public DateTimeOffset? CompletedAt { get; }
    public int Deaths { get; }

    public static RunState Start(string levelId, Vector3 spawnPoint, DateTimeOffset startedAt)
    {
        var player = new PlayerState(spawnPoint, Vector3.Zero, isGrounded: true);
        return new RunState(levelId, player, startedAt, TimeSpan.Zero, completed: false, completedAt: null, deaths: 0);
    }

    public RunState WithPlayer(PlayerState player)
    {
        return new RunState(LevelId, player, StartedAt, Elapsed, Completed, CompletedAt, Deaths);
    }

    public RunState Advance(TimeSpan delta)
    {
        return new RunState(LevelId, Player, StartedAt, Elapsed + delta, Completed, CompletedAt, Deaths);
    }

    public RunState MarkCompleted(DateTimeOffset completedAt)
    {
        if (Completed)
        {
            return this;
        }

        return new RunState(LevelId, Player, StartedAt, Elapsed, completed: true, completedAt, Deaths);
    }

    public RunState RecordDeath(Vector3 respawnPoint)
    {
        var respawnedPlayer = new PlayerState(respawnPoint, Vector3.Zero, isGrounded: true);
        return new RunState(LevelId, respawnedPlayer, StartedAt, Elapsed, completed: false, CompletedAt, deaths: Deaths + 1);
    }
}
