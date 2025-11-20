using System;
using System.IO;

namespace AzureSearch.DataApp.Publico.Util
{
    public static class FileLog
    {
        public static void GuardarArchivo(string nombreArchivo, string mensaje)
        {
            // Nombre de la carpeta dentro del proyecto donde se guardarán los archivos
            string carpetaProyecto = "LogFile";

            // Ruta del archivo
            string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, carpetaProyecto, nombreArchivo);

            // Crear la carpeta si no existe
            string carpetaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, carpetaProyecto);
            if (!Directory.Exists(carpetaCompleta)) Directory.CreateDirectory(carpetaCompleta);

            string fechaHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // Verificar si el archivo ya existe
            if (File.Exists(rutaArchivo))
            {
                // Abrir el archivo existente y agregar mensajes al final
                using StreamWriter writer = File.AppendText(rutaArchivo);
                writer.WriteLine(fechaHora + " - " + mensaje);
            }
            else
            {
                // Crear un nuevo archivo y escribir mensajes en él
                using StreamWriter writer = new StreamWriter(rutaArchivo);
                writer.WriteLine(fechaHora + " - " + mensaje);
            }
        }
    }
}
