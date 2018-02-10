using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class ColorSet : IEquatable<ColorSet>, IComparable, IComparable<ColorSet>
    {
        private Brush _backBrush;
        private Pen _backPen;
        private Brush _foreBrush;
        private string _fontName;
        private string _foreColorName;
        private string _backColorName;
        private float _frameWidth;

        public ColorSet()
        {
            FrameWidth = 1;
            Mode = ColorSetDrawMode.Fill;
        }

        public ColorSet(string name, string foreColorName, string backColorName)
            : this()
        {
            Name = name;
            ForeColorName = foreColorName;
            BackColorName = backColorName;
        }

        public string Name { get; set; }
        public ColorSetDrawMode Mode { get; set; }

        public float FrameWidth
        {
            get
            {
                return _frameWidth;
            }
            set
            {
                if (_frameWidth == value)
                    return;

                _frameWidth = value;
                _backPen = null;
            }
        }

        [XmlIgnore]
        public Brush ForeBrush
        {
            get
            {
                return _foreBrush ?? (_foreBrush = new SolidColorBrush(ForeColor));
            }
        }

        [XmlIgnore]
        public Brush BackBrush
        {
            get
            {
                return _backBrush ?? (_backBrush = new SolidColorBrush(BackColor));
            }
        }

        [XmlIgnore]
        public Pen BackPen
        {
            get
            {
                return _backPen ?? (_backPen = new Pen(new SolidColorBrush(BackColor), FrameWidth));
            }
        }

        [XmlIgnore]
        public Color ForeColor
        {
            get
            {
                if (string.IsNullOrEmpty(ForeColorName))
                    return Colors.Black;

                return ConvertColor(ForeColorName, Colors.Black);
            }
            set
            {
                ForeColorName = new ColorConverter().ConvertToInvariantString(value);
                _foreBrush = null;
            }
        }

        //internal static string ConvertColor(Color color)
        //{
        //    var kc = color.ToKnownColor();
        //    if (kc != 0)
        //        return kc.ToString();

        //    return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        //}

        internal static Color ConvertColor(string color, Color defaultValue)
        {
            try
            {
                return (Color)(new ColorConverter().ConvertFromInvariantString(color));
            }
            catch
            {
                return defaultValue;
            }
        }

        [XmlIgnore]
        public Color BackColor
        {
            get
            {
                if (string.IsNullOrEmpty(BackColorName))
                    return Colors.White;

                return ConvertColor(BackColorName, Colors.White);
            }
            set
            {
                BackColorName = new ColorConverter().ConvertToInvariantString(value);
                _backBrush = null;
                _backPen = null;
            }
        }

        [XmlElement("ForeColor")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ForeColorName
        {
            get
            {
                return _foreColorName;
            }
            set
            {
                if (_foreColorName == value)
                    return;

                _foreColorName = value;
                _foreBrush = null;
            }
        }

        [XmlElement("BackColor")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string BackColorName
        {
            get
            {
                return _backColorName;
            }
            set
            {
                if (_backColorName == value)
                    return;

                _backColorName = value;
                _backBrush = null;
                _backPen = null;
            }
        }

        [XmlElement("Font")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FontName
        {
            get
            {
                return _fontName;
            }
            set
            {
                if (_fontName == value)
                    return;

                _fontName = value;
            }
        }

        public override string ToString()
        {
            if (Name == null)
            {
                string text = string.Empty;
                if (!string.IsNullOrEmpty(ForeColorName))
                {
                    text += "ForeColor:" + ForeColorName;
                }

                if (!string.IsNullOrEmpty(BackColorName))
                {
                    if (text != null)
                    {
                        text += " ";
                    }
                    text += "BackColor:" + BackColorName;
                }

                if (!string.IsNullOrEmpty(FontName))
                {
                    if (text != null)
                    {
                        text += " ";
                    }
                    text += "Font:" + FontName;
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
    }
}
