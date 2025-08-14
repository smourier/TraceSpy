using System;

#if TRACESPY_SERVICE
namespace TraceSpyService
#else
namespace TraceSpy
#endif
{
    public sealed class EventRealtimeEventArgs(int processId, int threadId, string message, EtwTraceLevel level) : EventArgs
    {
        public string Message { get; } = message;
        public int ProcessId { get; } = processId;
        public int ThreadId { get; } = threadId;
        public EtwTraceLevel Level { get; } = level;
    }
}
