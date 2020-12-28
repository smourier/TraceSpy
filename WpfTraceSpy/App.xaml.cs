using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace TraceSpy
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
        private static readonly Lazy<string> _processName = new Lazy<string>(() => Process.GetCurrentProcess().ProcessName);

        public App()
        {
            ColumnLayout = new TraceEventColumnLayout();
            Settings = WpfSettings.DeserializeFromConfiguration();
        }

        public WpfSettings Settings { get; }
        public TraceEventColumnLayout ColumnLayout { get; }

        public static double PixelsPerDip => ((Current?.MainWindow as MainWindow)?.PixelsPerDip).GetValueOrDefault(1);

        public static TraceEvent AddTrace(TraceLevel level, string text)
        {
            var evt = new TraceEvent();
            evt.ProcessName = _processName.Value;
            evt.Text = text;
            switch (level)
            {
                case TraceLevel.Error:
                    evt.BackgroundBrush = Brushes.DarkOrange;
                    break;

                case TraceLevel.Warning:
                    evt.BackgroundBrush = Brushes.Yellow;
                    break;
            }

            AddTrace(evt);
            return evt;
        }

        private static void AddTrace(TraceEvent evt)
        {
            if (evt == null)
                return;

            if (!(Current?.MainWindow is MainWindow window))
                return;

            window.AddTrace(evt);
        }
    }
}
