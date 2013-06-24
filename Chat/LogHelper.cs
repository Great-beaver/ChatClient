using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web;
using log4net;
using log4net.Appender;

namespace Chat
{

    public static class LogHelper
    {
        private static bool isService = false;
        private static ILog log = null;

        /// <summary>
        /// Get Default Logger
        /// </summary>
        public static ILog Log
        {
            get
            {
                if (log == null)
                {
                    if (HttpContext.Current != null)
                    {
                        log4net.Config.XmlConfigurator.Configure();
                    }
                    else
                    {
                        string configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
                        log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath));
                    }
                    log = LogManager.GetLogger("LogHelper");
                }
                return log;
            }
        }

        private static readonly Dictionary<Type, ILog> logs = new Dictionary<Type, ILog>();

        public static ILog GetLogger<T>()
        {
            Type type = typeof(T);
            if (logs.ContainsKey(type))
            {
                return logs[type];
            }
            if (HttpContext.Current != null)
            {
                log4net.Config.XmlConfigurator.Configure();
            }
            else
            {
                string configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath));
            }
            log = LogManager.GetLogger(type);
            logs[type] = log;
            return log;
        }

        public static ILog GetLogger(string logger)
        {
            return LogManager.GetLogger(logger);
        }

        public static bool IsService
        {
            get
            {
                return isService;
            }

            set
            {
                isService = value;
                UpdateAppenders(isService);
            }
        }

        private static void UpdateAppenders(bool isService)
        {
            ResetLog();

            foreach (log4net.Appender.IAppender appender in Log.Logger.Repository.GetAppenders())
            {
                if (appender is EventLogAppender && !isService)
                {
                    EventLogAppender eventAppender = appender as EventLogAppender;
                    eventAppender.Threshold = log4net.Core.Level.Off;
                    break;
                }

                if (appender is ColoredConsoleAppender && isService)
                {
                    ColoredConsoleAppender consoleAppender = appender as ColoredConsoleAppender;
                    consoleAppender.Threshold = log4net.Core.Level.Off;
                    break;
                }
            }
        }

        private static void ResetLog()
        {
            log = null;
        }
    }

    public class LogTraceListener : TraceListener
    {
        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            this.TraceEvent(eventCache, source, eventType, id, String.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    LogHelper.Log.Error(message);
                    break;
                case TraceEventType.Warning:
                    LogHelper.Log.Warn(message);
                    break;
                case TraceEventType.Information:
                    LogHelper.Log.Info(message);
                    break;
                case TraceEventType.Verbose:
                    LogHelper.Log.Debug(message);
                    break;
            }
        }

        public override void Write(string message)
        {
            LogHelper.Log.Info(message);
        }

        public override void WriteLine(string message)
        {
            LogHelper.Log.Info(message);
        }
    }
}
