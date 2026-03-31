using System;
using System.Collections.Generic;
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
            Ticks = DateTime.Now.Ticks;
            _ranges = new Lazy<IReadOnlyList<ColorRange>>(GetRanges, true);
        }

        public long Index { get; }
        public long Ticks { get; }
        public long PreviousTicks { get; set; }
        public string ProcessName { get; set; }
        public bool IsSelected { get; set; }
        public Brush BackgroundBrush { get; set; }
        public bool DontColorize { get; set; }
        public string FullText => Index + "\t" + TicksText + "\t" + ProcessName + "\t" + Text;
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
                    Texts = [_text];
                }
                else
                {
                    Texts = value != null ? _text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries) : null;
                }
            }
        }

        public string[] Texts { get; private set; }

        public string TicksText
        {
            get
            {
                var text = App.Current.Settings.ShowTicksMode switch
                {
                    ShowTicksMode.AsTime => new TimeSpan(Ticks).ToString(@"hh\:mm\:ss"),
                    ShowTicksMode.AsFullTime => new TimeSpan(Ticks).ToString(@"hh\:mm\:ss\.fff"),
                    ShowTicksMode.AsDateTime => new DateTime(Ticks).ToString(@"yyyy-mm-dd hh\:mm\:ss"),
                    ShowTicksMode.AsFullDateTime => new DateTime(Ticks).ToString(@"yyyy-mm-dd hh\:mm\:ss\.fff"),
                    ShowTicksMode.AsSeconds => (Ticks / (double)TimeSpan.TicksPerSecond).ToString() + " s",
                    ShowTicksMode.AsMilliseconds => (Ticks / (double)TimeSpan.TicksPerMillisecond).ToString() + " ms",
                    ShowTicksMode.AsDeltaTicks => (Ticks - PreviousTicks).ToString(),
                    ShowTicksMode.AsDeltaSeconds => ((Ticks - PreviousTicks) / (double)TimeSpan.TicksPerSecond).ToString() + " s",
                    ShowTicksMode.AsDeltaMilliseconds => ((Ticks - PreviousTicks) / (double)TimeSpan.TicksPerMillisecond).ToString() + " ms",
                    _ => Ticks.ToString(),
                };
                return text;
            }
        }

        public static void ResetIndex() => Interlocked.Exchange(ref _index, 0);

        private IReadOnlyList<ColorRange> GetRanges() => App.Current.Settings.ComputeColorRanges(this);
    }
}
