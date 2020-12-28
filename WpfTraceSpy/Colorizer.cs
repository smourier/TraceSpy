using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class Colorizer : DictionaryObject, IComparable, IComparable<Colorizer>
    {
        private Lazy<Regex> _regex;

        public Colorizer()
        {
            _regex = new Lazy<Regex>(GetRegex, true);
        }

        public Colorizer(string definition, bool ignoreCase)
            : this()
        {
            Definition = definition;
            IgnoreCase = ignoreCase;
        }

        public bool IsActive { get => DictionaryObjectGetPropertyValue(true); set => DictionaryObjectSetPropertyValue(value); }
        public bool IgnoreCase { get => DictionaryObjectGetPropertyValue(true); set => DictionaryObjectSetPropertyValue(value); }
        public string Definition { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [XmlIgnore]
        public Regex Regex => _regex.Value;

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IgnoreCase) || e.PropertyName == nameof(Definition))
            {
                _regex = new Lazy<Regex>(GetRegex, true);
            }
            base.OnPropertyChanged(sender, e);
        }

        private Regex GetRegex()
        {
            var options = RegexOptions.Compiled;
            if (IgnoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }

            try
            {
                return new Regex(Definition, options);
            }
            catch(Exception e)
            {
                App.AddTrace(TraceLevel.Error, "*** Error parsing colorizer '" + Definition + "' regular expression: " + e.Message + Environment.NewLine + "*** This message will only be shown once.");
                return null;
            }
        }

        public Colorizer Clone()
        {
            var clone = new Colorizer();
            CopyTo(clone);
            return clone;
        }

        public override string ToString() => Definition + ":" + (IgnoreCase ? "I" : "C");
        int IComparable.CompareTo(object obj) => CompareTo(obj as Colorizer);
        public int CompareTo(Colorizer other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Definition.CompareTo(Definition);
        }
    }
}
