using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

#if TRACESPY_SERVICE
namespace TraceSpyService
#else
namespace TraceSpy
#endif
{
    public sealed class EventRealtimeListener : IDisposable
    {
        private ulong _handle;
        private bool _traceOn;
        private EventCallback _cb;

        public event EventHandler<RealtimeEventArgs> RealtimeEvent;

        public EventRealtimeListener(Guid providerGuid, string sessionName)
            : this(providerGuid, sessionName, EtwTraceLevel.Verbose, 0)
        {
        }

        public EventRealtimeListener(Guid providerGuid, string sessionName, EtwTraceLevel level)
            : this(providerGuid, sessionName, level, 0)
        {
        }

        public EventRealtimeListener(Guid providerGuid, string sessionName, EtwTraceLevel level, long keyword)
        {
            if (providerGuid == Guid.Empty)
                throw new ArgumentException(null, "guid");

            if (sessionName == null)
                throw new ArgumentNullException("sessionName");

            _cb = OnEvent;
            
            ProviderGuid = providerGuid;
            SessionName = sessionName;

            int status;
            int size;
            IntPtr properties = BuildProperties(false, out size);
            try
            {
                status = StartTrace(out _handle, SessionName, properties);
                if (status != 0)
                {
                    if (status != ERROR_ALREADY_EXISTS)
                        throw new Win32Exception(status);

                    // this can happen if something went wrong on another session with the same name
                    // so let's try to stop this existing thing and restart
                    StopTrace();

                    status = StartTrace(out _handle, SessionName, properties);
                    if (status != 0)
                        throw new Win32Exception(status);
                }
            }
            catch (Exception e)
            {
#if TRACESPY_SERVICE
                Program.Log("EventRealtimeListener e:" + e);
#else
                Main.Log("EventRealtimeListener e:" + e);
#endif
                return;
            }
            finally
            {
                Marshal.FreeCoTaskMem(properties);
            }

            status = EnableTraceEx(providerGuid, IntPtr.Zero, _handle, 1, (byte)level, keyword, keyword, 0, IntPtr.Zero);
            if (status != 0)
                throw new Win32Exception(status);

            _traceOn = true;
        }

        public static void ProcessTraces(Guid providerGuid, string sessionName)
        {
            ProcessTraces(providerGuid, sessionName, EtwTraceLevel.Verbose, 0);
        }

        public static void ProcessTraces(Guid providerGuid, string sessionName, EtwTraceLevel level, long keyword)
        {
            using (EventRealtimeListener listener = new EventRealtimeListener(providerGuid, sessionName, level, keyword))
            {
                listener.ProcessTraces();
            }
        }

        public void ProcessTraces()
        {
            if (!_traceOn)
            {
                bool admin;
#if TRACESPY_SERVICE
                admin = Program.IsAdministrator();
#else
                admin = UacUtilities.IsAdministrator();
#endif

                if (!admin)
                {
                    OnRealtimeEvent(Process.GetCurrentProcess().Id, GetCurrentThreadId(), "ETW Traces will not be displayed. TraceSpy must be run as administrator to display these traces.");
                }
                else
                {
                    OnRealtimeEvent(Process.GetCurrentProcess().Id, GetCurrentThreadId(), "ETW Traces are not started. An error occured during initialization.");
                }
                return;
            }

            EVENT_TRACE_LOGFILE etl = new EVENT_TRACE_LOGFILE();
            etl.EventCallback = _cb;
            etl.LoggerName = SessionName;
            etl.ProcessTraceMode = PROCESS_TRACE_MODE_REAL_TIME;
            long oh = OpenTrace(ref etl);
            if (oh == INVALID_PROCESSTRACE_HANDLE)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                int status = ProcessTrace(ref oh, 1, IntPtr.Zero, IntPtr.Zero);
                if (status != 0)
                    throw new Win32Exception(status);
            }
            finally
            {
                CloseTrace(oh);
            }
        }

        private IntPtr BuildProperties(bool stopping, out int size)
        {
            IntPtr properties;
            EVENT_TRACE_PROPERTIES prop = new EVENT_TRACE_PROPERTIES();
            if (stopping)
            {
                size = Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES)) + (1024 + 1) * 2;
                properties = Marshal.AllocCoTaskMem(size);
                RtlZeroMemory(properties, (IntPtr)size);
                prop.Wnode.Guid = ProviderGuid;
                prop.Wnode.BufferSize = size;
            }
            else
            {
                size = Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES)) + (SessionName.Length + 1) * 2;
                properties = Marshal.AllocCoTaskMem(size);
                RtlZeroMemory(properties, (IntPtr)size);
                prop.Wnode.Guid = ProviderGuid;
                prop.Wnode.Flags = WNODE_FLAG_TRACED_GUID;
                prop.Wnode.BufferSize = size;
                prop.LogFileMode = EVENT_TRACE_REAL_TIME_MODE;
            }

            prop.LoggerNameOffset = Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES));
            Marshal.StructureToPtr(prop, properties, false);
            return properties;
        }

        public Guid ProviderGuid { get; private set; }
        public string SessionName { get; private set; }
        public string Description { get; set; }
        public bool ConsoleOutput { get; set; }

        private void OnRealtimeEvent(int processId, int threadId, string s)
        {
            var handler = RealtimeEvent;
            if (handler != null)
            {
                handler(this, new RealtimeEventArgs(processId, threadId, s));
            }
        }

        private void OnEvent(ref EVENT_TRACE eventRecord)
        {
            string s = Marshal.PtrToStringUni(eventRecord.MofData);
            if (!string.IsNullOrEmpty(s))
            {
                OnRealtimeEvent(eventRecord.Header.ProcessId, eventRecord.Header.ThreadId, s);
            }
        }

        private void StopTrace2()
        {
            int count;
            IntPtr[] a = new IntPtr[64];
            for (int i = 0; i < a.Length; i++)
            {
                int size;
                a[i] = BuildProperties(true, out size);
            }

            int hr = QueryAllTraces(a, a.Length, out count);
            if (hr == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    EVENT_TRACE_PROPERTIES propi = (EVENT_TRACE_PROPERTIES)Marshal.PtrToStructure(a[i], typeof(EVENT_TRACE_PROPERTIES));
                    if (propi.Wnode.Guid == ProviderGuid)
                    {
                        StopTrace(propi.Wnode.HistoricalContext, null, a[i]);
                    }
                }
            }

            for (int i = 0; i < a.Length; i++)
            {
                Marshal.FreeCoTaskMem(a[i]);
            }
        }

        private void StopTrace()
        {
            int size;
            IntPtr props = BuildProperties(true, out size);
            try
            {
                if (StopTrace(0, SessionName, props) != 0)
                {
                    StopTrace2();
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(props);
            }
            _handle = 0;
        }

        public void Dispose()
        {
            if (_traceOn)
            {
                EnableTraceEx(ProviderGuid, IntPtr.Zero, _handle, 0, (byte)EtwTraceLevel.Verbose, 0, 0, 0, IntPtr.Zero);
                _traceOn = false;
            }

            StopTrace();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WNODE_HEADER // 48
        {
            public int BufferSize;
            public uint ProviderId;
            public ulong HistoricalContext;
            public long TimeStamp;
            public Guid Guid;
            public uint ClientContext;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct EVENT_TRACE_PROPERTIES
        {
            public WNODE_HEADER Wnode;
            public uint BufferSize;
            public uint MinimumBuffers;
            public uint MaximumBuffers;
            public uint MaximumFileSize;
            public uint LogFileMode;
            public uint FlushTimer;
            public uint EnableFlags;
            public int AgeLimit;
            public uint NumberOfBuffers;
            public uint FreeBuffers;
            public uint EventsLost;
            public uint BuffersWritten;
            public uint LogBuffersLost;
            public uint RealTimeBuffersLost;
            public IntPtr LoggerThreadId;
            public uint LogFileNameOffset;
            public int LoggerNameOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TIME_ZONE_INFORMATION
        {
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;
            public SYSTEMTIME StandardDate;
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;
            public SYSTEMTIME DaylightDate;
            public int DaylightBias;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TRACE_LOGFILE_HEADER
        {
            public uint BufferSize;
            public uint Version;
            public uint ProviderVersion;
            public uint NumberOfProcessors;
            public ulong EndTime;
            public uint TimerResolution;
            public uint MaximumFileSize;
            public uint LogFileMode;
            public uint BuffersWritten;
            public Guid LogInstanceGuid;
            public string LoggerName;
            public string LogFileName;
            public TIME_ZONE_INFORMATION TimeZone;
            public ulong BootTime;
            public ulong PerfFreq;
            public ulong StartTime;
            public uint ReservedFlags;
            public uint BuffersLost;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ETW_BUFFER_CONTEXT
        {
            public byte ProcessorNumber;
            public byte Alignment;
            public ushort LoggerId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EVENT_TRACE
        {
            public EVENT_TRACE_HEADER Header;
            public uint InstanceId;
            public uint ParentInstanceId;
            public Guid ParentGuid;
            public IntPtr MofData;
            public uint MofLength;
            public ETW_BUFFER_CONTEXT BufferContext;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EVENT_TRACE_HEADER
        {
            public ushort Size;
            public ushort FieldTypeFlags;
            public uint Version;
            public int ThreadId;
            public int ProcessId;
            public ulong TimeStamp;
            public Guid Guid;
            public ulong ProcessorTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct EVENT_TRACE_LOGFILE
        {
            [MarshalAs(UnmanagedType.LPWStr)] 
            public string LogFileName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string LoggerName;
            public long CurrentTime;
            public uint BuffersRead;
            public uint ProcessTraceMode;
            public EVENT_TRACE CurrentEvent;
            public TRACE_LOGFILE_HEADER LogfileHeader;
            public BufferCallback BufferCallback;
            public uint BufferSize;
            public uint Filled;
            public uint EventsLost;
            public EventCallback EventCallback;
            public uint IsKernelTrace;
            public IntPtr Context;
        }

        private const long INVALID_PROCESSTRACE_HANDLE = -1;
        private const uint PROCESS_TRACE_MODE_REAL_TIME = 0x00000100;
        private const uint WNODE_FLAG_TRACED_GUID = 0x00020000;
        private const uint EVENT_TRACE_REAL_TIME_MODE = 0x00000100;
        private const int ERROR_ALREADY_EXISTS = 183;

        private delegate uint BufferCallback(ref EVENT_TRACE_LOGFILE buffer);
        private delegate void EventCallback(ref EVENT_TRACE eventRecord);

        [DllImport("kernel32.dll")]
        private static extern void RtlZeroMemory(IntPtr destination, IntPtr length);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int StartTrace(out ulong sessionHandle, string sessionName, IntPtr properties);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int StopTrace(ulong sessionHandle, string sessionName, IntPtr properties);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern long OpenTrace(ref EVENT_TRACE_LOGFILE logFile);

        [DllImport("advapi32.dll")]
        private static extern int CloseTrace(long traceHandle);

        [DllImport("advapi32.dll")]
        private static extern int ProcessTrace(ref long traceHandle, int handleCount, IntPtr startTime, IntPtr endTime);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int QueryAllTraces(IntPtr[] propertyArray, int propertyArrayCount, out int sessionCount);

        [DllImport("Kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [DllImport("advapi32.dll")]
        private static extern int EnableTraceEx(
            [MarshalAs(UnmanagedType.LPStruct)] Guid providerId,
            IntPtr sourceId,
            ulong tracehandle,
            uint IsEnabled,
            byte level,
            long matchAnyKeyword,
            long matchAllKeyword,
            uint enableProperty,
            IntPtr enableFilterDesc
            );
    }
}
