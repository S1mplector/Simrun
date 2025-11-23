using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Simrun.Application.Ports;
using Simrun.Domain.Entities;
using Simrun.Domain.ValueObjects;

namespace Simrun.Infrastructure.Levels;

public sealed class FileSystemLevelRepository : ILevelRepository
{
    private readonly InMemoryLevelRepository _inner;

    public FileSystemLevelRepository(string levelsDirectory, IEnumerable<LevelDefinition> fallbackLevels)
    {
        var loaded = LoadLevels(levelsDirectory);
        var levels = loaded.Count > 0 ? loaded : fallbackLevels.ToList();
        _inner = new InMemoryLevelRepository(levels);
    }

    public LevelDefinition? FindById(string id) => _inner.FindById(id);

    public IReadOnlyCollection<LevelDefinition> List() => _inner.List();

    public LevelDefinition? FindFirst() => _inner.FindFirst();

    private static List<LevelDefinition> LoadLevels(string levelsDirectory)
    {
        var levels = new List<LevelDefinition>();

        if (!Directory.Exists(levelsDirectory))
        {
            return levels;
        }

        foreach (var file in Directory.EnumerateFiles(levelsDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var json = File.ReadAllText(file);
                var model = JsonSerializer.Deserialize<LevelFile>(json, SerializerOptions);
                if (model is null)
                {
                    continue;
                }

                var level = model.ToDomain(Path.GetFileNameWithoutExtension(file));
                levels.Add(level);
            }
            catch
            {
                // Ignore malformed or unreadable files to keep loading best-effort.
            }
        }

        return levels;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };
}

internal sealed class LevelFile
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public VectorData? Spawn { get; set; }
    public GoalData? Goal { get; set; }
    public float FloorHeight { get; set; }
    public MovementData? Movement { get; set; }
    public CollisionBoxData[]? Colliders { get; set; }

    public LevelDefinition ToDomain(string? fallbackId)
    {
        var id = Id ?? fallbackId ?? Guid.NewGuid().ToString("N");
        var name = Name ?? id;

        var spawn = Spawn?.ToVector() ?? Vector3.Zero;
        var goal = Goal ?? new GoalData { Position = new VectorData(10f, 0f, 10f), Radius = 2f };
        var movement = Movement?.ToDomain() ?? MovementSettings.Default;
        var colliders = Colliders?.Select(c => c.ToDomain()) ?? Array.Empty<CollisionBox>();

        return new LevelDefinition(id, name, spawn, goal.ToVector(), goal.Radius, FloorHeight, movement, colliders);
    }
}

internal sealed record VectorData(float X, float Y, float Z)
{
    public VectorData() : this(0f, 0f, 0f)
    {
    }

    public Vector3 ToVector() => new(X, Y, Z);
}

internal sealed record GoalData
{
    public VectorData Position { get; set; } = new(0f, 0f, 0f);
    public float Radius { get; set; } = 1f;

    public Vector3 ToVector() => Position.ToVector();
}

internal sealed record MovementData
{
    public float? MaxSpeed { get; set; }
    public float? Acceleration { get; set; }
    public float? Gravity { get; set; }
    public float? JumpStrength { get; set; }
    public float? AirControl { get; set; }

    public MovementSettings ToDomain()
    {
        var defaults = MovementSettings.Default;
        return new MovementSettings(
            MaxSpeed ?? defaults.MaxSpeed,
            Acceleration ?? defaults.Acceleration,
            Gravity ?? defaults.Gravity,
            JumpStrength ?? defaults.JumpStrength,
            AirControl ?? defaults.AirControl);
    }
}

internal sealed record CollisionBoxData
{
    public VectorData Center { get; set; } = new(0f, 0f, 0f);
    public VectorData HalfSize { get; set; } = new(1f, 1f, 1f);

    public CollisionBox ToDomain() => new(Center.ToVector(), HalfSize.ToVector());
}
