using System;
using System.Numerics;
using Simrun.Engine.Interop;
using Simrun.Engine.Rendering;
using DomainVector = Simrun.Domain.ValueObjects.Vector3;

namespace Simrun.Presentation.Cameras;

/// <summary>
/// Simple chase-camera that orbits behind the player's velocity vector with damped smoothing.
/// </summary>
internal sealed class CameraRig
{
    private float _yaw;
    private float _pitch = -0.25f;
    private readonly Vector3 _offset = new(0f, 3f, -10f);
    private Vector3 _currentPosition = Vector3.Zero;
    private Quaternion _currentRotation = Quaternion.Identity;

    public void Update(Camera camera, DomainVector targetPosition, DomainVector targetVelocity, Simrun.Presentation.Input.LookInput look, float deltaSeconds)
    {
        const float smoothing = 10f;
        const float maxPitch = 1.2f;

        _yaw += look.YawDelta;
        _pitch = Math.Clamp(_pitch - look.PitchDelta, -maxPitch, maxPitch);

        var horizontalVelocity = new Vector3(targetVelocity.X, 0f, targetVelocity.Z);
        if (horizontalVelocity.LengthSquared() > 0.01f)
        {
            _yaw = MathF.Atan2(horizontalVelocity.X, horizontalVelocity.Z);
        }

        var rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
        var worldOffset = Vector3.Transform(_offset, rotation);

        var desiredPosition = targetPosition.ToEngine() + worldOffset;
        var lerpFactor = 1f - MathF.Exp(-smoothing * deltaSeconds);

        _currentPosition = Vector3.Lerp(_currentPosition, desiredPosition, lerpFactor);
        _currentRotation = Quaternion.Slerp(_currentRotation, rotation, lerpFactor);

        camera.Transform.Position = _currentPosition;
        camera.Transform.Rotation = _currentRotation;
    }

    public void FaceDirection(DomainVector direction)
    {
        var flat = new Vector3(direction.X, 0f, direction.Z);
        if (flat.LengthSquared() > 0.001f)
        {
            _yaw = MathF.Atan2(flat.X, flat.Z);
        }
    }
}
