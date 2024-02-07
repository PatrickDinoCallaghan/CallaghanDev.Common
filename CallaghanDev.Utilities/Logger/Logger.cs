using log4net;
using log4net.Core;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.IO;

namespace CallaghanDev.Utilities
{
    public interface ICallaghanDevLogger : ILog { }
    public class Logger : ICallaghanDevLogger
    {
        public static readonly object LockObject=  new object();
        public static Logger _instance;

        public static Logger GetInstance(string to, string from, string frompassword, string smtphost, string authendicationvalue, bool enablessl)
        {
            lock (LockObject)
            {
                if (_instance == null)
                {
                    _instance = new Logger(to,  from,  frompassword,  smtphost,  authendicationvalue,  enablessl);
                }
                return _instance;
            }
        }

        public static Logger GetInstance()
        {
            lock (LockObject)
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
                return _instance;
            }
        }

        #region Private fields 
        private bool _EmailLoggerActive;
        private static readonly log4net.ILog Filelog = log4net.LogManager.GetLogger("FileLogger");
        private static readonly log4net.ILog Emaillog = log4net.LogManager.GetLogger("EmailLogger");


        private const int SRC_COPY = 0xCC0020;

        public bool IsDebugEnabled { get { return Filelog.IsDebugEnabled; } }

        bool IsInfoEnabled { get { return Filelog.IsInfoEnabled; } }

        bool IsWarnEnabled { get { return Filelog.IsWarnEnabled; } }

        bool IsErrorEnabled { get { return Filelog.IsErrorEnabled; } }


        bool IsFatalEnabled { get { return Filelog.IsFatalEnabled; } }

        bool ILog.IsInfoEnabled => Filelog.IsInfoEnabled;

        bool ILog.IsWarnEnabled => Filelog.IsWarnEnabled;

        bool ILog.IsErrorEnabled => Filelog.IsErrorEnabled;

        bool ILog.IsFatalEnabled => Filelog.IsFatalEnabled;

        ILogger ILoggerWrapper.Logger => Filelog.Logger;

        #endregion

        private void GenerateEmailLogerSetting(string to, string from, string frompassword, string smtphost, string authendicationvalue, bool enablessl)
        {
            string defaultstring; // This is the log string

            #region log4net config text
            defaultstring = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
    <configuration>
	<configSections>
		<section name=""log4net"" type=""log4net.Config.Log4NetConfigurationSectionHandler, log4net"" />
	</configSections>
	<log4net>
		<appender name=""LogFileAppender"" type=""log4net.Appender.RollingFileAppender"">
			<file type=""log4net.Util.PatternString"" value=""%Activity.log"" />
			<encoding value=""utf-8""/>
			<appendToFile value=""true""/>
			<rollingStyle value=""Size""/>
			<maxSizeRollBackups value=""10""/>
			<maximumFileSize value=""10MB""/>
			<staticLogFileName value=""true""/>
			<layout type=""log4net.Layout.PatternLayout"">
				<conversionPattern value=""%date [%thread] - %message%newline""/>
			</layout>
		</appender>
		<logger name=""FileLogger"">
			<level value=""Error"" />
			<appender-ref ref=""LogFileAppender"" />
		</logger>
		<appender name=""SmtpAppender"" type=""log4net.Appender.SmtpAppender"">
			<to value=""" + to + @""" />
			<from value=""" + from + @""" />
			<subject value=""ERROR:"" />
			<smtpHost value=""" + smtphost + @""" />
			<port value=""587""/>
			<authentication value=""Basic"" />
			<username value=""" + from + @"""/>
			<password value="""+ frompassword +@"""/>
			<EnableSsl value=""" + enablessl.ToString() + @""" />
			<bufferSize value=""1"" />
			<lossy value=""false"" />
			<evaluator type=""log4net.Core.LevelEvaluator"">
				<threshold value=""ERROR""/>
			</evaluator>
			<layout type=""log4net.Layout.PatternLayout"">
				<conversionPattern value=""%property{log4net:UserName}%newline %property{log4net:HostName}%newline %level :: %message  %newlineLogger: %logger%newlineDate: %date"" />
			</layout>
		</appender>
		<logger name=""EmailLogger"">
			<level value=""Error"" />
			<appender-ref ref=""SmtpAppender"" />
		</logger>
	</log4net>
    </configuration>";
            #endregion


            // Get the path to the folder where the executable is running from
            string folderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            // Create a file path by combining the folder path with the file name
            string filePath = Path.Combine(folderPath, "Log4net.config");

            // Check if file path exists
            if (!File.Exists(filePath))
            {
                // Write the string to the file
                File.WriteAllText(filePath, defaultstring);
            }
        }

