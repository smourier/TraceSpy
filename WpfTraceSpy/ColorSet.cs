using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class ColorSet : DictionaryObject, IEquatable<ColorSet>, IComparable, IComparable<ColorSet>
    {
        private Lazy<Brush> _backBrush;
        private Lazy<Brush> _foreBrush;
        private Lazy<Pen> _backPen;
        private Lazy<Typeface> _typeFace;

        public ColorSet()
        {
            _backBrush = new Lazy<Brush>(GetBackBrush, true);
            _foreBrush = new Lazy<Brush>(GetForeBrush, true);
            _backPen = new Lazy<Pen>(GetBackPen, true);
            _typeFace = new Lazy<Typeface>(GetTypeface, true);
        }

        public ColorSet(string name, string foreBrush, string backBrush)
            : this()
        {
            Name = name;
            ForeBrushText = foreBrush;
            BackBrushText = backBrush;
        }

        public string Name { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public ColorSetDrawMode Mode { get => DictionaryObjectGetPropertyValue(ColorSetDrawMode.Fill); set => DictionaryObjectSetPropertyValue(value); }
        public float FrameWidth { get => DictionaryObjectGetPropertyValue(1.0f); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsModeFrame { get => !IsModeFill; set => IsModeFill = !value; }

        public bool IsModeFill
        {
            get => Mode == ColorSetDrawMode.Fill;
            set
            {
                if (value == IsModeFill)
                    return;

                Mode = value ? ColorSetDrawMode.Fill : ColorSetDrawMode.Frame;
                OnPropertyChanged(nameof(IsModeFrame));
                OnPropertyChanged(nameof(IsModeFill));
            }
        }

        [XmlElement("ForeBrush")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ForeBrushText { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlElement("BackBrush")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string BackBrushText { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlElement("Typeface")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string TypefaceName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlIgnore]
        public Brush ForeBrush => _foreBrush.Value;
        private Brush GetForeBrush() => ConvertToBrush(BackBrushText, Brushes.Black);

        [XmlIgnore]
        public Brush BackBrush => _backBrush.Value;
        private Brush GetBackBrush() => ConvertToBrush(BackBrushText, Brushes.White);

        [XmlIgnore]
        public Pen BackPen => _backPen.Value;
        private Pen GetBackPen() => ConvertToPen(ForeBrushText, new Pen(BackBrush, 1));

        [XmlIgnore]
        public Typeface Typeface => _typeFace.Value;
        private Typeface GetTypeface() => ConvertToTypeface(TypefaceName, App.Current.Settings.TypeFace);

        protected override IEnumerable DictionaryObjectGetErrors(string propertyName)
        {
            if (propertyName == null || propertyName == nameof(Name))
            {
                if (string.IsNullOrWhiteSpace(Name))
                    yield return "Name cannot be empty.";
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
            }
            else if (e.PropertyName == nameof(TypefaceName))
            {
                _typeFace = new Lazy<Typeface>(GetTypeface, true);
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

                if (!string.IsNullOrEmpty(TypefaceName))
                {
                    if (text != null)
                    {
                        text += " ";
                    }
                    text += "Face:" + TypefaceName;
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

        private static Typeface ConvertToTypeface(string text, Typeface defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            try
            {
                return new Typeface(text);
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

        private static Pen ConvertToPen(string text, Pen defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            try
            {
                return new Pen((Brush)new BrushConverter().ConvertFromString(text), 1);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
