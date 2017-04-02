using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class Colorizer : IEquatable<Colorizer>
    {
        private Regex _regex;
        private string _definition;
        private bool _ignoreCase;

        public Colorizer()
        {
            IgnoreCase = true;
            Active = true;
        }

        public Colorizer(string definition, bool ignoreCase)
            : this()
        {
            Definition = definition;
            IgnoreCase = ignoreCase;
        }

        public bool Active { get; set; }

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

        [XmlIgnore]
        public Regex Regex
        {
            get
            {
                if (_regex == null)
                {
                    RegexOptions options = RegexOptions.Compiled;
                    if (IgnoreCase)
                    {
                        options |= RegexOptions.IgnoreCase;
                    }
                    _regex = new Regex(Definition, options);
                }
                return _regex;
            }
        }

        public override string ToString()
        {
            return Definition + ":" + (IgnoreCase ? "I" : "C");
        }

        public override int GetHashCode()
        {
            return Definition == null ? base.GetHashCode() : Definition.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Colorizer);
        }

        public bool Equals(Colorizer other)
        {
            if (other == null)
                return false;

            return Definition == other.Definition & IgnoreCase == other.IgnoreCase;
        }
    }
}
