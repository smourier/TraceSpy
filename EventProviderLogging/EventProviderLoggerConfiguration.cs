using System;

namespace EventProviderLogging;

public class EventProviderLoggerConfiguration
{
    public virtual Guid ProviderId { get; set; } = EventProvider.DefaultProviderId;
    public virtual bool OutputName { get; set; } = true;
    public virtual bool OutputLogLevel { get; set; } = true;
    public virtual string? OutputFormat { get; set; } // {0} = message, {1} = name, {2} = logLevel, {3] = eventId
    public virtual IFormatProvider? FormatProvider { get; set; }
}
