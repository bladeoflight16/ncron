using System.Configuration;
using System.Reflection;
using System.Linq;

namespace NCron
{
    internal static class ApplicationInfo
    {
        private const string NameKey = "NCronAppName";

        public static readonly string ApplicationName;

        static ApplicationInfo()
        {
            if (
                ConfigurationManager.AppSettings.AllKeys.Contains(NameKey)
                && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[NameKey])
            )
            {
                ApplicationName = ConfigurationManager.AppSettings[NameKey];
            }
            else
            {
                ApplicationName = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name;
            }
        }
    }
}