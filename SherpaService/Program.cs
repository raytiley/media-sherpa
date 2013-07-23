using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Sherpa.Classes;

namespace SherpaService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            LogHelper.Logger.Info("Starting MediaSherpa Service");

#if (!DEBUG)
                System.ServiceProcess.ServiceBase[] ServicesToRun;
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service1() };
                System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#else
            // Debug code: this allows the process to run as a non-service.
            // It will kick off the service start point, but never kill it.
            // Shut down the debugger to exit
            Service1 service = new Service1();
            service.StartService();
            // Put a breakpoint on the following line to always catch
            // your service when it has finished its work
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
        }
    }
}
