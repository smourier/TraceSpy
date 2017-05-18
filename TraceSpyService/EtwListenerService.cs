using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using TraceSpyService.Configuration;

namespace TraceSpyService
{
    public class EtwListenerService : IService
    {
        private readonly List<EventRealtimeListener> _listeners = new List<EventRealtimeListener>();
        private readonly static Dictionary<int, string> _processes = new Dictionary<int, string>();
        private readonly Stopwatch _watch;

        public EtwListenerService(int bufferCapacity)
        {
            Buffer = new ConcurrentCircularBuffer<EtwRecord>(bufferCapacity);
            _watch = new Stopwatch();
            SessionId = Guid.NewGuid();
        }

        public ConcurrentCircularBuffer<EtwRecord> Buffer { get; private set; }

        public void Start()
        {
            Host.Log(this, "Circular Buffer Capacity: " + Buffer.Capacity + " records.");
            Host.Log(this, "Session Id: " + SessionId);
            Status = ServiceControllerStatus.StartPending;
            int count = 0;
            foreach (var element in ServiceSection.Current.EtwListener.Providers.Cast<EtwProviderElement>().Where(e => e.Enabled))
            {
                var listener = new EventRealtimeListener(element.Guid, element.Guid.ToString(), element.TraceLevel);
                if (!string.IsNullOrWhiteSpace(element.Description))
                {
                    listener.Description = element.Description;
                }

                listener.ConsoleOutput = element.ConsoleOutput || ServiceSection.Current.EtwListener.ConsoleOutput;

                var t = new Thread(ProcessEtwTrace);
                t.Start(listener);

                listener.RealtimeEvent += OnEtwListenerRealtimeEvent;

                Host.Log(this, "Added listener for provider id: " + element.Guid + ", Trace level: " + element.TraceLevel);
                _listeners.Add(listener);
                count++;
            }

            if (count == 0)
            {
                Host.Log(this, "No provider enabled. Aborting.");
                Status = ServiceControllerStatus.Stopped;
                return;
            }

            Status = ServiceControllerStatus.Running;
        }

        private static string GetProcessName(int id)
        {
            string name;
            if (!_processes.TryGetValue(id, out name))
            {
                try
                {
                    var process = Process.GetProcessById(id);
                    name = process.ProcessName;
                }
                catch
                {
                }

                if (name == null)
                {
                    name = id.ToString();
                }
                _processes[id] = name;
            }
            return name;
        }

        private void OnEtwListenerRealtimeEvent(object sender, RealtimeEventArgs e)
        {
            if (Status != ServiceControllerStatus.Running)
                return;

            var listener = (EventRealtimeListener)sender;
            var record = new EtwRecord();
            record.ProcessName = e.ProcessId + "/" + GetProcessName(e.ProcessId);
            if (!string.IsNullOrWhiteSpace(listener.Description))
            {
                record.ProcessName += "/" + listener.Description;
            }

            record.Text = e.Message;

            if (!_watch.IsRunning)
            {
                _watch.Start();
                // small hack; we ensure the first record has 0 ticks
                record.Ticks = 0;
            }
            else
            {
                record.Ticks = _watch.ElapsedTicks;
            }

            long index = Buffer.AddAndGetIndex(record);
            record.Index = index;
            
            if (listener.ConsoleOutput)
            {
                Host.Log(this, listener.Description + "|#" + record.Index + "|" + record.Ticks + "|" + record.ProcessName + "|" + record.Text);
            }
        }

        private void ProcessEtwTrace(object state)
        {
            EventRealtimeListener listener = (EventRealtimeListener)state;
            listener.ProcessTraces();
            Host.Log(this, "Stopped listener for provider id: " + listener.ProviderGuid);
        }

        public void Stop()
        {
            Status = ServiceControllerStatus.StopPending;
            foreach (var listener in _listeners)
            {
                listener.Dispose();
            }
            _listeners.Clear();
            
            Status = ServiceControllerStatus.Stopped;
        }

        public void Pause()
        {
            Status = ServiceControllerStatus.Paused;
        }

        public void Continue()
        {
            Status = ServiceControllerStatus.Running;
        }

        public Guid SessionId { get; private set; }
        public ServiceControllerStatus Status { get; private set; }
        public IServiceHost Host { get; set; }
        public string Name { get; set; }
        public Exception StartException { get; set; }
        public Exception StopException { get; set; }
    }
}
