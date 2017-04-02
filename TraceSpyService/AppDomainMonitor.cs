using System;
using System.Threading;
using TraceSpyService.Configuration;

namespace TraceSpyService
{
    public static class AppDomainMonitor
    {
        private static DateTime _LastCollectTime = DateTime.UtcNow;
        private static bool _stopRequested;
        private static Timer _timer;
        private static TimeSpan _totalCpuTime;

        static AppDomainMonitor()
        {
            if (ServiceSection.Current.MonitoringPeriod > 0)
            {
                AppDomain.MonitoringIsEnabled = true;
                _totalCpuTime = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
            }
        }

        public static bool Start()
        {
            if (!AppDomain.MonitoringIsEnabled)
                return false;

            _timer = new Timer((state) => Compute(), null, 0, ServiceSection.Current.MonitoringPeriod);
            return true;
        }

        public static int CpuUsage { get; private set; }
        public static long MemoryUsage { get; private set; }

        private static void Compute()
        {
            if (_stopRequested)
                return;

            MemoryUsage = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize;
            
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan monitoringTotalProcessorTime = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
            CpuUsage = Math.Min(100, Math.Max(0, (int)(((monitoringTotalProcessorTime - _totalCpuTime).TotalMilliseconds * 100.0) / (utcNow - _LastCollectTime).TotalMilliseconds)));
            _totalCpuTime = monitoringTotalProcessorTime;
            _LastCollectTime = utcNow;
        }

        public static void Stop()
        {
            _stopRequested = true;
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}