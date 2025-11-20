using AzureSearch.DataApp.Cotizador.Util;
using AzureSearch.Utils;
using System;
using System.Threading.Tasks;
using static AzureSearch.Utils.ConstantUtil;

namespace AzureSearch.DataApp.Cotizador
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string nombreArchivo = "Log.DataApp.Cotizador-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";            
            string prefix = "Cotizador -> ";
            try
            {
                FileLog.GuardarArchivo(nombreArchivo, "---Inicio Proceso---");
                DateTime startTime = DateTime.Now;
                MessageUtil.WriteWithDatetime(prefix + "Start time:");

                FileLog.GuardarArchivo(nombreArchivo, "Processes.CotizadorProcess Inicio");
                var cotizadorProcess = new Processes.CotizadorProcess();
                await cotizadorProcess.Start();
                FileLog.GuardarArchivo(nombreArchivo, "Processes.CotizadorProcess Fin");

                DateTime endTime = DateTime.Now;
                MessageUtil.WriteWithDatetime(prefix + "End time:");

                TimeSpan ts = endTime - startTime;
                MessageUtil.WriteWithTimeSpan(prefix + "The process time was:", ts, DateTimeFormat.ProcessTimeShort);
                FileLog.GuardarArchivo(nombreArchivo, "---Fin Proceso---");
            }
            catch (Exception e)
            {
                Console.WriteLine(prefix + "Error: " + e.Message);
                FileLog.GuardarArchivo(nombreArchivo, "Error: " + e.Message + " - " + e.Source);
            }
        }
    }
}
