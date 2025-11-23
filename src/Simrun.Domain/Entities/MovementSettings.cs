namespace Simrun.Domain.Entities;

public sealed record MovementSettings(
    float MaxSpeed,
    float Acceleration,
    float Gravity,
    float JumpStrength,
    float AirControl)
{
    public static MovementSettings Default => new(12f, 35f, 32f, 15f, 0.5f);
}
