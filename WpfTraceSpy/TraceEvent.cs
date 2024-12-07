using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace TraceSpy
{
    public class TraceEvent
    {
        private static long _index;
        private Lazy<IReadOnlyList<ColorRange>> _ranges;
        private string _text;

        public TraceEvent()
        {
            Index = Interlocked.Increment(ref _index);
            Ticks = Stopwatch.GetTimestamp();
            _ranges = new Lazy<IReadOnlyList<ColorRange>>(GetRanges, true);
        }

        public long Index { get; }
        public long Ticks { get; }
        public long PreviousTicks { get; set; }
        public string ProcessName { get; set; }
        public bool IsSelected { get; set; }
        public Brush BackgroundBrush { get; set; }
        public bool DontColorize { get; set; }
        public string FullText => Index + "\t" + Ticks + "\t" + ProcessName + "\t" + Text;
        public IReadOnlyList<ColorRange> Ranges => _ranges.Value;

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;
                _ranges = new Lazy<IReadOnlyList<ColorRange>>(GetRanges, true);

                if (App.Current.Settings.DontSplitText)
                {
                    Texts = new[] { _text };
                }
                else
                {
                    Texts = value != null ? _text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) : null;
                }
            }
        }

        public string[] Texts { get; private set; }

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

        public static void ResetIndex() => Interlocked.Exchange(ref _index, 0);

        private IReadOnlyList<ColorRange> GetRanges() => App.Current.Settings.ComputeColorRanges(this);
    }
}
