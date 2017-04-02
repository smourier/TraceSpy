using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class ColorSet : IEquatable<ColorSet>, IComparable, IComparable<ColorSet>, IDisposable
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
                return _foreBrush ?? (_foreBrush = new SolidBrush(ForeColor));
            }
        }

        [XmlIgnore]
        public Brush BackBrush
        {
            get
            {
                return _backBrush ?? (_backBrush = new SolidBrush(BackColor));
            }
        }

        [XmlIgnore]
        public Pen BackPen
        {
            get
            {
                return _backPen ?? (_backPen = new Pen(BackColor, FrameWidth));
            }
        }

        [XmlIgnore]
        public Color ForeColor
        {
            get
            {
                if (string.IsNullOrEmpty(ForeColorName))
                    return Color.Black;

                return ConvertColor(ForeColorName, Color.Black);
            }
            set
            {
                ForeColorName = new ColorConverter().ConvertToInvariantString(value);
                _foreBrush = null;
            }
        }

        internal static string ConvertColor(Color color)
        {
            KnownColor kc = color.ToKnownColor();
            if (kc != 0)
                return kc.ToString();

            return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        internal static Color ConvertColor(string color, Color defaultValue)
        {
            try
            {
                return (Color)(new ColorConverter().ConvertFromInvariantString(color));
            }
#if DEBUG
            catch(Exception e)
            {
                Main.Log("ConvertColor e:" + e);
                return defaultValue;
            }
#else
            catch
            {
                return defaultValue;
            }
#endif
        }

        [XmlIgnore]
        public Color BackColor
        {
            get
            {
                if (string.IsNullOrEmpty(BackColorName))
                    return Color.White;

                return ConvertColor(BackColorName, Color.White);
            }
            set
            {
                BackColorName = new ColorConverter().ConvertToInvariantString(value);
                _backBrush = null;
                _backPen = null;
            }
        }

        [XmlIgnore]
        public Font Font
        {
            get
            {
                if (string.IsNullOrEmpty(FontName))
                    return null;

                return (Font)new FontConverter().ConvertFromInvariantString(FontName);
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

        public override bool Equals(object obj)
        {
            return Equals(obj as ColorSet);
        }

        public bool Equals(ColorSet other)
        {
            if (other == null)
                return false;

            if (Name == null)
                return other.Name == null;

            return Name == other.Name;
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as ColorSet);
        }

        public int CompareTo(ColorSet other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return Name.CompareTo(other.Name);
        }

        void IDisposable.Dispose()
        {
            if (_backBrush != null)
            {
                _backBrush.Dispose();
                _backBrush = null;
            }

            if (_foreBrush != null)
            {
                _foreBrush.Dispose();
                _foreBrush = null;
            }

            if (_backPen != null)
            {
                _backPen.Dispose();
                _backPen = null;
            }
        }
    }
}
