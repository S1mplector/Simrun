using Simrun.Application.Models;
using Simrun.Domain.Entities;

namespace Simrun.Application.Ports;

public interface IPhysicsEngine
{
    PlayerState Step(LevelDefinition level, PlayerState player, PlayerInput input, float deltaSeconds);
}
