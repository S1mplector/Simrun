using System;

namespace Simrun.Application.Models;

public readonly record struct PlayerInput(float Strafe, float Forward, bool Jump, bool Sprint = false)
{
    public bool HasMovement => MathF.Abs(Strafe) + MathF.Abs(Forward) > float.Epsilon;
}
