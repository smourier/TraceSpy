using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public class ColorRange : IComparable, IComparable<ColorRange>, IEquatable<ColorRange>
    {
        internal ColorRange(ColorSet colorset, int startIndex, int length)
        {
            ColorSet = colorset;
            StartIndex = startIndex;
            Length = length;
        }

        public int StartIndex { get; }
        public int Length { get; }
        public ColorSet ColorSet { get; }

        int IComparable.CompareTo(object obj) => CompareTo(obj as ColorRange);
        public int CompareTo(ColorRange other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return StartIndex.CompareTo(other.StartIndex);
        }

        internal static void ComputeColorizersColorRanges(List<ColorRange> ranges, WpfSettings settings, string line)
        {
            var colorizers = settings.Colorizers;
            var colorSets = settings.ColorSets;
            if (colorizers == null || colorizers.Length == 0 || colorSets == null || colorSets.Length == 0)
                return;

            // get named groups that match one of the defined color set
            foreach (var colorizer in colorizers)
            {
                if (!colorizer.IsActive)
                    continue;

                foreach (Match m in colorizer.Regex.Matches(line))
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

                        var range = CreateColorRange(set, group.Index, group.Length, line);
                        ranges.Add(range);
                    }
                }
            }

            ranges.Sort();
        }

        private static ColorRange CreateColorRange(ColorSet colorset, int startIndex, int length, string line)
        {
            var range = new ColorRange(colorset, startIndex, length);
#if DEBUG
            System.Diagnostics.Debug.Assert(startIndex >= 0);
            System.Diagnostics.Debug.Assert(length > 0);
            System.Diagnostics.Debug.Assert((startIndex + length) <= line.Length);
#endif
            return range;
        }

        internal static void FinishRanges(List<ColorRange> ranges, string line)
        {
            // create intermediate null range
            // note: overlapped match will create undetermined results...
            var lastCovered = 0;
            var newRanges = new List<ColorRange>();
            foreach (var range in ranges)
            {
                if (range.StartIndex > lastCovered)
                {
                    newRanges.Add(CreateColorRange(null, lastCovered, range.StartIndex - lastCovered, line));
                }

                lastCovered = range.StartIndex + range.Length;
            }

            if (lastCovered < line.Length)
            {
                // create intermediate null last range
                newRanges.Add(CreateColorRange(null, lastCovered, line.Length - lastCovered, line));
            }

            ranges.AddRange(newRanges);

            // sort again (note: could be removed if we change the algorithm and insert intermediate ranges at the same time... not sure it's really useful)
            ranges.Sort();
        }

        public override string ToString() => StartIndex + ":" + Length + ":" + ColorSet;
        public override int GetHashCode() => Length.GetHashCode() ^ StartIndex.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as ColorRange);
        public bool Equals(ColorRange other)
        {
            if (other == null)
                return false;

            if (Length != other.Length)
                return false;

            if (StartIndex != other.StartIndex)
                return false;

            if (ColorSet == null)
                return other.ColorSet == null;

            return ColorSet.Equals(other.ColorSet);
        }
    }
}
