#nullable enable
using System.Text.Json;

namespace AzureSearch.WebApp.Publico.Utils.Config
{
    public interface IConfigLoader
    {
        string ConfigPath { get; }
        string ConfigVersion { get; }

        string GetRaw();
        T Get<T>();
        JsonDocument GetJsonDocument();
    }
}