using Azure.Search.Documents.Indexes;

namespace AzureSearch.Models.Cotizador.Documents
{
    public class CotizadorCategoryDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }
    }
}
