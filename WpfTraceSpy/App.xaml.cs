using System;
using System.Diagnostics;
using System.Windows;

namespace TraceSpy
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;

        private static readonly Lazy<int> _processId = new Lazy<int>(() => Process.GetCurrentProcess().Id);
        private static readonly Lazy<string> _processName = new Lazy<string>(() => Process.GetCurrentProcess().ProcessName);

        public App()
        {
            ColumnLayout = new TraceEventColumnLayout();
            Settings = WpfSettings.DeserializeFromConfiguration();
        }

        public WpfSettings Settings { get; }
        public TraceEventColumnLayout ColumnLayout { get; }

        public static TraceEvent AddTrace(string text)
        {
            var evt = new TraceEvent();
            evt.ProcessName = _processName.Value;
            evt.Text = text;
            AddTrace(evt);
            return evt;
        }

        public static void AddTrace(TraceEvent evt)
        {
            if (evt == null)
                return;

            if (!(Current.MainWindow is MainWindow window))
                return;

            window.AddTrace(evt);
        }
    }
}
