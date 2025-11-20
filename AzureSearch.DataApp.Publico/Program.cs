using AzureSearch.DataApp.Publico.Processes;
using AzureSearch.DataApp.Publico.Util;
using AzureSearch.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using static AzureSearch.Utils.ConstantUtil;

namespace AzureSearch.DataApp.Publico
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string nombreArchivo = "Log.DataApp.Publico-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            string prefix = "Público -> ";
            try
            {
                FileLog.GuardarArchivo(nombreArchivo, "---Inicio Proceso---");
                DateTime startTime = DateTime.Now;
                MessageUtil.WriteWithDatetime(prefix + "Start time:");

                IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

                var dataProcess = new PublicoDataProcess();
                var uploadProcess = new PublicoUploadProcess(configuration);

                FileLog.GuardarArchivo(nombreArchivo, "GetDocuments y LoadDocuments Inicio");
                var documents = await dataProcess.GetDocuments();

                var forceFullReload = bool.Parse(configuration["AppSettings:ForceFullReload"]);
                await uploadProcess.LoadDocuments(documents, forceFullReload);
                FileLog.GuardarArchivo(nombreArchivo, "GetDocuments y LoadDocuments Fin");

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
