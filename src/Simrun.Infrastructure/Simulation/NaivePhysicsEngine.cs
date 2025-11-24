using System;
using Simrun.Application.Models;
using Simrun.Application.Ports;
using Simrun.Domain.Entities;
using Simrun.Domain.ValueObjects;

namespace Simrun.Infrastructure.Simulation;

public sealed class NaivePhysicsEngine : IPhysicsEngine
{
    private const float CapsuleRadius = 0.5f;
    private const float CapsuleHalfHeight = 0.9f;
    private const float GroundSnapTolerance = 0.05f;
    private const float MaxStepDistance = 0.5f;

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
        var totalDisplacement = velocity.Scale(deltaSeconds);
        var totalDistance = totalDisplacement.Magnitude();
        var steps = Math.Max(1, (int)MathF.Ceiling(totalDistance / MaxStepDistance));
        var stepDt = deltaSeconds / steps;

        var position = player.Position;
        var grounded = player.IsGrounded;
        var distanceTravelled = player.DistanceTravelled;

        for (var i = 0; i < steps; i++)
        {
            var previous = position;
            var displacement = velocity.Scale(stepDt);

            var sweep = SweepCapsule(level, position, displacement);
            var segmentNext = sweep.Position;
            if (sweep.Hit)
            {
                velocity = SlideAlongNormal(velocity, sweep.Normal);
                if (sweep.Normal.Y > 0.2f)
                {
                    grounded = true;
                }
            }

            var groundedSegment = segmentNext.Y <= level.FloorHeight + GroundSnapTolerance;

            if (groundedSegment)
            {
                segmentNext = segmentNext.WithY(level.FloorHeight);
                velocity = new Vector3(velocity.X, 0f, velocity.Z);
                grounded = true;
            }

            (segmentNext, velocity, grounded) = ResolveCollisions(level, segmentNext, velocity, grounded);

            var horizontalDelta = new Vector3(segmentNext.X - previous.X, 0f, segmentNext.Z - previous.Z);
            distanceTravelled += horizontalDelta.Magnitude();
            position = segmentNext;
        }

