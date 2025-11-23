using Simrun.Domain.ValueObjects;

namespace Simrun.Domain.Entities;

public sealed class LevelDefinition
{
    public LevelDefinition(
        string id,
        string name,
        Vector3 spawnPoint,
        Vector3 goalPosition,
        float goalRadius,
        float floorHeight,
        MovementSettings movement)
    {
        Id = id;
        Name = name;
        SpawnPoint = spawnPoint;
        GoalPosition = goalPosition;
        GoalRadius = goalRadius;
        FloorHeight = floorHeight;
        Movement = movement;
    }

    public string Id { get; }
    public string Name { get; }
    public Vector3 SpawnPoint { get; }
    public Vector3 GoalPosition { get; }
    public float GoalRadius { get; }
    public float FloorHeight { get; }
    public MovementSettings Movement { get; }

    public bool IsGoalReached(Vector3 position)
    {
        var offset = new Vector3(position.X - GoalPosition.X, 0f, position.Z - GoalPosition.Z);
        return offset.Magnitude() <= GoalRadius && position.Y >= FloorHeight - 0.5f;
    }
}
