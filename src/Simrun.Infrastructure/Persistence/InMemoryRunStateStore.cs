using Simrun.Application.Ports;
using Simrun.Domain.Entities;

namespace Simrun.Infrastructure.Persistence;

public sealed class InMemoryRunStateStore : IRunStateStore
{
    private RunState? _current;

    public RunState? LoadCurrent() => _current;

    public void Save(RunState state) => _current = state;

    public void Clear() => _current = null;
}
