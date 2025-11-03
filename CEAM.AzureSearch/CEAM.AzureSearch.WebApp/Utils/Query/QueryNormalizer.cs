#nullable enable
using AzureSearch.WebApp.Publico.Utils.Config;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AzureSearch.WebApp.Publico.Utils.Query
{
    /// Normalizador simple basado en tu JSON compartido (sin tocar el “query: ” del servicio de embeddings).
    public sealed class QueryNormalizer : IQueryNormalizer
    {
        private readonly JsonDocument _cfg;

        public QueryNormalizer(IConfigLoader configLoader)
        {
            _cfg = configLoader.GetJsonDocument();
        }

        public string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            // --- 0) normalización básica ---
            var s = raw.Trim().ToLowerInvariant();
            s = Regex.Replace(s, @"\s+", " ");

            // --- 1) separar decimal coma→punto (15,6 -> 15.6) ---
            s = Regex.Replace(s, @"(\d+),(\d+)", "$1.$2");

            // ----2) quitar conectores (ANTES de sinónimos)
            if (TryGetStopwords(out var pattern)) s = RemoveStopwords(s, pattern);

            // --- 3) sinónimos: category / featuretype / values ---
            s = ApplySynonyms(s, "synonyms", "category");
            s = ApplySynonyms(s, "synonyms", "featuretype");
            s = ApplySynonyms(s, "synonyms", "values");

            // --- 4) unidades típicas (memoria/almacenamiento) ---
            s = ApplyUnitAliases(s, new[] { "units", "memory", "aliases" }, "gb");
            // storage: primero TB, luego GB para evitar sobre-reemplazos
            s = ApplyUnitAliases(s, new[] { "units", "storage", "aliases", "tb" }, "tb");
            s = ApplyUnitAliases(s, new[] { "units", "storage", "aliases", "gb" }, "gb");

            // Mantén “con/sin/no” tal cual (no se eliminan); ya vienen en el texto.

            // --- 5) colapsa espacios otra vez y retorna ---
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        // ---- helpers ----
        private string ApplySynonyms(string s, string root, string node)
        {
            if (!_cfg.RootElement.TryGetProperty(root, out var rootEl)) return s;
            if (!rootEl.TryGetProperty(node, out var nodeEl)) return s;

            foreach (var map in nodeEl.EnumerateObject())
            {
                var canonical = map.Name;
                var aliases = map.Value.EnumerateArray()
                    .Select(a => a.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .OrderByDescending(x => x!.Length)           // primero los largos
                    .Select(x => Regex.Escape(x!));

                var alternation = string.Join("|", aliases);
                if (string.IsNullOrEmpty(alternation)) continue;

                var canonEsc = Regex.Escape(canonical);
                // no reescribir si ya está pegado al canónico a izq/der
                var pattern = $@"(?<!{canonEsc}\s)\b(?:{alternation})\b(?!\s{canonEsc})";

                s = Regex.Replace(s, pattern, canonical, RegexOptions.IgnoreCase);
            }
            return s;
        }

        private string ApplyUnitAliases(string s, string[] path, string canonical)
        {
            // path ejemplo: ["units","memory","aliases"] -> objeto de arrays por unidad
            // o ["units","storage","aliases","tb"] -> array puntual
            JsonElement node = _cfg.RootElement;
            foreach (var key in path)
            {
                if (!node.TryGetProperty(key, out node)) return s;
            }

            if (node.ValueKind == JsonValueKind.Object)
            {
                foreach (var unitKvp in node.EnumerateObject())
                {
                    foreach (var alias in unitKvp.Value.EnumerateArray())
                    {
                        var a = Regex.Escape(alias.GetString() ?? "");
                        if (string.IsNullOrWhiteSpace(a)) continue;
                        s = Regex.Replace(s, $@"\b(\d+)\s*{a}\b", $"$1 {unitKvp.Name}", RegexOptions.IgnoreCase);
                    }
                }
            }
            else if (node.ValueKind == JsonValueKind.Array)
            {
                foreach (var alias in node.EnumerateArray())
                {
                    var a = Regex.Escape(alias.GetString() ?? "");
                    if (string.IsNullOrWhiteSpace(a)) continue;
                    s = Regex.Replace(s, $@"\b(\d+)\s*{a}\b", $"$1 {canonical}", RegexOptions.IgnoreCase);
                }
            }

            // normaliza formatos pegados: "16gb" -> "16 gb"
            s = Regex.Replace(s, $@"\b(\d+)(?={Regex.Escape(canonical)})", "$1 ", RegexOptions.IgnoreCase);
            return s;
        }

        private bool TryGetStopwords(out string pattern)
        {
            pattern = "";
            if (!_cfg.RootElement.TryGetProperty("text_normalization", out var tn)) return false;
            if (tn.TryGetProperty("remove_stopwords_in_query", out var on) && on.GetBoolean()
                && tn.TryGetProperty("stopwords_query", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                var words = arr.EnumerateArray()
                    .Select(e => e.GetString())
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Select(w => Regex.Escape(w!));
                var alt = string.Join("|", words);
                if (!string.IsNullOrEmpty(alt)) { pattern = $@"\b(?:{alt})\b"; return true; }
            }
            return false;
        }

        private string RemoveStopwords(string s, string pattern)
        {
            s = Regex.Replace(s, pattern, " ", RegexOptions.IgnoreCase);
            return Regex.Replace(s, @"\s+", " ").Trim();
        }
    }
}