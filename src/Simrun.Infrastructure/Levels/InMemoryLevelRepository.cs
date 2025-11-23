using System.Collections.Generic;
using System.Linq;
using Simrun.Application.Ports;
using Simrun.Domain.Entities;

namespace Simrun.Infrastructure.Levels;

public sealed class InMemoryLevelRepository : ILevelRepository
{
    private readonly Dictionary<string, LevelDefinition> _levels;

    public InMemoryLevelRepository(IEnumerable<LevelDefinition> levels)
    {
        _levels = levels.ToDictionary(level => level.Id);
    }

    public LevelDefinition? FindById(string id) => _levels.TryGetValue(id, out var level) ? level : null;

    public IReadOnlyCollection<LevelDefinition> List() => _levels.Values.ToArray();

    public LevelDefinition? FindFirst() => _levels.Values.FirstOrDefault();
}