        return new PlayerState(position, velocity, grounded, distanceTravelled);
    }

    private static (Vector3 position, Vector3 velocity, bool grounded) ResolveCollisions(
        LevelDefinition level,
        Vector3 candidatePosition,
        Vector3 velocity,
        bool grounded)
    {
        var position = candidatePosition;
        var playerMin = new Vector3(position.X - CapsuleRadius, position.Y - CapsuleHalfHeight, position.Z - CapsuleRadius);
        var playerMax = new Vector3(position.X + CapsuleRadius, position.Y + CapsuleHalfHeight, position.Z + CapsuleRadius);

        foreach (var collider in level.Colliders)
        {
            var min = collider.Min;
            var max = collider.Max;

            var expandedMin = new Vector3(min.X - CapsuleRadius, min.Y - CapsuleHalfHeight, min.Z - CapsuleRadius);
            var expandedMax = new Vector3(max.X + CapsuleRadius, max.Y + CapsuleHalfHeight, max.Z + CapsuleRadius);

            if (!AabbOverlap(playerMin, playerMax, expandedMin, expandedMax))
            {
                continue;
            }

            var pushX = MathF.Min(expandedMax.X - playerMin.X, playerMax.X - expandedMin.X);
            var pushY = MathF.Min(expandedMax.Y - playerMin.Y, playerMax.Y - expandedMin.Y);
            var pushZ = MathF.Min(expandedMax.Z - playerMin.Z, playerMax.Z - expandedMin.Z);

            if (pushX <= pushY && pushX <= pushZ)
            {
                var sign = (position.X > collider.Center.X) ? 1f : -1f;
                position = new Vector3(position.X + (pushX * sign), position.Y, position.Z);
                velocity = new Vector3(0f, velocity.Y, velocity.Z);
                playerMin = new Vector3(position.X - CapsuleRadius, position.Y - CapsuleHalfHeight, position.Z - CapsuleRadius);
                playerMax = new Vector3(position.X + CapsuleRadius, position.Y + CapsuleHalfHeight, position.Z + CapsuleRadius);
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
                playerMin = new Vector3(position.X - CapsuleRadius, position.Y - CapsuleHalfHeight, position.Z - CapsuleRadius);
                playerMax = new Vector3(position.X + CapsuleRadius, position.Y + CapsuleHalfHeight, position.Z + CapsuleRadius);
            }
            else
            {
                var sign = (position.Z > collider.Center.Z) ? 1f : -1f;
                position = new Vector3(position.X, position.Y, position.Z + (pushZ * sign));
                velocity = new Vector3(velocity.X, velocity.Y, 0f);
                playerMin = new Vector3(position.X - CapsuleRadius, position.Y - CapsuleHalfHeight, position.Z - CapsuleRadius);
                playerMax = new Vector3(position.X + CapsuleRadius, position.Y + CapsuleHalfHeight, position.Z + CapsuleRadius);
            }
        }

        return (position, velocity, grounded);
    }

    private static (bool Hit, Vector3 Position, Vector3 Normal) SweepCapsule(LevelDefinition level, Vector3 start, Vector3 displacement)
    {
        var bestT = float.MaxValue;
        Vector3 bestNormal = Vector3.Zero;

        var expandedColliders = level.Colliders;

        foreach (var collider in expandedColliders)
        {
            var expandedMin = new Vector3(collider.Min.X - CapsuleRadius, collider.Min.Y - CapsuleHalfHeight, collider.Min.Z - CapsuleRadius);
            var expandedMax = new Vector3(collider.Max.X + CapsuleRadius, collider.Max.Y + CapsuleHalfHeight, collider.Max.Z + CapsuleRadius);

            if (RayAabb(start, displacement, expandedMin, expandedMax, out var tHit, out var normal))
            {
                if (tHit < bestT)
                {
                    bestT = tHit;
                    bestNormal = normal;
                }
            }
        }

        if (bestT <= 1f)
        {
            var pos = start.Add(displacement.Scale(bestT));
            return (true, pos, bestNormal);
        }

        return (false, start.Add(displacement), Vector3.Zero);
    }

    private static bool RayAabb(Vector3 origin, Vector3 dir, Vector3 min, Vector3 max, out float tHit, out Vector3 normal)
    {
        tHit = 0f;
        normal = Vector3.Zero;

        var tMin = (min.X - origin.X) / (dir.X == 0 ? float.Epsilon : dir.X);
        var tMax = (max.X - origin.X) / (dir.X == 0 ? float.Epsilon : dir.X);
        var nx = dir.X >= 0 ? -1f : 1f;
        if (tMin > tMax)
        {
            (tMin, tMax) = (tMax, tMin);
            nx = -nx;
        }

        var tyMin = (min.Y - origin.Y) / (dir.Y == 0 ? float.Epsilon : dir.Y);
        var tyMax = (max.Y - origin.Y) / (dir.Y == 0 ? float.Epsilon : dir.Y);
        var ny = dir.Y >= 0 ? -1f : 1f;
        if (tyMin > tyMax)
        {
            (tyMin, tyMax) = (tyMax, tyMin);
            ny = -ny;
        }

        if ((tMin > tyMax) || (tyMin > tMax))
        {
            return false;
        }

        if (tyMin > tMin)
        {
            tMin = tyMin;
            nx = 0f;
        }
        if (tyMax < tMax)
        {
            tMax = tyMax;
        }

        var tzMin = (min.Z - origin.Z) / (dir.Z == 0 ? float.Epsilon : dir.Z);
        var tzMax = (max.Z - origin.Z) / (dir.Z == 0 ? float.Epsilon : dir.Z);
        var nz = dir.Z >= 0 ? -1f : 1f;
        if (tzMin > tzMax)
        {
            (tzMin, tzMax) = (tzMax, tzMin);
            nz = -nz;
        }

        if ((tMin > tzMax) || (tzMin > tMax))
        {
            return false;
        }

        if (tzMin > tMin)
        {
            tMin = tzMin;
            nx = 0f;
            ny = 0f;
        }
        if (tzMax < tMax)
        {
            tMax = tzMax;
        }

        tHit = tMin;
        if (tHit < 0f || tHit > 1f)
        {
            return false;
        }

        if (MathF.Abs(nx) > 0f) normal = new Vector3(nx, 0f, 0f);
        else if (MathF.Abs(ny) > 0f) normal = new Vector3(0f, ny, 0f);
        else normal = new Vector3(0f, 0f, nz);

        return true;
    }

    private static Vector3 SlideAlongNormal(Vector3 velocity, Vector3 normal)
    {
        var dot = (velocity.X * normal.X) + (velocity.Y * normal.Y) + (velocity.Z * normal.Z);
        return velocity.Add(normal.Scale(-dot));
    }

    private static bool AabbOverlap(Vector3 minA, Vector3 maxA, Vector3 minB, Vector3 maxB)
    {
        return minA.X <= maxB.X && maxA.X >= minB.X &&
               minA.Y <= maxB.Y && maxA.Y >= minB.Y &&
               minA.Z <= maxB.Z && maxA.Z >= minB.Z;
    }
}
