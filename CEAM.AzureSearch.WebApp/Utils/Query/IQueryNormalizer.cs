namespace AzureSearch.WebApp.Publico.Utils.Query
{
    public interface IQueryNormalizer
    {
        /// Devuelve la consulta en formato canónico (minúsculas, sinónimos/unidades normalizados, etc.).
        string Normalize(string raw);
    }
}