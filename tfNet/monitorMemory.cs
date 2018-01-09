using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Threading;

namespace tfNet
{
    public class monitorMemory : Program
    {
        // This method will be called when the thread is started. 
        public void DoWork()
        {
            List<long> lala = new List<long>();
            bool prntMsg = false;
            long memToUse = -1;
            if ((int)Environment.OSVersion.Platform == 2)
            {
                foreach (ManagementObject item in new ManagementClass("Win32_ComputerSystem").GetInstances())
                {
                    memToUse = Convert.ToInt64((memPrcntg * Math.Round(Convert.ToDouble(item.Properties["TotalPhysicalMemory"].Value) / 100)));
                }
            }
            else
            {
                memToUse = Convert.ToInt64((memPrcntg * new PerformanceCounter("Mono Memory", "Total Physical Memory").RawValue) / 100);
            }
            while (!_shouldStop)
            {
                //if (!prntMsg && memToUse < System.Diagnostics.Process.GetCurrentProcess().WorkingSet64)
                if (!prntMsg && memToUse < System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64)
                {
                    Console.WriteLine("\nWARNING: the process is using more than " + memPrcntg + "% of the total system memory!");
                    prntMsg = true;
                }
                lala.Add(System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
                Thread.Sleep(1000);
            }
            //Console.WriteLine (lala.Max());
            //Console.WriteLine("worker thread: terminating gracefully.");
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        private volatile bool _shouldStop;
    }
}

