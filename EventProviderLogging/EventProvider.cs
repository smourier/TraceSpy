using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace EventProviderLogging;

// this class provides simple ETW event logging capabilities directly (without using ILogger and Microsoft Logging Abstractions)
public sealed partial class EventProvider : IDisposable
{
    public static Guid DefaultProviderId { get; set; } = new Guid("964d4572-adb9-4f3a-8170-fcbecec27467");

    public static EventProvider Current => _current.Value;
    private static readonly Lazy<EventProvider> _current = new(() => new EventProvider(DefaultProviderId));

    private long _handle;
    public Guid Id { get; }

    public EventProvider(Guid id)
    {
        Id = id;
        var hr = EventRegister(id, nint.Zero, nint.Zero, out _handle);
        if (hr != 0)
            throw new Win32Exception(hr);
    }

    public bool WriteMessageEvent(string text, byte level = 0, long keywords = 0) => EventWriteString(_handle, level, keywords, text) == 0;

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, 0);
        if (handle != 0)
        {
            _ = EventUnregister(handle);
        }
    }

    [LibraryImport("advapi32")]
    private static partial int EventRegister(in Guid ProviderId, nint EnableCallback, nint CallbackContext, out long RegHandle);

    [LibraryImport("advapi32")]
    private static partial int EventUnregister(long RegHandle);

    [LibraryImport("advapi32", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int EventWriteString(long RegHandle, byte Level, long Keyword, string String);
}
