using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;

namespace AzureSearch.WebApp.Publico.Utils.Config
{
    public sealed class ConfigLoader : IConfigLoader
    {
        private readonly JsonDocument _json;
        private readonly string _raw;

        public string ConfigPath { get; }
        public string ConfigVersion { get; } = "unknown";

        // Opciones SOLO para deserialización (OK aquí):
        private static readonly JsonSerializerOptions SerOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public ConfigLoader(IConfiguration cfg, IWebHostEnvironment env)
        {
            var rel = cfg["CanonicalConfig:Path"] ?? "Config/config_optimizado.json";
            ConfigPath = Path.Combine(env.ContentRootPath, rel);

            if (!File.Exists(ConfigPath))
                throw new FileNotFoundException($"No se encontró el archivo de configuración: {ConfigPath}");

            _raw = File.ReadAllText(ConfigPath);

            // Ahora: sin opciones (o usa JsonDocumentOptions si lo necesitas)
            _json = JsonDocument.Parse(_raw);

            if (_json.RootElement.TryGetProperty("config_version", out var v) &&
                v.ValueKind == JsonValueKind.String)
            {
                ConfigVersion = v.GetString() ?? "unknown";
            }
        }

        public string GetRaw() => _raw;

        public T Get<T>() =>
            JsonSerializer.Deserialize<T>(_json.RootElement.GetRawText(), SerOpts)
            ?? throw new InvalidOperationException("No se pudo deserializar el JSON al tipo solicitado.");

        public JsonDocument GetJsonDocument() =>
            // Reparsea sin opciones para entregar un documento independiente
            JsonDocument.Parse(_json.RootElement.GetRawText());
    }
}