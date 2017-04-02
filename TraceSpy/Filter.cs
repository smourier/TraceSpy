using System;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public class Filter : IEquatable<Filter>
    {
        private Regex _regex;
        private string _definition;
        private bool _ignoreCase;
        private FilterType _filterType;
        private FilterColumn _filterColumn;
        private bool _regexParsed;

        public Filter()
        {
            IgnoreCase = true;
            Active = true;
        }

        public Filter(FilterType filterType, string definition, bool ignoreCase)
            : this()
        {
            FilterType = filterType;
            Definition = definition;
            IgnoreCase = ignoreCase;
        }

        public bool Active { get; set; }

        public FilterType FilterType
        {
            get
            {
                return _filterType;
            }
            set
            {
                if (_filterType == value)
                    return;

                _filterType = value;
                _regex = null;
            }
        }

        public FilterColumn FilterColumn
        {
            get
            {
                return _filterColumn;
            }
            set
            {
                if (_filterColumn == value)
                    return;

                _filterColumn = value;
                _regex = null;
            }
        }

        public string Definition
        {
            get
            {
                return _definition;
            }
            set
            {
                if (_definition == value)
                    return;

                _definition = value;
                _regex = null;
            }
        }

        public bool IgnoreCase
        {
            get
            {
                return _ignoreCase;
            }
            set
            {
                if (_ignoreCase == value)
                    return;

                _ignoreCase = value;
                _regex = null;
            }
        }

        public Regex Regex
        {
            get
            {
                if (_regex == null && !_regexParsed && FilterType != FilterType.IncludeAll)
                {
                    _regexParsed = true;
                    RegexOptions options = RegexOptions.Compiled;
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
                        Main._instance.Enqueue(Main._processId, "*** Error parsing filter regular expression: " + e.Message + Environment.NewLine + "*** This message will only be shown once.");
                    }
                }
                return _regex;
            }
        }

        public override string ToString()
        {
            return FilterType + ":" + Definition + ":C" + (IgnoreCase ? "I" : "C");
        }

        public override int GetHashCode()
        {
            return FilterType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Filter);
        }

        public bool Equals(Filter other)
        {
            if (other == null)
                return false;

            return FilterType == other.FilterType && Definition == other.Definition & IgnoreCase == other.IgnoreCase;
        }
    }
}
