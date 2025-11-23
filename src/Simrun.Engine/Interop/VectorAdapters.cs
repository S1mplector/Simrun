using DomainVector = Simrun.Domain.ValueObjects.Vector3;
using EngineVector = System.Numerics.Vector3;

namespace Simrun.Engine.Interop;

public static class VectorAdapters
{
    public static EngineVector ToEngine(this DomainVector vector) => new(vector.X, vector.Y, vector.Z);

    public static DomainVector ToDomain(this EngineVector vector) => new(vector.X, vector.Y, vector.Z);
}
