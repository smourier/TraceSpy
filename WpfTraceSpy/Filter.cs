using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public class Filter : DictionaryObject, IEquatable<Filter>
    {
        private Lazy<Regex> _regex;

        public Filter()
        {
            _regex = new Lazy<Regex>(GetRegex, true);
            IgnoreCase = true;
            IsActive = true;
        }

        public Filter(string definition, bool ignoreCase)
            : this()
        {
            Definition = definition;
            IgnoreCase = ignoreCase;
        }

        public bool IsActive { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        public FilterType Type
        {
            get => DictionaryObjectGetPropertyValue<FilterType>();
            set
            {
                DictionaryObjectSetPropertyValue(value);
                OnPropertyChanged(nameof(IsFilterTypeText));
                OnPropertyChanged(nameof(IsFilterTypeRegex));
            }
        }

        public bool IsFilterTypeText
        {
            get => Type == FilterType.Text;
            set
            {
                if (value)
                {
                    Type = FilterType.Text;
                }
            }
        }

        public bool IsFilterTypeRegex
        {
            get => Type == FilterType.Regex;
            set
            {
                if (value)
                {
                    Type = FilterType.Regex;
                }
            }
        }

        public FilterColumn Column
        {
            get => DictionaryObjectGetPropertyValue<FilterColumn>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(IsFilterColumnText));
                    OnPropertyChanged(nameof(IsFilterColumnProcessName));
                }
            }
        }

        public bool IsFilterColumnText
        {
            get => Column == FilterColumn.Text;
            set
            {
                if (value)
                {
                    Column = FilterColumn.Text;
                }
            }
        }

        public bool IsFilterColumnProcessName
        {
            get => Column == FilterColumn.ProcessName;
            set
            {
                if (value)
                {
                    Column = FilterColumn.ProcessName;
                }
            }
        }

        public string Definition
        {
            get => DictionaryObjectGetPropertyValue<string>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    _regex = new Lazy<Regex>(GetRegex, true);
                }
            }
        }

        public bool IgnoreCase
        {
            get => DictionaryObjectGetPropertyValue<bool>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    _regex = new Lazy<Regex>(GetRegex, true);
                }
            }
        }

        public Regex Regex => _regex.Value;

        private Regex GetRegex()
        {
            if (Type == FilterType.Regex)
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
                catch (Exception e)
                {
                    App.AddTrace(TraceLevel.Error, "*** Error parsing filter '" + Definition + "' regular expression: " + e.Message + Environment.NewLine + "*** This message will only be shown once.");
                }
            }
            return null;
        }

        public override string ToString() => Definition + ":C" + (IgnoreCase ? "I" : "C");

        public Filter Clone()
        {
            var clone = new Filter();
            CopyTo(clone);
            return clone;
        }

        protected override IEnumerable DictionaryObjectGetErrors(string propertyName)
        {
            if (propertyName == null || propertyName == nameof(Definition))
            {
                if (string.IsNullOrWhiteSpace(Definition))
                    yield return "Definition cannot be empty nor whitespaces only.";
            }
        }

        public bool ExcludeLine(string line, string processName)
        {
            switch (Type)
            {
                case FilterType.Regex:
                    switch (Column)
                    {
                        case FilterColumn.ProcessName:
                            return Regex != null && processName != null && Regex.Match(processName).Success;

                        default:
                            return Regex != null && line != null && Regex.Match(line).Success;
                    }

                default:
                    var sc = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    switch (Column)
                    {
                        case FilterColumn.ProcessName:
                            return Definition != null && processName != null && processName.IndexOf(Definition, sc) >= 0;

                        default:
                            return Definition != null && line != null && line.IndexOf(Definition, sc) >= 0;
                    }
            }
        }

        public override int GetHashCode() => Column.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as Filter);
        public bool Equals(Filter other)
        {
            if (other == null)
                return false;

            return Definition == other.Definition &&
                IgnoreCase == other.IgnoreCase &&
                Type == other.Type &&
                Column == other.Column &&
                IsActive == other.IsActive;
        }
    }
}
