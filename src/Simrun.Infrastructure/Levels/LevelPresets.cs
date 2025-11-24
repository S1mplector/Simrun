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
                movement: defaultMovement,
                colliders: new[]
                {
                    new CollisionBox(new Vector3(0f, -1f, 0f), new Vector3(200f, 1f, 200f)), // floor
                    new CollisionBox(new Vector3(15f, 2f, 0f), new Vector3(4f, 2f, 4f)),     // mid platform
                    new CollisionBox(new Vector3(25f, 3f, 0f), new Vector3(4f, 2f, 4f)),     // mid platform 2
                }),
            new LevelDefinition(
                id: "level-2",
                name: "Sunset Platforms",
                spawnPoint: new Vector3(-5f, 8f, 0f),
                goalPosition: new Vector3(30f, 12f, 20f),
                goalRadius: 2f,
                floorHeight: 0f,
                movement: defaultMovement with { JumpStrength = 18f, AirControl = 0.7f },
                colliders: new[]
                {
                    new CollisionBox(new Vector3(0f, 0f, 0f), new Vector3(200f, 0.5f, 200f)),
                    new CollisionBox(new Vector3(12f, 10f, 8f), new Vector3(6f, 0.5f, 6f)),
                    new CollisionBox(new Vector3(24f, 14f, 16f), new Vector3(4f, 0.5f, 4f))
                }),
            new LevelDefinition(
                id: "level-3",
                name: "Starlit Cavern",
                spawnPoint: new Vector3(0f, 16f, -20f),
                goalPosition: new Vector3(55f, 18f, 15f),
                goalRadius: 2.5f,
                floorHeight: -2f,
                movement: defaultMovement with { Gravity = 24f, MaxSpeed = 14f },
                colliders: new[]
                {
                    new CollisionBox(new Vector3(0f, -3f, 0f), new Vector3(200f, 1f, 200f)),
                    new CollisionBox(new Vector3(10f, 10f, -10f), new Vector3(5f, 0.5f, 5f)),
                    new CollisionBox(new Vector3(30f, 14f, 0f), new Vector3(6f, 0.5f, 6f)),
                    new CollisionBox(new Vector3(50f, 18f, 12f), new Vector3(8f, 0.5f, 8f))
                })
        };
    }
}
