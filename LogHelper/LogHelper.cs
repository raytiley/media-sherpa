using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using System.Reflection;

namespace Sherpa.LogHelper
{
    public static class LogHelper
    {
        private static ILog m_Logger = null;

        public static ILog Logger
        {
            get
            {
                if (m_Logger == null)
                {
                    SetupLogger();
                }
                return m_Logger;
            }

        }

        private static void SetupLogger()
        {
            m_Logger = LogManager.GetLogger("root");

            var logConfig = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Log.config");
            FileInfo info = new FileInfo(logConfig);

            log4net.Config.XmlConfigurator.ConfigureAndWatch(info);
        }
    }
}
