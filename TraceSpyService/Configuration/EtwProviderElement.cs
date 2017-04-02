using System;
using System.Configuration;

namespace TraceSpyService.Configuration
{
    public class EtwProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("description")]
        public string Description
        {
            get
            {
                return (string)base["description"];
            }
        }

        [ConfigurationProperty("guid", IsRequired = true, IsKey = true)]
        public Guid Guid    
        {
            get
            {
                return (Guid)base["guid"];
            }
        }

        [ConfigurationProperty("traceLevel", DefaultValue = EtwTraceLevel.Verbose)]
        public EtwTraceLevel TraceLevel
        {
            get
            {
                return (EtwTraceLevel)base["traceLevel"];
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

        [ConfigurationProperty("consoleOutput", DefaultValue = false)]
        public bool ConsoleOutput
        {
            get
            {
                return (bool)base["consoleOutput"];
            }
        }
    }
}
