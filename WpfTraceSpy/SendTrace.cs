using System.Collections;

namespace TraceSpy
{
    public class SendTrace : DictionaryObject
    {
        public string Text { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        public SendTrace Clone()
        {
            var clone = new SendTrace();
            CopyTo(clone);
            return clone;
        }

        protected override IEnumerable DictionaryObjectGetErrors(string propertyName)
        {
            if (propertyName == null || propertyName == nameof(Text))
            {
                if (string.IsNullOrWhiteSpace(Text))
                    yield return "Text cannot be empty.";
            }
        }

        public override string ToString() => Text;
    }
}
