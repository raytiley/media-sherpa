using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;

namespace Sherpa.Classes
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

            FileInfo info = new FileInfo("Log.config");

            log4net.Config.XmlConfigurator.ConfigureAndWatch(info);
        }
    }
}
