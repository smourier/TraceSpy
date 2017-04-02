using System;
using System.Configuration;

namespace TraceSpyService.Configuration
{
    [ConfigurationCollection(typeof(PrefixElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class PrefixElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PrefixElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((PrefixElement)element).Uri;
        }

        public new PrefixElement this[string uri]
        {
            get
            {
                if (uri == null)
                    throw new ArgumentNullException("uri");

                return (PrefixElement)BaseGet(uri);
            }
        }

        public string GetRelativePath(string rawUrl)
        {
            if (rawUrl == null)
                throw new ArgumentNullException("rawUrl");

            foreach (PrefixElement pe in this)
            {
                if (pe.BasePath == null)
                    continue;

                if (rawUrl.StartsWith(pe.BasePath, StringComparison.OrdinalIgnoreCase))
                    return rawUrl.Substring(pe.BasePath.Length);
            }
            return null;
        }
    }
}
