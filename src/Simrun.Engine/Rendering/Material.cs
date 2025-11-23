using System.Numerics;

namespace Simrun.Engine.Rendering;

public sealed class Material
{
    public Vector3 Albedo { get; set; } = new(1f, 1f, 1f);
    public float Roughness { get; set; } = 0.5f;
    public float Metallic { get; set; } = 0f;
}
