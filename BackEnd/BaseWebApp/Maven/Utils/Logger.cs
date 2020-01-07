using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils
{
    public class Logger
    {
        private static ILog _logger = null;

        private static ILog Log4Net
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger(typeof(Logger));
                    log4net.Config.XmlConfigurator.Configure();
                }
                return _logger;
            }
        }

        public static void Log(string format, object arg0)
        {
            Log(string.Format(format, arg0));
        }

        public static void Log(string format, object arg0, object arg1)
        {
            Log(string.Format(format, arg0, arg1));
        }

        public static void Log()
        {
            Log("");
        }

        public static void Log(string log)
        {
            Debug.WriteLine(DateTime.Now + " :: " + log);

            Log4Net.Info(log);
        }
    }
}