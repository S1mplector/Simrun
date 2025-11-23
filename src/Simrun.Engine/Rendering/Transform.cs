using System.Numerics;

namespace Simrun.Engine.Rendering;

public sealed class Transform
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

    public Matrix4x4 ToMatrix()
    {
        var scaleMatrix = Matrix4x4.CreateScale(Scale);
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(Rotation);
        var translationMatrix = Matrix4x4.CreateTranslation(Position);
        return scaleMatrix * rotationMatrix * translationMatrix;
    }
}
