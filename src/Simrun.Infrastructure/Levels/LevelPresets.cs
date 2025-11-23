using System.Collections.Generic;
using Simrun.Domain.Entities;
using Simrun.Domain.ValueObjects;

namespace Simrun.Infrastructure.Levels;

public static class LevelPresets
{
    public static IReadOnlyCollection<LevelDefinition> CreateDefault()
    {
        var defaultMovement = MovementSettings.Default;

        return new[]
        {
            new LevelDefinition(
                id: "level-1",
                name: "Crystal Runway",
                spawnPoint: new Vector3(0f, 2f, 0f),
                goalPosition: new Vector3(45f, 2f, 0f),
                goalRadius: 1.5f,
                floorHeight: 0f,
                movement: defaultMovement),
            new LevelDefinition(
                id: "level-2",
                name: "Sunset Platforms",
                spawnPoint: new Vector3(-5f, 8f, 0f),
                goalPosition: new Vector3(30f, 12f, 20f),
                goalRadius: 2f,
                floorHeight: 0f,
                movement: defaultMovement with { JumpStrength = 18f, AirControl = 0.7f }),
            new LevelDefinition(
                id: "level-3",
                name: "Starlit Cavern",
                spawnPoint: new Vector3(0f, 16f, -20f),
                goalPosition: new Vector3(55f, 18f, 15f),
                goalRadius: 2.5f,
                floorHeight: -2f,
                movement: defaultMovement with { Gravity = 24f, MaxSpeed = 14f })
        };
    }
}
