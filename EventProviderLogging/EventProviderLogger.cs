using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace EventProviderLogging;

public sealed class EventProviderLogger : ILogger
{
    private readonly Func<EventProviderLoggerConfiguration> _getCurrentConfig;
    private readonly ConcurrentDictionary<Guid, EventProvider> _providers = new();

    public EventProviderLogger(string name, Func<EventProviderLoggerConfiguration> getCurrentConfig)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(getCurrentConfig);

        Name = name;
        _getCurrentConfig = getCurrentConfig;
    }

    public string Name { get; }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
    public bool IsEnabled(LogLevel logLevel) => true;

    private void WriteMessage(string message, byte level)
    {
        var guid = _getCurrentConfig().ProviderId;
        if (guid == Guid.Empty)
            return;

        if (!_providers.TryGetValue(guid, out var provider))
        {
            provider = new EventProvider(guid);
            var updated = _providers.AddOrUpdate(guid, provider, (k, o) => o);
            if (updated != provider)
            {
                provider.Dispose();
            }
            provider = updated;
        }
        provider.WriteMessageEvent(message, level);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        // formatter just ignores exception, so, to make it clear we do not send the
        // exception anymore
        // see: https://github.com/aspnet/Logging/issues/442
        // see also: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExtensions.cs
        var message = formatter(state, null);
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (exception != null)
        {
            message = message + Environment.NewLine + exception.ToString();
        }

        var format = _getCurrentConfig().OutputFormat;
        if (format != null)
        {
            var formatProvider = _getCurrentConfig().FormatProvider;
            message = string.Format(formatProvider, format, message, Name, logLevel, eventId);
        }
        else
        {
            var outputName = _getCurrentConfig().OutputName;
            var outputLogLevel = _getCurrentConfig().OutputLogLevel;
            if (outputLogLevel)
            {
                message = $"({logLevel}) {message}";
            }

            if (outputName)
            {
                message = $"[{Name}] {message}";
            }
        }

        WriteMessage(message, (byte)logLevel);
    }
}
