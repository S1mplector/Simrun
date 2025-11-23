using System;

namespace Simrun.Domain.ValueObjects;

public readonly record struct Vector3(float X, float Y, float Z)
{
    public static Vector3 Zero => new(0f, 0f, 0f);
    public static Vector3 Up => new(0f, 1f, 0f);

    public float Magnitude() => MathF.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public Vector3 Normalize()
    {
        var magnitude = Magnitude();
        return magnitude <= float.Epsilon ? this : Scale(1f / magnitude);
    }

    public Vector3 Add(Vector3 other) => new(X + other.X, Y + other.Y, Z + other.Z);

    public Vector3 Scale(float scalar) => new(X * scalar, Y * scalar, Z * scalar);

    public Vector3 WithY(float y) => new(X, y, Z);
}
