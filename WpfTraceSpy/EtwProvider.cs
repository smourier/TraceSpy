using System;
using System.Collections;

namespace TraceSpy
{
    public class EtwProvider : DictionaryObject, IEquatable<EtwProvider>
    {
        public EtwProvider()
        {
            IsActive = true;
            TraceLevel = (byte)EtwTraceLevel.Verbose;
        }

        public bool IsActive { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public string Description { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public Guid Guid { get => DictionaryObjectGetPropertyValue<Guid>(); set => DictionaryObjectSetPropertyValue(value); }
        public byte TraceLevel { get => DictionaryObjectGetPropertyValue<byte>(); set => DictionaryObjectSetPropertyValue(value); }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description))
                return Guid.ToString();

            return Guid + " '" + Description + "'";
        }

        public override int GetHashCode() => Guid.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as EtwProvider);

        public bool Equals(EtwProvider other)
        {
            if (other == null)
                return false;

            return Guid.Equals(other.Guid);
        }

        public EtwProvider Clone()
        {
            var clone = new EtwProvider();
            CopyTo(clone);
            return clone;
        }

        protected override IEnumerable DictionaryObjectGetErrors(string propertyName)
        {
            if (propertyName == null || propertyName == nameof(Guid))
            {
                if (Guid == Guid.Empty)
                    yield return "Guid cannot be empty.";
            }

            if (propertyName == null || propertyName == nameof(Description))
            {
                if (string.IsNullOrWhiteSpace(Description))
                    yield return "Description cannot be empty nor whitespaces only.";
            }
        }
    }
}