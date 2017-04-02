using System;
using System.Configuration;

namespace TraceSpyService.Configuration
{
    public class PrefixElement : ConfigurationElement
    {
        [ConfigurationProperty("uri", IsRequired = true, IsKey = true)]
        public string Uri
        {
            get
            {
                return (string)base["uri"];
            }
        }

        [ConfigurationProperty("basePath")]
        public string BasePath
        {
            get
            {
                string path = (string)base["basePath"];
                if (string.IsNullOrWhiteSpace(path) && Uri != null)
                {
                    // we need to support + and *
                    // http://msdn.microsoft.com/en-us/library/windows/desktop/aa364698(v=vs.85).aspx
                    string noWildcardUri = Uri.Replace("//*", "//dummy").Replace("//+", "//dummy");
                    Uri uri = new Uri(noWildcardUri);
                    path = uri.AbsolutePath;
                }
                return path;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base["enabled"];
            }
        }
    }
}