        private void GenerateFileLogerSetting()
        {

            string defaultstring; // This is the log string

            #region log4net config text
            defaultstring = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
    <configuration>
	<configSections>
		<section name=""log4net"" type=""log4net.Config.Log4NetConfigurationSectionHandler, log4net"" />
	</configSections>
	<log4net>
		<appender name=""LogFileAppender"" type=""log4net.Appender.RollingFileAppender"">
			<file type=""log4net.Util.PatternString"" value=""%Activity.log"" />
			<encoding value=""utf-8""/>
			<appendToFile value=""true""/>
			<rollingStyle value=""Size""/>
			<maxSizeRollBackups value=""10""/>
			<maximumFileSize value=""10MB""/>
			<staticLogFileName value=""true""/>
			<layout type=""log4net.Layout.PatternLayout"">
				<conversionPattern value=""%date [%thread] - %message%newline""/>
			</layout>
		</appender>
		<logger name=""FileLogger"">
			<level value=""Error"" />
			<appender-ref ref=""LogFileAppender"" />
		</logger>
	</log4net>
    </configuration>";
            #endregion


            // Get the path to the folder where the executable is running from
            string folderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            Console.WriteLine(folderPath);
            // Create a confile path by combining the folder path with the file name
            string filePath = Path.Combine(folderPath, "Log4net.config");

            // Check if file path exists
            if (!File.Exists(filePath))
            {
                // Write the string to the file
                File.WriteAllText(filePath, defaultstring);
            }
            Console.WriteLine("here2");
        }

        public Logger()
        {
            GenerateFileLogerSetting();
            _EmailLoggerActive = false;
            string AppConfig = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Log4net.config");

            if (!System.IO.File.Exists(AppConfig))
            {
                throw new Exception("Logger not set, cannot find logger config file at " + AppConfig);
            }

            log4net.Config.XmlConfigurator.Configure(new FileInfo(AppConfig));
        }

        public Logger(string to, string from, string frompassword, string smtphost, string authendicationvalue, bool enablessl)
        {
            GenerateEmailLogerSetting(to,  from,  frompassword,  smtphost ,  authendicationvalue ,  enablessl);

            _EmailLoggerActive = true;
            string AppConfig = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Log4net.config");

            if (!System.IO.File.Exists(AppConfig))
            {
                throw new Exception("Logger not set, cannot find logger config file at " + AppConfig);
            }

            log4net.Config.XmlConfigurator.Configure(new FileInfo(AppConfig));
        }

        public void Info(object message)
        {
            Filelog.Error(message);
            System.Diagnostics.Debug.WriteLine("INFO:" + message);
        }

        void ILog.Info(object message, Exception exception)
        {
            Filelog.Info(message, exception);
            System.Diagnostics.Debug.WriteLine("INFO:" + message);
        }

        public void Error(object message)
        {
            Filelog.Error(message);
            if (_EmailLoggerActive)
            {
                Emaillog.Error(message);
            }
            System.Diagnostics.Debug.WriteLine("ERROR:" + message);
        }

        public void Error(object message, Exception exception)
        {
            Filelog.Error(message, exception); 
            if (_EmailLoggerActive)
            {
                Emaillog.Error(message, exception);
            }
            System.Diagnostics.Debug.WriteLine("ERROR:" + message);
        }

        public void Fatal(object message)
        {
            Filelog.Fatal(message);
            if (_EmailLoggerActive)
            {
                Emaillog.Fatal(message);
            }
            System.Diagnostics.Debug.WriteLine("FATAL:" + message);

        }

        public void Fatal(object message, Exception exception)
        {
            Filelog.Fatal(message, exception);
            if (_EmailLoggerActive)
            {
                Emaillog.Fatal(message, exception);
            }
            System.Diagnostics.Debug.WriteLine("FATAL:" + message);
        }

