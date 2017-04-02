using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceSpy
{
    public class QuickColorizer : IEquatable<QuickColorizer>    
    {
        private string _text;
        private bool _ignoreCase;
        private bool _wholeText;
        private ColorSet _colorSet;

        public QuickColorizer()
        {
            IgnoreCase = true;
            Active = true;
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
            }
        }

        public bool WholeText
        {
            get
            {
                return _wholeText;
            }
            set
            {
                if (_wholeText == value)
                    return;

                _wholeText = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value)
                    return;

                _text = value;
            }
        }

        public ColorSet ColorSet
        {
            get
            {
                return _colorSet;
            }
            set
            {
                if (_colorSet == value)
                    return;

                _colorSet = value;
            }
        }

        public override string ToString()
        {
            return Text + " IgnoreCase:" + IgnoreCase + " WholeText:" + WholeText;
        }

        public override int GetHashCode()
        {
            return Text == null ? base.GetHashCode() : Text.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as QuickColorizer);
        }

        public bool Equals(QuickColorizer other)
        {
            if (other == null)
                return false;

            return Text == other.Text & IgnoreCase == other.IgnoreCase;
        }
    }
}
