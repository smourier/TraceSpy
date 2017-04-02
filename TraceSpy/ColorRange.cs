using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public class ColorRange : IEquatable<ColorRange>, IComparable, IComparable<ColorRange>
    {
        internal ColorRange(ColorSet colorset, int startIndex, int length)
        {
            ColorSet = colorset;
            StartIndex = startIndex;
            Length = length;
        }

        public int StartIndex { get; private set; }
        public int Length { get; private set; }
        public ColorSet ColorSet { get; private set; }

        internal static void ComputeQuickColorizersColorRanges(List<ColorRange> ranges, Settings settings, string line)
        {
            QuickColorizer[] colorizers = settings.QuickColorizers;
            if (colorizers == null || colorizers.Length == 0)
                return;

            Main.Log("QCCR colorizers:" + colorizers.Length);
            foreach (QuickColorizer colorizer in colorizers)
            {
                if (!colorizer.Active || string.IsNullOrEmpty(colorizer.Text))
                    continue;

                StringComparison sc = colorizer.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                int index = line.IndexOf(colorizer.Text, sc);
                Main.Log("QCCR test qc:" + colorizer + " sc:" + sc + " index:" + index);
                if (index < 0)
                    continue;

                if (colorizer.WholeText)
                {
                    // start
                    if (index > 0)
                    {
                        Main.Log("QCCR IsWholeTextCompatible('" + line[index - 1] + "'):" + IsWholeTextCompatible(line[index - 1]));
                        if (!IsWholeTextCompatible(line[index - 1]))
                            continue;
                    }

                    // end
                    if ((index + colorizer.Text.Length)< (line.Length - 1))
                    {
                        Main.Log("QCCR IsWholeTextCompatible('" + line[index + colorizer.Text.Length] + "'):" + IsWholeTextCompatible(line[index + colorizer.Text.Length]));
                        if (!IsWholeTextCompatible(line[index + colorizer.Text.Length]))
                            continue;
                    }
                }

                Main.Log("QCCR match '" + line +  "'");
                ColorRange range = CreateColorRange(colorizer.ColorSet, index, colorizer.Text.Length, line);
                ranges.Add(range);
            }
            ranges.Sort();
        }

        private static bool IsWholeTextCompatible(char c)
        {
            if (char.IsControl(c))
                return true;

            if (char.IsPunctuation(c))
                return true;

            if (char.IsSeparator(c))
                return true;

            return false;
        }

        internal static void ComputeColorizersColorRanges(List<ColorRange> ranges, Settings settings, string line)
        {
            Colorizer[] colorizers = settings.Colorizers;
            if (colorizers == null || colorizers.Length == 0 || settings.ColorSets == null || settings.ColorSets.Length == 0)
                return;

            // get named groups that match one of the defined color set
            //Main.Log("CCCR");
            //Main.Log("CCCR Compute line(" + line.Length + "):'" + line + "'");
            foreach (Colorizer colorizer in colorizers)
            {
                if (!colorizer.Active)
                    continue;

                //Main.Log("CCCR Match colorizer:" + colorizer.Definition);
                foreach (Match m in colorizer.Regex.Matches(line))
                {
                    for (int i = 0; i < m.Groups.Count; i++)
                    {
                        string name = colorizer.Regex.GroupNameFromNumber(i);
                        if (string.IsNullOrEmpty(name))
                            continue;

                        Group group = m.Groups[i];
                        if (!group.Success)
                            continue;

                        ColorSet set = settings.GetColorSet(name);
                        if (set == null)
                            continue;

                        //Main.Log("CCCR  Match group name:" + name + " value=" + group.Value);
                        ColorRange range = CreateColorRange(set, group.Index, group.Length, line);
                        ranges.Add(range);
                    }
                }
            }

            ranges.Sort();
        }

        private static ColorRange CreateColorRange(ColorSet colorset, int startIndex, int length, string line)
        {
            ColorRange range = new ColorRange(colorset, startIndex, length);
#if DEBUG
            System.Diagnostics.Debug.Assert(startIndex >= 0);
            System.Diagnostics.Debug.Assert(length > 0);
            System.Diagnostics.Debug.Assert((startIndex + length) <= line.Length);
#endif
            return range;
        }

        internal static void FinishRanges(List<ColorRange> ranges, string line)
        {
            //Dump(ranges, "Before", line);

            // create intermediate null range
            // note: overlapped match will create undetermined results...
            int lastCovered = 0;
            List<ColorRange> newRanges = new List<ColorRange>();
            foreach (ColorRange range in ranges)
            {
                if (range.StartIndex > lastCovered)
                {
                    //Main.Log("CCCR Add range start(last):" + lastCovered + " len:" + (range.StartIndex - lastCovered) + " '" + line.Substring(lastCovered, range.StartIndex - lastCovered) + "'");
                    newRanges.Add(CreateColorRange(null, lastCovered, range.StartIndex - lastCovered, line));
                }

                lastCovered = range.StartIndex + range.Length;
                //Main.Log("CCCR  last:" + lastCovered);
            }
            if (lastCovered < line.Length)
            {
                // create intermediate null last range
                newRanges.Add(CreateColorRange(null, lastCovered, line.Length - lastCovered, line));
            }

            ranges.AddRange(newRanges);

            // sort again (note: could be removed if we change the algorithm and insert intermediate ranges at the same time... not sure it's really useful)
            ranges.Sort();
            //Dump(ranges, "CCCR After", line);
        }

        private static void Dump(List<ColorRange> ranges, string text, string line)
        {
            Main.Log(text + "CCCR Ranges:" + ranges.Count);
            foreach (ColorRange range in ranges)
            {
                Main.Log("CCCR  range:" + range + " colorset:" + range.ColorSet + " startIndex:" + range.StartIndex + " length:" + range.Length);
                Main.Log("CCCR   text:'" + line.Substring(range.StartIndex, range.Length) + "'");
            }
        }

        public override string ToString()
        {
            return StartIndex + ":" + Length + ":" + ColorSet;
        }

        public override int GetHashCode()
        {
            return Length.GetHashCode() ^ StartIndex.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ColorRange);
        }

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

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as ColorRange);
        }

        public int CompareTo(ColorRange other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return StartIndex.CompareTo(other.StartIndex);
        }
    }
}
