using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class WpfSettings : Serializable<WpfSettings>
    {
        public const int WHERE_NOONE_CAN_SEE_ME = -32000; // from \windows\core\ntuser\kernel\userk.h

        private string _fontName;
        private string _odsEncodingName;
        private Lazy<Typeface> _typeFace;
        private Lazy<Encoding> _odsEncoding;
        private Lazy<byte[]> _odsEncodingTerminator;
        private string _alternateColor;
        private Lazy<Brush> _alternateBrush;
        private List<string> _searches = new List<string>();
        private List<Filter> _filters = new List<Filter>();
        private List<Colorizer> _colorizers = new List<Colorizer>();
        private List<ColorSet> _colorSets = new List<ColorSet>();
        private List<EtwProvider> _etwProviders = new List<EtwProvider>();
        private double _left;
        private double _findLeft;
        private double _top;
        private double _findTop;

        public WpfSettings()
        {
            _alternateBrush = new Lazy<Brush>(CreateAlternateBrush);
            _typeFace = new Lazy<Typeface>(CreateTypeFace);
            RemoveEmptyLines = true;
            AutoScroll = true;
            ResolveProcessName = true;
            Left = 50;
            Top = 50;
            FindLeft = 50;
            FindTop = 100;
            Width = 800;
            Height = 600;
            FontName = "Lucida Console";
            FontSize = 10;
            IndexColumnWidth = 75;
            TicksColumnWidth = 96;
            ProcessColumnWidth = 102;
            TextColumnWidth = 681;
            Opacity = 1;
            _filters.Add(new Filter(null, false));
            ResetOdsEncoding();
        }

        public bool ResolveProcessName { get; set; }
        public bool ShowProcessId { get; set; }
        public bool ShowTooltips { get; set; }
        public bool ShowEtwDescription { get; set; }
        public bool AutoScroll { get; set; }
        public bool RemoveEmptyLines { get; set; }
        public bool WrapText { get; set; }
        public bool CaptureEtwTraces { get; set; }
        public bool CaptureOdsTraces { get; set; }
        public ShowTicksMode ShowTicksMode { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double FontSize { get; set; }
        public double IndexColumnWidth { get; set; }
        public double TicksColumnWidth { get; set; }
        public double ProcessColumnWidth { get; set; }
        public double TextColumnWidth { get; set; }
        public bool DontAnimateCaptureMenuItem { get; set; }
        public bool DontSplitText { get; set; }
        public bool IsTopmost { get; set; }
        public bool EnableTransparency { get; set; }
        public double Opacity { get; set; }
        public bool DontAllowClearingText { get; set; }
        public string TestTraceText { get; set; }

        public double Left
        {
            get => _left;
            set
            {
                if (value == WHERE_NOONE_CAN_SEE_ME)
                    return;

                _left = value;
            }
        }

        public double Top
        {
            get => _top;
            set
            {
                if (value == WHERE_NOONE_CAN_SEE_ME)
                    return;

                _top = value;
            }
        }

        public double FindLeft
        {
            get => _findLeft;
            set
            {
                if (value == WHERE_NOONE_CAN_SEE_ME)
                    return;

                _findLeft = value;
            }
        }

        public double FindTop
        {
            get => _findTop;
            set
            {
                if (value == WHERE_NOONE_CAN_SEE_ME)
                    return;

                _findTop = value;
            }
        }

        public string OdsEncodingName
        {
            get => _odsEncodingName;
            set
            {
                if (_odsEncodingName == value)
                    return;

                ResetOdsEncoding();
                _odsEncodingName = value;
            }
        }

        public byte[] OdsEncodingTerminator => _odsEncodingTerminator.Value;
        public Encoding OdsEncoding => _odsEncoding.Value;

        private byte[] GetOdsEncodingTerminator() => OdsEncoding.GetBytes("\0");
        private void ResetOdsEncoding()
        {
            _odsEncoding = new Lazy<Encoding>(GetOdsEncoding);
            _odsEncodingTerminator = new Lazy<byte[]>(GetOdsEncodingTerminator);
        }

        private Encoding GetOdsEncoding()
        {
            if (OdsEncodingName != null)
            {
                try
                {
                    if (int.TryParse(OdsEncodingName, out var cp))
                        return Encoding.GetEncoding(cp);

                    return Encoding.GetEncoding(OdsEncodingName);
                }
                catch
                {
                    // can't find continue
                }
            }
            return Encoding.Default;
        }

        [XmlIgnore] // we don't persist this one
        public bool DisableAllFilters { get; set; }

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
                var color = (Color)new ColorConverter().ConvertFromInvariantString(AlternateColor);
                return new SolidColorBrush(color);
            }
            catch
            {
                return null;
            }
        }

        public string[] Searches
        {
            get => _searches.ToArray();
            set => _searches = value == null ? new List<string>() : new List<string>(value);
        }

        public void ClearSearches() => _searches = new List<string>();
        public void AddSearch(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return;

            _ = _searches.RemoveAll(s => s == search);
            _searches.Insert(0, search);
        }

        public Filter[] Filters
        {
            get => _filters.ToArray();
            set => _filters = value == null ? new List<Filter>() : new List<Filter>(value);
        }

        public bool AddFilter(Filter old, Filter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var existing = _filters.FirstOrDefault(p => p.Equals(old));
            if (existing != null)
            {
                filter.CopyTo(existing);
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

        public IReadOnlyList<ColorRange> ComputeColorRanges(TraceEvent evt)
        {
#if FX4
            if (string.IsNullOrWhiteSpace(evt.Text))
                return new ColorRange[0];
#else
            if (string.IsNullOrWhiteSpace(evt.Text))
                return Array.Empty<ColorRange>();
#endif

            var list = new List<ColorRange>();
            ColorRange.ComputeColorizersColorRanges(list, this, evt);
            ColorRange.FinishRanges(list, evt);
            return list;
        }

        public ColorSet[] ColorSets
        {
            get => _colorSets.ToArray();
            set => _colorSets = value == null ? new List<ColorSet>() : new List<ColorSet>(value);
        }

        public bool AddColorSet(ColorSet colorSet)
        {
            if (colorSet == null)
                throw new ArgumentNullException(nameof(colorSet));

            var existing = _colorSets.FirstOrDefault(p => p.Equals(colorSet));
            if (existing != null)
            {
                colorSet.CopyTo(existing);
                return false;
            }

            _colorSets.Add(colorSet);
            return true;
        }

        public bool RemoveColorSet(ColorSet colorSet)
        {
            if (colorSet == null)
                throw new ArgumentNullException(nameof(colorSet));

            return _colorSets.Remove(colorSet);
        }

        public Colorizer[] Colorizers
        {
            get => _colorizers.ToArray();
            set => _colorizers = value == null ? new List<Colorizer>() : new List<Colorizer>(value);
        }

        public bool AddColorizer(Colorizer colorizer)
        {
            if (colorizer == null)
                throw new ArgumentNullException(nameof(colorizer));

            var existing = _colorizers.FirstOrDefault(p => p.Equals(colorizer));
            if (existing != null)
            {
                colorizer.CopyTo(existing);
                return false;
            }

            _colorizers.Add(colorizer);
            return true;
        }

        public bool RemoveColorizer(Colorizer colorizer)
        {
            if (colorizer == null)
                throw new ArgumentNullException(nameof(colorizer));

            return _colorizers.Remove(colorizer);
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
                provider.CopyTo(existing);
                return false;
            }

            _etwProviders.Add(provider);
            return true;
        }

        public bool RemoveEtwProvider(Guid providerId)
        {
            var existing = _etwProviders.FirstOrDefault(p => p.Guid == providerId);
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

        public bool ExcludeLine(string line, string processName)
        {
            if (RemoveEmptyLines && string.IsNullOrWhiteSpace(line))
                return true;

            if (App.Current.Settings.DisableAllFilters)
                return true;

            foreach (var filter in _filters.Where(f => f.IsActive))
            {
                if (filter.ExcludeLine(line, processName))
                    return true;
            }
            return false;
        }
    }
}
