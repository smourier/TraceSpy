using System.Configuration;
using System.Net;

namespace TraceSpyService.Configuration
{
    public class WebServerElement : ConfigurationElement
    {
        [ConfigurationProperty("login", IsRequired = false)]
        public string Login
        {
            get
            {
                return (string)base["login"];
            }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return (string)base["password"];
            }
        }

        [ConfigurationProperty("realm", DefaultValue = null)]
        public string Realm
        {
            get
            {
                return (string)base["realm"];
            }
        }

#if DEBUG
        [ConfigurationProperty("ignoreWriteExceptions", DefaultValue = false)]
#else
        [ConfigurationProperty("ignoreWriteExceptions", DefaultValue = true)]
#endif
        public bool IgnoreWriteExceptions
        {
            get
            {
                return (bool)base["ignoreWriteExceptions"];
            }
        }

        [ConfigurationProperty("unsafeConnectionNtlmAuthentication", DefaultValue = false)]
        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                return (bool)base["unsafeConnectionNtlmAuthentication"];
            }
        }

        [ConfigurationProperty("authenticationSchemes", DefaultValue = AuthenticationSchemes.Anonymous)]
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                return (AuthenticationSchemes)base["authenticationSchemes"];
            }
        }

        [ConfigurationProperty("prefixes", IsDefaultCollection = false, IsRequired = true)]
        public PrefixElementCollection Prefixes
        {
            get
            {
                return (PrefixElementCollection)this["prefixes"];
            }
        }
    }
}
