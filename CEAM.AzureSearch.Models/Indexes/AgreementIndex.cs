using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using AzureSearch.Core.Models.Documents;
using System.Collections.Generic;

namespace AzureSearch.Core.Models.Indexes
{
    public class AgreementIndex
    {
        [SimpleField(IsKey = true, IsFilterable = true, IsFacetable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Status { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string SearchText { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<CatalogueDocument> Catalogues { get; set; }
    }
}
