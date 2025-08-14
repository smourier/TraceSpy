using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventProviderLogging;

[ProviderAlias("EventProvider")]
public sealed class EventProviderLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private EventProviderLoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, EventProviderLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public EventProviderLoggerProvider(IOptionsMonitor<EventProviderLoggerConfiguration> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    private EventProviderLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new EventProviderLogger(name, GetCurrentConfig));

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}
