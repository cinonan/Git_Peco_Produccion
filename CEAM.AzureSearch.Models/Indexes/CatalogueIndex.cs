using Azure.Search.Documents.Indexes;
using AzureSearch.Core.Models.Documents;
using System.Collections.Generic;

namespace AzureSearch.Core.Models.Indexes
{
    public class CatalogueIndex
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<CategoryDocument> Categories { get; set; }
    }
}
