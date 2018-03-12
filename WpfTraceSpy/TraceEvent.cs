using System;
using System.Diagnostics;
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
            Ticks = Stopwatch.GetTimestamp();
        }

        public long Index { get; }
        public long Ticks { get; set; }
        public long PreviousTicks { get; set; }
        public string ProcessName { get; set; }
        public string Text { get; set; }

        public int Height { get; set; }
        public Brush Background { get; set; }
        public string FullText => Index + "\t" + Ticks + "\t" + ProcessName + "\t" + Text;

        public string TicksText
        {
            get
            {
                string text;
                const string decFormat = "0.00000000";
                switch (App.Current.Settings.ShowTicksMode)
                {
                    case ShowTicksMode.AsTime:
                        text = new TimeSpan(Ticks).ToString();
                        break;

                    case ShowTicksMode.AsSeconds:
                        text = (Ticks / (double)Stopwatch.Frequency).ToString() + " s";
                        break;

                    case ShowTicksMode.AsMilliseconds:
                        text = (Ticks / (double)Stopwatch.Frequency / 1000).ToString() + " ms";
                        break;

                    case ShowTicksMode.AsDeltaTicks:
                        text = (Ticks - PreviousTicks).ToString();
                        break;

                    case ShowTicksMode.AsDeltaSeconds:
                        text = ((Ticks - PreviousTicks) / (double)Stopwatch.Frequency).ToString(decFormat) + " s";
                        break;

                    case ShowTicksMode.AsDeltaMilliseconds:
                        text = ((1000 * (Ticks - PreviousTicks)) / (double)Stopwatch.Frequency).ToString(decFormat) + " ms";
                        break;

                    case ShowTicksMode.AsTicks:
                    default:
                        text = Ticks.ToString();
                        break;

                }
                return text;
            }
        }
    }
}
