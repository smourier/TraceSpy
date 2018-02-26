using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class WpfSettings : Serializable<WpfSettings>
    {
        private string _fontName;
        private Lazy<Typeface> _typeFace;
        private string _alternateColor;
        private Lazy<Brush> _alternateBrush;
        private List<Filter> _filters = new List<Filter>();
        private List<EtwProvider> _etwProviders = new List<EtwProvider>();
        private List<Colorizer> _colorizers = new List<Colorizer>();
        private List<QuickColorizer> _quickColorizers = new List<QuickColorizer>();
        private List<ColorSet> _colorSets = new List<ColorSet>();

        public WpfSettings()
        {
            _alternateBrush = new Lazy<Brush>(CreateAlternateBrush);
            _typeFace = new Lazy<Typeface>(CreateTypeFace);
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
            FontSize = 10;
            _filters.Add(new Filter(null, false));
        }

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
        public double FontSize { get; set; }
        public bool DontAnimateCaptureMenuItem { get; set; }

        public string FontName
        {
            get => _fontName;
            set
            {
                if (_fontName == value)
                    return;

                _fontName = value;
                _typeFace = new Lazy<Typeface>(CreateTypeFace);
            }
        }

        [XmlIgnore]
        public Typeface TypeFace => _typeFace.Value;

        private Typeface CreateTypeFace()
        {
            if (!string.IsNullOrWhiteSpace(FontName))
            {
                try
                {
                    return new Typeface(FontName);
                }
                catch
                {
                    // continue
                }
            }
            return new Typeface("Lucida Console");
        }

        public string AlternateColor
        {
            get => _alternateColor;
            set
            {
                if (_alternateColor == value)
                    return;

                _alternateColor = value;
                _alternateBrush = new Lazy<Brush>(CreateAlternateBrush);
            }
        }

        [XmlIgnore]
        public Brush AlternateBrush => _alternateBrush.Value;

        private Brush CreateAlternateBrush()
        {
            if (string.IsNullOrWhiteSpace(AlternateColor))
                return null;

            try
            {
                var color = (Color)(new ColorConverter().ConvertFromInvariantString(AlternateColor));
                return new SolidColorBrush(color);
            }
            catch
            {
                return null;
            }
        }

        public Filter[] Filters
        {
            get => _filters.ToArray();
            set => _filters = value == null ? new List<Filter>() : new List<Filter>(value);
        }

        public bool AddFilter(Filter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var existing = _filters.FirstOrDefault(p => p.Equals(filter));
            if (existing != null)
            {
                existing.IsActive = filter.IsActive;
                existing.FilterColumn = filter.FilterColumn;
                existing.Definition = filter.Definition;
                existing.IgnoreCase = filter.IgnoreCase;
                return false;
            }

            _filters.Add(filter);
            return true;
        }

        public bool RemoveFilter(Filter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return _filters.Remove(filter);
        }

        public EtwProvider[] EtwProviders
        {
            get => _etwProviders.ToArray();
            set => _etwProviders = value == null ? new List<EtwProvider>() : new List<EtwProvider>(value);
        }

        public bool AddEtwProvider(EtwProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            var existing = _etwProviders.FirstOrDefault(p => p.Equals(provider));
            if (existing != null)
            {
                existing.IsActive = provider.IsActive;
                existing.Description = provider.Description;
                existing.TraceLevel = provider.TraceLevel;
                return false;
            }

            _etwProviders.Add(provider);
            return true;
        }

        public bool RemoveEtwProvider(Guid guid)
        {
            var existing = _etwProviders.FirstOrDefault(p => p.Guid == guid);
            if (existing == null)
                return false;

            return _etwProviders.Remove(existing);
        }

        public bool RemoveEtwProvider(EtwProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            return _etwProviders.Remove(provider);
        }

        public ColorSet GetColorSet(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            foreach (var colorSet in _colorSets)
            {
                if (string.Compare(name, colorSet.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    return colorSet;
            }
            return null;
        }

        public ColorSet[] ColorSets
        {
            get => _colorSets.ToArray();
            set => _colorSets = value == null ? new List<ColorSet>() : new List<ColorSet>(value);
        }

        public Colorizer[] Colorizers
        {
            get => _colorizers.ToArray();
            set => _colorizers = value == null ? new List<Colorizer>() : new List<Colorizer>(value);
        }

        public QuickColorizer[] QuickColorizers
        {
            get => _quickColorizers.ToArray();
            set => _quickColorizers = value == null ? new List<QuickColorizer>() : new List<QuickColorizer>(value);
        }

        public IEnumerable<ColorRange> ComputeColorRanges(string line)
        {
            var list = new List<ColorRange>();
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

            foreach (var filter in _filters)
            {
                if (!filter.IsActive)
                    continue;

                if (filter.FilterColumn == FilterColumn.Process && processName == null)
                    continue;

                if (filter.FilterColumn == FilterColumn.Text &&
                    filter.Regex != null &&
                    filter.Regex.Match(line).Success)
                    return true;

                if (filter.FilterColumn == FilterColumn.Process &&
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

            foreach (var filter in _filters)
            {
                if (!filter.IsActive)
                    continue;

                if (filter.FilterColumn == FilterColumn.Process && processName == null)
                    continue;

                if (filter.FilterColumn == FilterColumn.Text &&
                    filter.Regex != null &&
                    filter.Regex.Match(line).Success)
                    return true;

                if (filter.FilterColumn == FilterColumn.Process &&
                    filter.Regex != null &&
                    processName != null &&
                    filter.Regex.Match(processName).Success)
                    return true;
            }
            return false;
        }
    }
}
