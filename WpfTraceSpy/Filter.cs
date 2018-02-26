using System;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public class Filter : DictionaryObject, IEquatable<Filter>
    {
        private Regex _regex;
        private bool _regexParsed;

        public Filter()
        {
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
        public FilterColumn FilterColumn { get => DictionaryObjectGetPropertyValue<FilterColumn>(); set => DictionaryObjectSetPropertyValue(value); }
        public string Definition
        {
            get => DictionaryObjectGetPropertyValue<string>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    _regex = null;
                    _regexParsed = false;
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
                    _regex = null;
                    _regexParsed = false;
                }
            }
        }

        public Regex Regex
        {
            get
            {
                if (_regex == null && !_regexParsed)
                {
                    _regexParsed = true;
                    var options = RegexOptions.Compiled;
                    if (IgnoreCase)
                    {
                        options |= RegexOptions.IgnoreCase;
                    }

                    try
                    {
                        _regex = new Regex(Definition, options);
                    }
                    catch (Exception e)
                    {
                        App.AddTrace("*** Error parsing filter regular expression: " + e.Message + Environment.NewLine + "*** This message will only be shown once.");
                    }
                }
                return _regex;
            }
        }

        public override string ToString() => Definition + ":C" + (IgnoreCase ? "I" : "C");

        public Filter Clone()
        {
            var clone = new Filter();
            CopyTo(clone);
            return clone;
        }

        public override int GetHashCode() => FilterColumn.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as Filter);
        public bool Equals(Filter other)
        {
            if (other == null)
                return false;

            return Definition == other.Definition & IgnoreCase == other.IgnoreCase;
        }
    }
}
