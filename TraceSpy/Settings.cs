using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class Settings : Serializable<Settings>
    {
        private List<Filter> _filters = new List<Filter>();
        private List<EtwProvider> _etwProviders = new List<EtwProvider>();
        private List<Colorizer> _colorizers = new List<Colorizer>();
        private List<QuickColorizer> _quickColorizers = new List<QuickColorizer>();
        private List<ColorSet> _colorSets = new List<ColorSet>();
        private AutoCompleteStringCollection _searches = new AutoCompleteStringCollection();

        public Settings()
        {
            RemoveEmptyLines = true;
            AutoScroll = true;
            ShowProcessName = true;
            CaptureOutputDebugString = true;
            Left = 50;
            Top = 50;
            FindLeft = 50;
            FindTop = 100;
            Width = 800;
            Height = 600;
            FontName = "Lucida Console";
            IndexColumnWidth = 75;
            TicksColumnWidth = 96;
            ProcessColumnWidth = 102;
            TextColumnWidth = 681;
            TabSize = 20;
            _filters.Add(new Filter(FilterType.IncludeAll, null, false));
        }

        public float TabSize { get; set; }
        public bool ShowProcessName { get; set; }
        public bool ShowProcessId { get; set; }
        public bool ShowTooltips { get; set; }
        public bool ShowEtwDescription { get; set; }
        public bool AutoScroll { get; set; }
        public bool CaptureEtwTraces { get; set; }
        public bool CaptureOutputDebugString { get; set; }
        public bool RemoveEmptyLines { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FindLeft { get; set; }
        public int FindTop { get; set; }
        public int IndexColumnWidth { get; set; }
        public int TicksColumnWidth { get; set; }
        public int ProcessColumnWidth { get; set; }
        public int TextColumnWidth { get; set; }
        public bool DontAnimateCaptureMenuItem { get; set; }

        [XmlIgnore]
        public Font Font
        {
            get
            {
                if (string.IsNullOrEmpty(FontName))
                    return null;

                return (Font)(new FontConverter().ConvertFromInvariantString(FontName));
            }
            set
            {
                FontName = value == null ? null : new FontConverter().ConvertToInvariantString(value);
            }
        }

        [XmlElement("Font")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FontName { get; set; }

        public Filter[] Filters
        {
            get
            {
                return _filters.ToArray();
            }
            set
            {
                _filters = value == null ? new List<Filter>() : new List<Filter>(value);
            }
        }

        public EtwProvider[] EtwProviders
        {
            get
            {
                return _etwProviders.ToArray();
            }
            set
            {
                _etwProviders = value == null ? new List<EtwProvider>() : new List<EtwProvider>(value);
            }
        }

        public ColorSet GetColorSet(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            foreach (ColorSet colorSet in _colorSets)
            {
                if (string.Compare(name, colorSet.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    return colorSet;
            }
            return null;
        }

        public ColorSet[] ColorSets
        {
            get
            {
                return _colorSets.ToArray();
            }
            set
            {
                _colorSets = value == null ? new List<ColorSet>() : new List<ColorSet>(value);
            }
        }

        public Colorizer[] Colorizers
        {
            get
            {
                return _colorizers.ToArray();
            }
            set
            {
                _colorizers = value == null ? new List<Colorizer>() : new List<Colorizer>(value);
            }
        }

        public QuickColorizer[] QuickColorizers
        {
            get
            {
                return _quickColorizers.ToArray();
            }
            set
            {
                _quickColorizers = value == null ? new List<QuickColorizer>() : new List<QuickColorizer>(value);
            }
        }

        [XmlIgnore]
        public AutoCompleteStringCollection AutoCompleteSearches
        {
            get
            {
                return _searches;
            }
        }

        public void AddSearch(string search)
        {
            if (string.IsNullOrEmpty(search))
                return;

            foreach (string s in _searches)
            {
                if (string.Compare(s, search, StringComparison.OrdinalIgnoreCase) == 0)
                    return;
            }

            _searches.Add(search);
        }

        public string[] Searches
        {
            get
            {
                string[] searches = new string[_searches.Count];
                _searches.CopyTo(searches, 0);
                return searches;
            }
            set
            {
                _searches = new AutoCompleteStringCollection();
                if (value != null)
                {
                    _searches.AddRange(value);
                }
            }
        }

        public IEnumerable<ColorRange> ComputeColorRanges(string line)
        {
            List<ColorRange> list = new List<ColorRange>();
            if (string.IsNullOrEmpty(line))
            {
                list.Add(new ColorRange(null, 0, 0));
                return list;
            }

            ColorRange.ComputeColorizersColorRanges(list, this, line);
            ColorRange.ComputeQuickColorizersColorRanges(list, this, line);
            ColorRange.FinishRanges(list, line);

            if (list.Count == 0)
            {
                list.Add(new ColorRange(null, 0, line.Length));
            }
            return list;
        }

        public bool IncludeLine(string line, string processName)
        {
            if (line == null)
                return false;

            foreach (Filter filter in _filters)
            {
                if (!filter.Active)
                    continue;

                if (filter.FilterColumn == FilterColumn.Process && processName == null)
                    continue;

                if (filter.FilterType == FilterType.IncludeAll)
                    return true;

                if (filter.FilterType == FilterType.Include &&
                    filter.FilterColumn == FilterColumn.Text &&
                    filter.Regex != null &&
                    filter.Regex.Match(line).Success)
                    return true;

                if (filter.FilterType == FilterType.Include &&
                    filter.FilterColumn == FilterColumn.Process &&
                    filter.Regex != null &&
                    processName != null &&
                    filter.Regex.Match(processName).Success)
                    return true;
            }
            return false;
        }

        public bool ExcludeLine(string line, string processName)
        {
            if (line == null)
                return true;

            foreach (Filter filter in _filters)
            {
                if (!filter.Active)
                    continue;

                if (filter.FilterColumn == FilterColumn.Process && processName == null)
                    continue;

                if (filter.FilterType == FilterType.IncludeAll)
                    continue;

                if (filter.FilterType == FilterType.Exclude &&
                    filter.FilterColumn == FilterColumn.Text &&
                    filter.Regex != null &&
                    filter.Regex.Match(line).Success)
                    return true;

                if (filter.FilterType == FilterType.Exclude &&
                    filter.FilterColumn == FilterColumn.Process &&
                    filter.Regex != null &&
                    processName != null &&
                    filter.Regex.Match(processName).Success)
                    return true;
            }
            return false;
        }
    }
}
