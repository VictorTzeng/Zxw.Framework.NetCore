using Microsoft.Extensions.Configuration;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Helpers
{
    public class ConfigHelper
    {
        private static readonly IConfiguration configuration;
        static ConfigHelper()
        {
            configuration = AutofacContainer.Resolve<IConfiguration>();
        }

        public static IConfigurationSection GetSection(string key)
        {
            return configuration.GetSection(key);
        }

        public static string GetConfigurationValue(string key)
        {
            return configuration[key];
        }

        public static string GetConfigurationValue(string section, string key)
        {
            return GetSection(section)?[key];
        }

        public static string GetConnectionString(string key)
        {
            return configuration.GetConnectionString(key);
        }
    }
}
