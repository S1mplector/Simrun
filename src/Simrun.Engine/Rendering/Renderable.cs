namespace Simrun.Engine.Rendering;

public sealed class Renderable
{
    public Renderable(Mesh mesh, Material material, Transform transform, bool isDebug = false)
    {
        Mesh = mesh;
        Material = material;
        Transform = transform;
        IsDebug = isDebug;
    }

    public Mesh Mesh { get; }
    public Material Material { get; }
    public Transform Transform { get; }
    public bool IsDebug { get; }
}
