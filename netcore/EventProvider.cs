using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace TraceSpy
{
    // !note: this only works on Windows!
    public class EventProvider : IDisposable
    {
        private long _handle;

        public EventProvider(Guid id)
        {
            Id = id;
            int hr = EventRegister(id, IntPtr.Zero, IntPtr.Zero, out _handle);
            if (hr != 0)
                throw new Win32Exception(hr);
        }

        public Guid Id { get; }

        public bool WriteMessageEvent(string text, byte level = 0, long keywords = 0) => EventWriteString(_handle, level, keywords, text) == 0;

        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, 0);
            if (handle != 0)
            {
                EventUnregister(handle);
            }
        }

        [DllImport("advapi32")]
        private static extern int EventRegister([MarshalAs(UnmanagedType.LPStruct)] Guid ProviderId, IntPtr EnableCallback, IntPtr CallbackContext, out long RegHandle);

        [DllImport("advapi32")]
        private static extern int EventUnregister(long RegHandle);

        [DllImport("advapi32")]
        private static extern int EventWriteString(long RegHandle, byte Level, long Keyword, [MarshalAs(UnmanagedType.LPWStr)] string String);
    }

    public class EventProviderLogger : ILogger
    {
        public EventProviderLogger(EventProvider provider, string category)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            Provider = provider;
            Category = category;
        }

        public EventProvider Provider { get; }
        public string Category { get; }

        public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message))
                return;

            message = $"{ logLevel }: {message}";
            if (exception != null)
            {
                message += Environment.NewLine + Environment.NewLine + exception;
            }
            Provider.WriteMessageEvent(message);
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();
            public void Dispose() { }
        }
    }

    public class EventProviderLoggerProvider : ILoggerProvider
    {
        public EventProviderLoggerProvider(Guid id)
        {
            Provider = new EventProvider(id);
        }

        public EventProvider Provider { get; private set; }

        public ILogger CreateLogger(string categoryName) => new EventProviderLogger(Provider, categoryName);

        public void Dispose()
        {
            Provider?.Dispose();
            Provider = null;
        }
    }

    public static class EventProviderExtensions
    {
        public static void AddEventProvider(this ILoggerFactory factory, Guid providerId)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            // fail gracefully
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            factory.AddProvider(new EventProviderLoggerProvider(providerId));
        }
    }
}
