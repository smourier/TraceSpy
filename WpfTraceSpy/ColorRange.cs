using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public class ColorRange : IComparable, IComparable<ColorRange>, IEquatable<ColorRange>
    {
        internal ColorRange(ColorSet colorset, int startIndex, int length, int textsIndex)
        {
            TextsIndex = textsIndex;
            ColorSet = colorset;
            StartIndex = startIndex;
            Length = length;
        }

        public int TextsIndex { get; }
        public int StartIndex { get; }
        public int Length { get; }
        public ColorSet ColorSet { get; }
#if DEBUG
        public string Text { get; set; }
#endif

        int IComparable.CompareTo(object obj) => CompareTo(obj as ColorRange);
        public int CompareTo(ColorRange other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var cmp = TextsIndex.CompareTo(other.TextsIndex);
            if (cmp == 0)
                return StartIndex.CompareTo(other.StartIndex);

            return cmp;
        }

        internal static void ComputeColorizersColorRanges(List<ColorRange> ranges, WpfSettings settings, TraceEvent evt)
        {
            var colorizers = settings.Colorizers;
            var colorSets = settings.ColorSets;
            if (colorizers == null || colorizers.Length == 0 || colorSets == null || colorSets.Length == 0 || string.IsNullOrWhiteSpace(evt.Text) || evt.Text.Length <= 1)
                return;

            for (var k = 0; k < evt.Texts.Length; k++)
            {
                // get named groups that match one of the defined color set
                foreach (var colorizer in colorizers)
                {
                    if (!colorizer.IsActive)
                        continue;

                    if (colorizer.Regex == null)
                        continue;

                    foreach (Match m in colorizer.Regex.Matches(evt.Texts[k]))
                    {
                        for (var i = 0; i < m.Groups.Count; i++)
                        {
                            var name = colorizer.Regex.GroupNameFromNumber(i);
                            if (string.IsNullOrEmpty(name))
                                continue;

                            var group = m.Groups[i];
                            if (!group.Success)
                                continue;

                            var set = settings.GetColorSet(name);
                            if (set == null)
                                continue;

                            var range = CreateColorRange(set, group.Index, group.Length, evt.Texts[k], k);
                            ranges.Add(range);
                        }
                    }
                }
            }
            ranges.Sort();
        }

        private static ColorRange CreateColorRange(ColorSet colorset, int startIndex, int length, string line, int lineIndex)
        {
            var range = new ColorRange(colorset, startIndex, length, lineIndex);
#if DEBUG
            System.Diagnostics.Debug.Assert(startIndex >= 0);
            System.Diagnostics.Debug.Assert(length >= 0);
            System.Diagnostics.Debug.Assert((startIndex + length) <= line.Length);
            range.Text = line.Substring(startIndex, length);
#endif
            return range;
        }

        internal static void FinishRanges(List<ColorRange> ranges, TraceEvent evt)
        {
            if (ranges.Count == 0 || evt.Texts == null || evt.Texts.Length == 0)
                return;

            // create intermediate null range
            // note: overlapped match will create undetermined results...
            var newRanges = new List<ColorRange>();
            for (var i = 0; i < evt.Texts.Length; i++)
            {
                var text = evt.Texts[i];
                var lastCovered = 0;
                foreach (var range in ranges.Where(r => r.TextsIndex == i))
                {
                    if (range.StartIndex > lastCovered)
                    {
                        newRanges.Add(CreateColorRange(null, lastCovered, range.StartIndex - lastCovered, text, i));
                    }

                    lastCovered = range.StartIndex + range.Length;
                }

                if (lastCovered < text.Length)
                {
                    // create intermediate null last range
                    newRanges.Add(CreateColorRange(null, lastCovered, text.Length - lastCovered, text, i));
                }
            }

            ranges.AddRange(newRanges);

            // sort again (note: could be removed if we change the algorithm and insert intermediate ranges at the same time... not sure it's really useful)
            ranges.Sort();
        }

#if DEBUG
        public override string ToString() => TextsIndex + ":" + StartIndex + ":" + Length + ":" + ColorSet + " '" + Text + "'";
#else
        public override string ToString() => TextsIndex + ":" + StartIndex + ":" + Length + ":" + ColorSet;
#endif
        public override int GetHashCode() => Length.GetHashCode() ^ TextsIndex.GetHashCode() ^ StartIndex.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as ColorRange);
        public bool Equals(ColorRange other)
        {
            if (other == null)
                return false;

            if (Length != other.Length)
                return false;

            if (TextsIndex != other.TextsIndex)
                return false;

            if (StartIndex != other.StartIndex)
                return false;

            if (ColorSet == null)
                return other.ColorSet == null;

            return ColorSet.Equals(other.ColorSet);
        }
    }
}
