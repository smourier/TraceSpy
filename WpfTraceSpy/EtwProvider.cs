using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TraceSpy
{
    public class EtwProvider : IEquatable<EtwProvider>
    {
        private string _description;
        private string _providerId;
        private byte _traceLevel;

        public EtwProvider()
        {
            Active = true;
            TraceLevel = (byte)EtwTraceLevel.Verbose;
        }

        public bool Active { get; set; }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description == value)
                    return;

                _description = value;
            }
        }

        [XmlIgnore]
        public Guid ProviderGuid { get; private set; }

        public byte TraceLevel
        {
            get
            {
                return _traceLevel;
            }
            set
            {
                if (_traceLevel == value)
                    return;

                _traceLevel = value;
                if (_traceLevel == (byte)EtwTraceLevel.None)
                {
                    _traceLevel = (byte)EtwTraceLevel.Verbose;
                }
            }
        }

        public string ProviderId
        {
            get
            {
                return _providerId;
            }
            set
            {
                if (_providerId == value)
                    return;

                _providerId = value;
                ProviderGuid = new Guid(value);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description))
                return ProviderId;

            return ProviderId + " '" + Description + "'";
        }

        public override int GetHashCode()
        {
            return ProviderGuid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EtwProvider);
        }

        public bool Equals(EtwProvider other)
        {
            if (other == null)
                return false;

            return ProviderGuid.Equals(other.ProviderGuid); 
        }
    }
}
