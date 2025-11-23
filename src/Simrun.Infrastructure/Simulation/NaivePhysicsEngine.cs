using System;
using Simrun.Application.Models;
using Simrun.Application.Ports;
using Simrun.Domain.Entities;
using Simrun.Domain.ValueObjects;

namespace Simrun.Infrastructure.Simulation;

public sealed class NaivePhysicsEngine : IPhysicsEngine
{
    public PlayerState Step(LevelDefinition level, PlayerState player, PlayerInput input, float deltaSeconds)
    {
        var movement = level.Movement;
        var wishDirection = new Vector3(input.Strafe, 0f, input.Forward);

        if (wishDirection.Magnitude() > 1f)
        {
            wishDirection = wishDirection.Normalize();
        }

        var appliedAcceleration = movement.Acceleration * deltaSeconds * (player.IsGrounded ? 1f : movement.AirControl);
        var acceleration = wishDirection.Scale(appliedAcceleration);

        var velocity = player.Velocity.Add(acceleration);

        var horizontalSpeed = MathF.Sqrt((velocity.X * velocity.X) + (velocity.Z * velocity.Z));
        var maxSpeed = movement.MaxSpeed * (input.Sprint ? 1.3f : 1f);
        if (horizontalSpeed > maxSpeed && horizontalSpeed > 0f)
        {
            var scale = maxSpeed / horizontalSpeed;
            velocity = new Vector3(velocity.X * scale, velocity.Y, velocity.Z * scale);
        }

        velocity = new Vector3(velocity.X, velocity.Y - (movement.Gravity * deltaSeconds), velocity.Z);

        if (input.Jump && player.IsGrounded)
        {
            velocity = new Vector3(velocity.X, movement.JumpStrength, velocity.Z);
        }

        var nextPosition = player.Position.Add(velocity.Scale(deltaSeconds));
        var grounded = nextPosition.Y <= level.FloorHeight;

        if (grounded)
        {
            nextPosition = nextPosition.WithY(level.FloorHeight);
            velocity = new Vector3(velocity.X, 0f, velocity.Z);
        }

        (nextPosition, velocity, grounded) = ResolveCollisions(level, nextPosition, velocity, grounded);

        var horizontalDelta = new Vector3(nextPosition.X - player.Position.X, 0f, nextPosition.Z - player.Position.Z);
        var distanceTravelled = player.DistanceTravelled + horizontalDelta.Magnitude();

        return new PlayerState(nextPosition, velocity, grounded, distanceTravelled);
    }

    private static (Vector3 position, Vector3 velocity, bool grounded) ResolveCollisions(
        LevelDefinition level,
        Vector3 candidatePosition,
        Vector3 velocity,
        bool grounded)
    {
        var position = candidatePosition;
        foreach (var collider in level.Colliders)
        {
            if (!collider.Contains(position))
            {
                continue;
            }

            var min = collider.Min;
            var max = collider.Max;

            var pushX = MathF.Min(max.X - position.X, position.X - min.X);
            var pushY = MathF.Min(max.Y - position.Y, position.Y - min.Y);
            var pushZ = MathF.Min(max.Z - position.Z, position.Z - min.Z);

            if (pushX <= pushY && pushX <= pushZ)
            {
                var sign = (position.X > collider.Center.X) ? 1f : -1f;
                position = new Vector3(position.X + (pushX * sign), position.Y, position.Z);
                velocity = new Vector3(0f, velocity.Y, velocity.Z);
            }
            else if (pushY <= pushX && pushY <= pushZ)
            {
                var sign = (position.Y > collider.Center.Y) ? 1f : -1f;
                position = new Vector3(position.X, position.Y + (pushY * sign), position.Z);
                velocity = new Vector3(velocity.X, 0f, velocity.Z);
                if (sign > 0f)
                {
                    grounded = true;
                }
            }
            else
            {
                var sign = (position.Z > collider.Center.Z) ? 1f : -1f;
                position = new Vector3(position.X, position.Y, position.Z + (pushZ * sign));
                velocity = new Vector3(velocity.X, velocity.Y, 0f);
            }
        }

        return (position, velocity, grounded);
    }
}
