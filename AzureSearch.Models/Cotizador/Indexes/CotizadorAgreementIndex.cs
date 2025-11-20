using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using AzureSearch.Models.Cotizador.Documents;
using System.Collections.Generic;

namespace AzureSearch.Models.Cotizador.Indexes
{
    public class CotizadorAgreementIndex
    {
        [SimpleField(IsKey = true, IsFilterable = true, IsFacetable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string Status { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string TextSearch { get; set; }

        [SimpleField]
        public List<CotizadorCatalogueDocument> Catalogues { get; set; }
    }
}
