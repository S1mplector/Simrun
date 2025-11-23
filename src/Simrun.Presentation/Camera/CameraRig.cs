using System;
using System.Numerics;
using Simrun.Engine.Interop;
using Simrun.Engine.Rendering;
using DomainVector = Simrun.Domain.ValueObjects.Vector3;

namespace Simrun.Presentation.Camera;

/// <summary>
/// Simple chase-camera that orbits behind the player's velocity vector.
/// </summary>
internal sealed class CameraRig
{
    private float _yaw;
    private float _pitch = -0.25f;
    private readonly Vector3 _offset = new(0f, 3f, -10f);

    public void Update(Camera camera, DomainVector targetPosition, DomainVector targetVelocity)
    {
        var horizontalVelocity = new Vector3(targetVelocity.X, 0f, targetVelocity.Z);
        if (horizontalVelocity.LengthSquared() > 0.01f)
        {
            _yaw = MathF.Atan2(horizontalVelocity.X, horizontalVelocity.Z);
        }

        var rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
        var worldOffset = Vector3.Transform(_offset, rotation);

        camera.Transform.Position = targetPosition.ToEngine() + worldOffset;
        camera.Transform.Rotation = rotation;
    }
}
