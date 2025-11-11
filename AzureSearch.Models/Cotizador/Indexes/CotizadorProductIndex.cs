using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using AzureSearch.Models.Cotizador.Documents;
using System.Collections.Generic;

namespace AzureSearch.Models.Cotizador.Indexes
{
    public class CotizadorProductIndex
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; }

        [SimpleField]
        public string Name { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        public string TextSearch { get; set; }

        [SimpleField]
        public string PublishedDate { get; set; }

        [SimpleField]
        public string UpdatedDate { get; set; }

        [SimpleField]
        public string ImageURL { get; set; }

        [SimpleField]
        public string FileURL { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string Status { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] FeatureFilters { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] FeatureIdFilters { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<CotizadorFeatureTypeDocument> Features { get; set; }
        public CotizadorAgreementDocument Agreement { get; set; }
        public CotizadorCatalogueDocument Catalogue { get; set; }
        public CotizadorCategoryDocument Category { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Departments { get; set; }
        [SimpleField(IsFilterable = true, IsFacetable = true, IsSortable=true)]
        public double PrecioUnitario { get; set; }
        [SimpleField(IsFilterable = true, IsFacetable = true, IsSortable = true)]
        public int CantidadTransacciones { get; set; }
    }
}
