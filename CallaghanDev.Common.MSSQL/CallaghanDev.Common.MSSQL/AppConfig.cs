using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.MSSQL
{
    internal class AppConfig
    {
        private static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string ConnectionString
        {
            get
            {
                return GetAppSetting("ConnectionString");
            }
        }


    }
}
