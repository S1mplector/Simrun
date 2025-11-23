using System;
using Simrun.Application.Ports;

namespace Simrun.Infrastructure.Time;

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
