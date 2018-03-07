using System;
using System.Threading;
using System.Windows.Media;

namespace TraceSpy
{
    public class TraceEvent
    {
        private static long _index;

        public TraceEvent()
        {
            Index = Interlocked.Increment(ref _index);
            Ticks = DateTime.Now.Ticks;
        }

        public long Index { get; }
        public long Ticks { get; set; }
        public string ProcessName { get; set; }
        public string Text { get; set; }

        public int Height { get; set; }
        public Brush Background { get; set; }
    }
}
