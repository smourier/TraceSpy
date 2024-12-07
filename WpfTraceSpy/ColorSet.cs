using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class ColorSet : DictionaryObject, IEquatable<ColorSet>, IComparable, IComparable<ColorSet>
    {
        private Lazy<Brush> _foreBrush;
        private Lazy<Brush> _backBrush;
        private Lazy<Brush> _frameBrush;
        private Lazy<Pen> _backPen;
        private Lazy<Tuple<Typeface, double>> _typeFace;

        public ColorSet()
        {
            _foreBrush = new Lazy<Brush>(GetForeBrush, true);
            _backBrush = new Lazy<Brush>(GetBackBrush, true);
            _frameBrush = new Lazy<Brush>(GetFrameBrush, true);
            _backPen = new Lazy<Pen>(GetBackPen, true);
            _typeFace = new Lazy<Tuple<Typeface, double>>(GetTypeface, true);
        }

        public string Name { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public double FrameWidth { get => DictionaryObjectGetPropertyValue(0d); set => DictionaryObjectSetPropertyValue(value); }

        [XmlElement("ForeBrush")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ForeBrushText { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlElement("BackBrush")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string BackBrushText { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlElement("FrameBrush")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FrameBrushText { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlElement("Font")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FontText { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlIgnore]
        public Brush ForeBrush => _foreBrush.Value;
        private Brush GetForeBrush() => ConvertToBrush(ForeBrushText, App.CurrentTheme.RangeTextColorBrush);

        [XmlIgnore]
        public Brush BackBrush => _backBrush.Value;
        private Brush GetBackBrush() => ConvertToBrush(BackBrushText, App.CurrentTheme.RangeBackColorBrush);

        [XmlIgnore]
        public Brush FrameBrush => _frameBrush.Value;
        private Brush GetFrameBrush() => ConvertToBrush(FrameBrushText, ForeBrush);

        [XmlIgnore]
        public Pen BackPen => _backPen.Value;
        private Pen GetBackPen() => ConvertToPen(FrameBrushText, new Pen(FrameBrush, FrameWidth));

        [XmlIgnore]
        public Tuple<Typeface, double> Typeface => _typeFace.Value;
        private Tuple<Typeface, double> GetTypeface() => ConvertToTypeface(FontText, new Tuple<Typeface, double>(App.Current.Settings.TypeFace, App.Current.Settings.FontSize));

        protected override IEnumerable DictionaryObjectGetErrors(string propertyName)
        {
            if (propertyName == null || propertyName == nameof(Name))
            {
                if (string.IsNullOrWhiteSpace(Name))
                    yield return "Name cannot be empty.";
            }

            if (propertyName == null || propertyName == nameof(ForeBrushText))
            {
                if (!string.IsNullOrWhiteSpace(ForeBrushText))
                {
                    var brush = ConvertToBrush(ForeBrushText, null);
                    if (brush == null)
                        yield return "Fore brush cannot be resolved.";
                }
            }

            if (propertyName == null || propertyName == nameof(BackBrushText))
            {
                if (!string.IsNullOrWhiteSpace(BackBrushText))
                {
                    var brush = ConvertToBrush(BackBrushText, null);
                    if (brush == null)
                        yield return "Back brush cannot be resolved.";
                }
            }

            if (propertyName == null || propertyName == nameof(FontText))
            {
                if (!string.IsNullOrWhiteSpace(FontText))
                {
                    var face = ConvertToTypeface(FontText, null);
                    if (face == null)
                        yield return "Type face cannot be resolved.";
                }
            }
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FrameWidth))
            {
                _backPen = new Lazy<Pen>(GetBackPen, true);
            }
            else if (e.PropertyName == nameof(ForeBrushText))
            {
                _foreBrush = new Lazy<Brush>(GetForeBrush, true);
            }
            else if (e.PropertyName == nameof(BackBrushText))
            {
                _backBrush = new Lazy<Brush>(GetBackBrush, true);
                _backPen = new Lazy<Pen>(GetBackPen, true);
                _frameBrush = new Lazy<Brush>(GetFrameBrush, true);
            }
            else if (e.PropertyName == nameof(FrameBrushText))
            {
                _frameBrush = new Lazy<Brush>(GetFrameBrush, true);
                _backPen = new Lazy<Pen>(GetBackPen, true);
            }
            else if (e.PropertyName == nameof(FontText))
            {
                _typeFace = new Lazy<Tuple<Typeface, double>>(GetTypeface, true);
            }
            base.OnPropertyChanged(sender, e);
        }

        public ColorSet Clone()
        {
            var clone = new ColorSet();
            CopyTo(clone);
            return clone;
        }

        public override string ToString()
        {
            if (Name == null)
            {
                var text = string.Empty;
                if (!string.IsNullOrEmpty(ForeBrushText))
                {
                    text += "Fore:" + ForeBrushText;
                }

                if (!string.IsNullOrEmpty(BackBrushText))
                {
                    if (text != null)
                    {
                        text += " ";
                    }
                    text += "Back:" + BackBrushText;
                }

                if (!string.IsNullOrEmpty(FontText))
                {
                    if (text != null)
                    {
                        text += " ";
                    }
                    text += "Face:" + FontText;
                }
                return text;
            }
            return Name;
        }

        public override int GetHashCode()
        {
            if (Name == null)
                return base.GetHashCode();

            return Name.GetHashCode();
        }

        public override bool Equals(object obj) => Equals(obj as ColorSet);
        public bool Equals(ColorSet other)
        {
            if (other == null)
                return false;

            if (Name == null)
                return other.Name == null;

            return Name == other.Name;
        }

        int IComparable.CompareTo(object obj) => CompareTo(obj as ColorSet);
        public int CompareTo(ColorSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Name.CompareTo(other.Name);
        }

        private static Tuple<Typeface, double> ConvertToTypeface(string text, Tuple<Typeface, double> defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            try
            {
                var font = (System.Drawing.Font)(new System.Drawing.FontConverter().ConvertFromString(text));
                var tf = new Typeface(font.FontFamily.ToFontFamily(), font.GetStyle(), font.GetWeight(), font.GetStretch());
                return new Tuple<Typeface, double>(tf, font.Size);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static Brush ConvertToBrush(string text, Brush defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            try
            {
                return (Brush)new BrushConverter().ConvertFromString(text);
            }
            catch
            {
                return defaultValue;
            }
        }

        private Pen ConvertToPen(string text, Pen defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            try
            {
                return new Pen((Brush)new BrushConverter().ConvertFromString(text), FrameWidth);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
