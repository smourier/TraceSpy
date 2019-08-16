using System;

#if TRACESPY_SERVICE
namespace TraceSpyService
#else
namespace TraceSpy
#endif
{
    public sealed class EventRealtimeEventArgs : EventArgs
    {
        public EventRealtimeEventArgs(int processId, int threadId, string message, EtwTraceLevel level)
        {
            ProcessId = processId;
            ThreadId = threadId;
            Message = message;
            Level = level;
        }

        public string Message { get; }
        public int ProcessId { get; }
        public int ThreadId { get; }
        public EtwTraceLevel Level { get; }
    }
}
