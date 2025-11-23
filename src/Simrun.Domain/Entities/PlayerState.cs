using Simrun.Domain.ValueObjects;

namespace Simrun.Domain.Entities;

public sealed class PlayerState
{
    public PlayerState(Vector3 position, Vector3 velocity, bool isGrounded, float distanceTravelled = 0f)
    {
        Position = position;
        Velocity = velocity;
        IsGrounded = isGrounded;
        DistanceTravelled = distanceTravelled;
    }

    public Vector3 Position { get; }
    public Vector3 Velocity { get; }
    public bool IsGrounded { get; }
    public float DistanceTravelled { get; }

    public PlayerState With(Vector3? position = null, Vector3? velocity = null, bool? isGrounded = null, float? distanceTravelled = null)
    {
        return new PlayerState(
            position ?? Position,
            velocity ?? Velocity,
            isGrounded ?? IsGrounded,
            distanceTravelled ?? DistanceTravelled);
    }
}
