using Simrun.Domain.Entities;

namespace Simrun.Application.Ports;

public interface IRunStateStore
{
    RunState? LoadCurrent();

    void Save(RunState state);

    void Clear();
}
