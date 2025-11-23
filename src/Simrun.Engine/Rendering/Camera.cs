using System;
using System.Numerics;

namespace Simrun.Engine.Rendering;

public sealed class Camera
{
    public Transform Transform { get; } = new();
    public float FieldOfView { get; set; } = MathF.PI / 3f;
    public float NearClip { get; set; } = 0.1f;
    public float FarClip { get; set; } = 500f;

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Forward, Vector3.UnitY);

    public Matrix4x4 ProjectionMatrix(float aspectRatio) =>
        Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearClip, FarClip);

    public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Transform.Rotation);
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Transform.Rotation);
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Transform.Rotation);
}
