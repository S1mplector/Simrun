using Simrun.Domain.ValueObjects;

namespace Simrun.Domain.Entities;

public readonly record struct CollisionBox(Vector3 Center, Vector3 HalfSize)
{
    public Vector3 Min => new(Center.X - HalfSize.X, Center.Y - HalfSize.Y, Center.Z - HalfSize.Z);
    public Vector3 Max => new(Center.X + HalfSize.X, Center.Y + HalfSize.Y, Center.Z + HalfSize.Z);

    public bool Contains(Vector3 point)
    {
        var min = Min;
        var max = Max;
        return point.X >= min.X && point.X <= max.X &&
               point.Y >= min.Y && point.Y <= max.Y &&
               point.Z >= min.Z && point.Z <= max.Z;
    }
}
