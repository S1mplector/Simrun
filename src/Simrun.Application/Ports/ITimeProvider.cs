using System;

namespace Simrun.Application.Ports;

public interface ITimeProvider
{
    DateTimeOffset Now { get; }
}
