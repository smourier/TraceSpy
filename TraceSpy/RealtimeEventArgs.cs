using System;

#if TRACESPY_SERVICE
namespace TraceSpyService
#else
namespace TraceSpy
#endif
{
    public sealed class RealtimeEventArgs : EventArgs
    {
        public RealtimeEventArgs(int processId, int threadId, string message)
        {
            ProcessId = processId;
            ThreadId = threadId;
            Message = message;
        }

        public string Message { get; private set; }
        public int ProcessId { get; private set; }
        public int ThreadId { get; private set; }
    }
}
