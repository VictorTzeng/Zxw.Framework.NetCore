using System.Configuration;

namespace Zxw.Framework.Helpers
{
    public class ConfigHelper
    {
        public static string GetAppSettingsValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
