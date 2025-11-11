using System;
using System.Threading.Tasks;
using CEAM.AzureSearch.Loader.Processes;
using CEAM.AzureSearch.Loader.Utils;
using static CEAM.AzureSearch.Loader.Utils.ConstantUtil;

namespace CEAM.AzureSearch.Loader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string prefix = "Cotizador -> ";
            try
            {
                DateTime startTime = DateTime.Now;
                MessageUtil.WriteWithDatetime(prefix + "Start time:");

                var cotizadorProcess = new CotizadorProcess();
                await cotizadorProcess.Start();
                
                DateTime endTime = DateTime.Now;
                MessageUtil.WriteWithDatetime(prefix + "End time:");

                TimeSpan ts = endTime - startTime;
                MessageUtil.WriteWithTimeSpan(prefix + "The process time was:", ts, DateTimeFormat.ProcessTimeShort);

                //string format = @"dd\:hh\:mm\:ss\.fffffff";
                //Console.WriteLine("The process time was: {0}", ts.ToString(format));
                //Console.WriteLine(@"The process time was: {0} Days, {1} Hours, {2} Minutes, {3} Seconds and {4} Milliseconds", 
                //    ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine(prefix + "Error: " + e.Message);
            }
        }
    }
}
