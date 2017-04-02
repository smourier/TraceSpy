using System.Collections;
using System.Configuration;
using System.Xml;

namespace TraceSpyService.Configuration
{
    public class ServiceSection : ConfigurationSection
    {
        public static readonly string SectionName = "traceSpyService";

        private static ServiceSection _current;
        private Hashtable _templates = new Hashtable();

        internal ServiceSection()
        {
        }

        public static ServiceSection Current
        {
            get
            {
                if (_current == null)
                {
                    _current = ConfigurationManager.GetSection(SectionName) as ServiceSection;
                    if (_current == null)
                    {
                        _current = new ServiceSection();
                    }
                }
                return _current;
            }
        }

        public static ServiceSection Get(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return Current;

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            return Get(doc.SelectSingleNode("//" + SectionName));
        }

        public static ServiceSection Get(XmlNode node)
        {
            if (node == null)
                return Current;

            using (XmlNodeReader reader = new XmlNodeReader(node))
            {
                return Get(reader);
            }
        }

        public static ServiceSection Get(XmlReader reader)
        {
            if (reader == null)
                return Current;

            ServiceSection section = new ServiceSection();
            section.DeserializeSection(reader);
            return section;
        }

        [ConfigurationProperty("consoleCloseMaxWaitTime", DefaultValue = -1)]
        public int ConsoleCloseMaxWaitTime
        {
            get
            {
                return (int)this["consoleCloseMaxWaitTime"];
            }
        }

        [ConfigurationProperty("stopServicesMaxWaitTime", DefaultValue = -1)]
        public int StopServicesMaxWaitTime
        {
            get
            {
                return (int)this["stopServicesMaxWaitTime"];
            }
        }

        [ConfigurationProperty("waitExitContext", DefaultValue = true)]
        public bool WaitExitContext
        {
            get
            {
                return (bool)base["waitExitContext"];
            }
        }

        [ConfigurationProperty("monitoringPeriod", DefaultValue = 1000)]
        public int MonitoringPeriod
        {
            get
            {
                return (int)this["monitoringPeriod"];
            }
        }

        [ConfigurationProperty("stopServicesAsync", DefaultValue = true)]
        public bool StopServicesAsync
        {
            get
            {
                return (bool)base["stopServicesAsync"];
            }
        }

        [ConfigurationProperty("startServicesAsync", DefaultValue = true)]
        public bool StartServicesAsync
        {
            get
            {
                return (bool)base["startServicesAsync"];
            }
        }

        [ConfigurationProperty("stopOnServiceStartError", DefaultValue = true)]
        public bool StopOnServiceStartError
        {
            get
            {
                return (bool)base["stopOnServiceStartError"];
            }
        }

        [ConfigurationProperty("webServer", Options = ConfigurationPropertyOptions.IsRequired)]
        public WebServerElement WebServer
        {
            get
            {
                return (WebServerElement)this["webServer"];
            }
        }

        [ConfigurationProperty("etwListener", Options = ConfigurationPropertyOptions.IsRequired)]
        public EtwListenerElement EtwListener
        {
            get
            {
                return (EtwListenerElement)this["etwListener"];
            }
        }
    }
}
