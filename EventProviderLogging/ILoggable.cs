using Microsoft.Extensions.Logging;

namespace EventProviderLogging;

public interface ILoggable<out T>
{
    ILogger<T>? Logger { get; }
}

public interface ILoggable
{
    ILogger? Logger { get; }
}
