using System.Collections.Generic;
using Simrun.Domain.Entities;

namespace Simrun.Application.Ports;

public interface ILevelRepository
{
    LevelDefinition? FindById(string id);

    IReadOnlyCollection<LevelDefinition> List();

    LevelDefinition? FindFirst();
}
