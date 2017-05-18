using System;
using System.Configuration;

namespace TraceSpyService.Configuration
{
    [ConfigurationCollection(typeof(EtwProviderElement), AddItemName = "etwProvider", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EtwProviderElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EtwProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return ((EtwProviderElement)element).Guid;
        }

        public new EtwProviderElement this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                return (EtwProviderElement)BaseGet(name);
            }
        }
    }
}