        void ILog.Debug(object message)
        {
            Filelog.Debug(message);
            System.Diagnostics.Debug.WriteLine("DEBUG:" + message);
        }

        void ILog.Debug(object message, Exception exception)
        {
            Filelog.Debug(message, exception);
            System.Diagnostics.Debug.WriteLine("DEBUG:" + message);
        }

        void ILog.Warn(object message)
        {
            Filelog.Warn(message);
            System.Diagnostics.Debug.WriteLine("WARN:" + message);
        }

        void ILog.Warn(object message, Exception exception)
        {
            Filelog.Warn(message, exception);
            System.Diagnostics.Debug.WriteLine("WARN:" + message);
        }

        #region Formats
        public void ErrorFormat(string format, params object[] args)
        {

            Filelog.ErrorFormat(format, args);
            Emaillog.ErrorFormat(format, args);
        }
        public void ErrorFormat(string format, object arg0)
        {

            Filelog.ErrorFormat(format, arg0);
            Emaillog.ErrorFormat(format, arg0);
        }
        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Filelog.ErrorFormat(format, arg0, arg1);

            Emaillog.ErrorFormat(format, arg0, arg1);

        }
        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Filelog.ErrorFormat(format, arg0, arg1, arg2);

            Emaillog.ErrorFormat(format, arg0, arg1, arg2);
        }
        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Filelog.ErrorFormat(provider, format, args);
            Emaillog.ErrorFormat(provider, format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Filelog.FatalFormat(format, args);
            Emaillog.FatalFormat(format, args);
        }
        public void FatalFormat(string format, object arg0)
        {
            Filelog.FatalFormat(format, arg0);
            Emaillog.FatalFormat(format, arg0);
        }
        public void FatalFormat(string format, object arg0, object arg1)
        {
            Filelog.FatalFormat(format, arg0, arg1);
            Emaillog.FatalFormat(format, arg0, arg1);

        }
        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Filelog.FatalFormat(format, arg0, arg1, arg2);
            Emaillog.FatalFormat(format, arg0, arg1, arg2);

        }
        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Filelog.FatalFormat(provider, format, args);
        }

        void ILog.DebugFormat(string format, params object[] args)
        {
            Filelog.DebugFormat(format, args);
            Filelog.DebugFormat(format, args);
        }
        void ILog.DebugFormat(string format, object arg0)
        {
            Filelog.DebugFormat(format, arg0);
            Emaillog.DebugFormat(format, arg0);
        }
        void ILog.DebugFormat(string format, object arg0, object arg1)
        {
            Filelog.DebugFormat(format, arg0, arg1);
            Emaillog.DebugFormat(format, arg0, arg1);
        }
        void ILog.DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Filelog.DebugFormat(format, arg0, arg1, arg2);
            Emaillog.DebugFormat(format, arg0, arg1, arg2);
        }
        void ILog.DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Filelog.DebugFormat(provider, format, args);
            Emaillog.DebugFormat(provider, format, args);
        }

        void ILog.InfoFormat(string format, params object[] args)
        {
            Filelog.InfoFormat(format, args);
        }
        void ILog.InfoFormat(string format, object arg0)
    {
        Filelog.InfoFormat(format, arg0);
    }
        void ILog.InfoFormat(string format, object arg0, object arg1)
    {
        Filelog.InfoFormat(format, arg0, arg1);
    }
        void ILog.InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Filelog.InfoFormat(format, arg0, arg1, arg2);
        }
        void ILog.InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Filelog.InfoFormat(provider, format, args);
        }

        void ILog.WarnFormat(string format, params object[] args)
        {
            Filelog.WarnFormat(format, args);
        }
        void ILog.WarnFormat(string format, object arg0)
        {
            Filelog.WarnFormat(format, arg0);
        }
        void ILog.WarnFormat(string format, object arg0, object arg1)
        {
            Filelog.WarnFormat(format, arg0, arg1);
        }
        void ILog.WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Filelog.WarnFormat(format, arg0, arg1, arg2);
        }
        void ILog.WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            Filelog.WarnFormat(provider, format, args);
        }

        #endregion
    }

}
