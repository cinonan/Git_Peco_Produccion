using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AzureSearch.DataApp.Publico.Util
{
    public static class HashingUtil
    {
        /// <summary>
        /// Calcula un hash SHA256 para el contenido de un objeto, excluyendo recursivamente cualquier propiedad llamada 'ContentHash'.
        /// </summary>
        public static string CalculateContentHash<T>(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            var jsonNode = JsonSerializer.SerializeToNode(obj);
            if (jsonNode == null)
            {
                return string.Empty;
            }

            // Eliminar recursivamente todas las instancias de 'ContentHash'.
            RemovePropertyRecursively(jsonNode, "ContentHash");

            string jsonString = jsonNode.ToJsonString();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Recorre un JsonNode y elimina todas las propiedades que coincidan con el nombre especificado.
        /// </summary>
        private static void RemovePropertyRecursively(JsonNode node, string propertyName)
        {
            if (node is JsonObject jsonObject)
            {
                // Eliminar la propiedad si existe en el objeto actual.
                if (jsonObject.ContainsKey(propertyName))
                {
                    jsonObject.Remove(propertyName);
                }

                // Recorrer las propiedades restantes para continuar la búsqueda recursiva.
                foreach (var property in jsonObject.ToList())
                {
                    if (property.Value != null)
                    {
                        RemovePropertyRecursively(property.Value, propertyName);
                    }
                }
            }
            else if (node is JsonArray jsonArray)
            {
                // Si es un array, recorrer cada elemento y aplicar la lógica recursiva.
                foreach (var item in jsonArray)
                {
                    if (item != null)
                    {
                        RemovePropertyRecursively(item, propertyName);
                    }
                }
            }
        }
    }
}
