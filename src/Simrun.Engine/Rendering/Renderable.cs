namespace Simrun.Engine.Rendering;

public sealed class Renderable
{
    public Renderable(Mesh mesh, Material material, Transform transform)
    {
        Mesh = mesh;
        Material = material;
        Transform = transform;
    }

    public Mesh Mesh { get; }
    public Material Material { get; }
    public Transform Transform { get; }
}
