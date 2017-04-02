using System.Configuration;

namespace TraceSpyService.Configuration
{
    public class EtwListenerElement : ConfigurationElement
    {
        [ConfigurationProperty("capacity", DefaultValue = 10000)]
        public int Capacity
        {
            get
            {
                return (int)base["capacity"];
            }
        }

        [ConfigurationProperty("consoleOutput", DefaultValue = false)]
        public bool ConsoleOutput
        {
            get
            {
                return (bool)base["consoleOutput"];
            }
        }

        [ConfigurationProperty("etwProviders", IsDefaultCollection = false, IsRequired = true)]
        public EtwProviderElementCollection Providers
        {
            get
            {
                return (EtwProviderElementCollection)this["etwProviders"];
            }
        }
    }
}
