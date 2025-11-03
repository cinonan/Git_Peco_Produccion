using Azure.Search.Documents.Indexes;
using System.Collections.Generic;

namespace AzureSearch.Models.Cotizador.Documents
{
    public class CotizadorCatalogueDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Name { get; set; }

        [SimpleField]
        public List<CotizadorCategoryDocument> Categories { get; set; }
    }
}
