using System.IO;
using System.Text;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace TraceSpy
{
    public abstract class Theme
    {
        public const string DefaultIndexColor = "Black";
        private string _indexColor;
        public string IndexColor { get => _indexColor; set { _indexColor = value; IndexColorBrush = GetBrush(value, DefaultIndexColor); } }

        [XmlIgnore]
        public Brush IndexColorBrush { get; private set; }

        public const string DefaultTicksColor = "Black";
        private string _ticksColor;
        public string TicksColor { get => _ticksColor; set { _ticksColor = value; TicksColorBrush = GetBrush(value, DefaultTicksColor); } }

        [XmlIgnore]
        public Brush TicksColorBrush { get; private set; }

        public const string DefaultProcessColor = "Black";
        private string _processColor;
        public string ProcessColor { get => _processColor; set { _processColor = value; ProcessColorBrush = GetBrush(value, DefaultProcessColor); } }

        [XmlIgnore]
        public Brush ProcessColorBrush { get; private set; }

        public const string DefaultTextColor = "Black";
        private string _textColor;
        public string TextColor { get => _textColor; set { _textColor = value; TextColorBrush = GetBrush(value, DefaultTextColor); } }

        [XmlIgnore]
        public Brush TextColorBrush { get; private set; }

        public const string DefaultRangeTextColor = "Black";
        private string _rangeTextColor;
        public string RangeTextColor { get => _rangeTextColor; set { _rangeTextColor = value; RangeTextColorBrush = GetBrush(value, DefaultRangeTextColor); } }

        [XmlIgnore]
        public Brush RangeTextColorBrush { get; private set; }

        public const string DefaultRangeBackColor = "White";
        private string _rangeBackColor;
        public string RangeBackColor { get => _rangeBackColor; set { _rangeBackColor = value; RangeBackColorBrush = GetBrush(value, DefaultRangeBackColor); } }

        [XmlIgnore]
        public Brush RangeBackColorBrush { get; private set; }

        public const string DefaultErrorBackColor = "DarkOrange";
        private string _errorBackColor;
        public string ErrorBackColor { get => _errorBackColor; set { _errorBackColor = value; ErrorBackColorBrush = GetBrush(value, DefaultErrorBackColor); } }

        [XmlIgnore]
        public Brush ErrorBackColorBrush { get; private set; }

        public const string DefaultWarningBackColor = "Yellow";
        private string _WarningBackColor;
        public string WarningBackColor { get => _WarningBackColor; set { _WarningBackColor = value; WarningBackColorBrush = GetBrush(value, DefaultWarningBackColor); } }

        [XmlIgnore]
        public Brush WarningBackColorBrush { get; private set; }

        public const string DefaultListViewBackColor = "White";
        private string _ListViewBackColor;
        public string ListViewBackColor { get => _ListViewBackColor; set { _ListViewBackColor = value; ListViewBackColorBrush = GetBrush(value, DefaultListViewBackColor); } }

        [XmlIgnore]
        public Brush ListViewBackColorBrush { get; private set; }

        public const string DefaultListViewTextColor = "Black";
        private string _ListViewTextColor;
        public string ListViewTextColor { get => _ListViewTextColor; set { _ListViewTextColor = value; ListViewTextColorBrush = GetBrush(value, DefaultListViewTextColor); } }

        [XmlIgnore]
        public Brush ListViewTextColorBrush { get; private set; }

        public const string DefaultMenuTextColor = "Black";
        private string _MenuTextColor;
        public string MenuTextColor { get => _MenuTextColor; set { _MenuTextColor = value; MenuTextColorBrush = GetBrush(value, DefaultMenuTextColor); } }

        [XmlIgnore]
        public Brush MenuTextColorBrush { get; private set; }

        public const string DefaultMenuBackColor = "#F0F0F0";
        private string _MenuBackColor;
        public string MenuBackColor { get => _MenuBackColor; set { _MenuBackColor = value; MenuBackColorBrush = GetBrush(value, DefaultMenuBackColor); } }

        [XmlIgnore]
        public Brush MenuBackColorBrush { get; private set; }

        public const string DefaultSelectedTextColor = "Black";
        private string _SelectedTextColor;
        public string SelectedTextColor { get => _SelectedTextColor; set { _SelectedTextColor = value; SelectedTextColorBrush = GetBrush(value, DefaultSelectedTextColor); } }

        [XmlIgnore]
        public Brush SelectedTextColorBrush { get; private set; }

        private static Brush GetBrush(string text, string defaultColor)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (string.IsNullOrEmpty(defaultColor) || string.Compare(text, defaultColor, true) == 0)
                    return new SolidColorBrush();

                return GetBrush(defaultColor, null);
            }

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(text);
                return new SolidColorBrush(color);
            }
            catch
            {
                if (string.IsNullOrEmpty(defaultColor) || string.Compare(text, defaultColor, true) == 0)
                    return new SolidColorBrush();

                return GetBrush(defaultColor, null);
            }
        }

        public abstract string Name { get; }

        public virtual void Save()
        {
            var filePath = Path.Combine(Path.GetDirectoryName(WpfSettings.ConfigurationFilePath), Name + ".xml");
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(GetType());
                serializer.Serialize(writer, this);
            }
        }

        public static Theme Load(string name)
        {
            name = name.Nullify() ?? LightTheme.ThemeName;

            var filePath = Path.Combine(Path.GetDirectoryName(WpfSettings.ConfigurationFilePath), name + ".xml");
            if (!File.Exists(filePath))
            {
                try
                {
                    using (var reader = new XmlTextReader(filePath))
                    {
                        if (string.Compare(name, LightTheme.ThemeName, true) == 0)
                        {
                            var deserializer = new XmlSerializer(typeof(LightTheme));
                            return (LightTheme)deserializer.Deserialize(reader);
                        }

                        if (string.Compare(name, DarkTheme.ThemeName, true) == 0)
                        {
                            var deserializer = new XmlSerializer(typeof(DarkTheme));
                            return (DarkTheme)deserializer.Deserialize(reader);
                        }

                        var deserializer2 = new XmlSerializer(typeof(CustomTheme));
                        var theme = (CustomTheme)deserializer2.Deserialize(reader);
                        theme.ThemeName = name;
                        return theme;
                    }
                }
                catch
                {
                    // continue
                }
            }

            if (name == DarkTheme.ThemeName)
                return new DarkTheme();

            return new LightTheme();
        }
    }

    public class CustomTheme : Theme
    {
        public override string Name => ThemeName;
        internal string ThemeName { get; set; }
    }

    public class LightTheme : Theme
    {
        public const string ThemeName = "Light";

        public LightTheme()
        {
            IndexColor = DefaultIndexColor;
            TicksColor = DefaultTicksColor;
            ProcessColor = DefaultProcessColor;
            TextColor = DefaultTextColor;
            RangeTextColor = DefaultRangeTextColor;
            RangeBackColor = DefaultRangeBackColor;
            ErrorBackColor = DefaultErrorBackColor;
            WarningBackColor = DefaultWarningBackColor;
            ListViewBackColor = DefaultListViewBackColor;
            ListViewTextColor = DefaultListViewTextColor;
            MenuBackColor = DefaultMenuBackColor;
            MenuTextColor = DefaultMenuTextColor;
            SelectedTextColor = DefaultSelectedTextColor;
        }

        public override string Name => ThemeName;
    }

    public class DarkTheme : Theme
    {
        public const string ThemeName = "Dark";

        public DarkTheme()
        {
            IndexColor = "#CCCCCC";
            TicksColor = IndexColor;
            ProcessColor = IndexColor;
            TextColor = IndexColor;
            RangeTextColor = IndexColor;
            RangeBackColor = "#1F1F1F";
            ErrorBackColor = RangeBackColor;
            WarningBackColor = RangeBackColor;
            ListViewBackColor = RangeBackColor;
            ListViewTextColor = IndexColor;
            MenuBackColor = "#3F3F3F";
            MenuTextColor = IndexColor;
            SelectedTextColor = RangeBackColor;
        }

        public override string Name => ThemeName;
    }
}
