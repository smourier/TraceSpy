using System;

#if TRACESPY_SERVICE
namespace TraceSpyService
#else
namespace TraceSpy
#endif
{
    public enum EtwTraceLevel : byte
    {
        None = 0,
        Fatal = 1,
        Error = 2,
        Warning = 3,
        Information = 4,
        Verbose = 5,
    }
}
