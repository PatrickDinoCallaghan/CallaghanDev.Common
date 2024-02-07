using log4net;
using Microsoft.Extensions.Configuration;

namespace CallaghanDev.ConfigManagement
{
    internal class appsettings
    {
        private static IConfiguration _instance = null;
        private static readonly object _Config_padlock = new object();

        public const string Dir = @"C:\Users\p.callaghan\source\repos\CallaghanDev\CallaghanDev.Common\CallaghanDev.ConfigManagement\";
        public const string configFile = @"appsettings.json";
        public const string LoggerFile = @"Log4net.config";
        public static IConfiguration configuration
        {
            get
            {
                lock (_Config_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigurationBuilder()
                            .AddJsonFile(Dir+ configFile, optional: true, reloadOnChange: true)
                            .Build();
                    }
                    return _instance;
                }
            }
        }

    }
}
