using Azure.Search.Documents.Indexes;
using AzureSearch.Core.Models.Documents;
using System.Collections.Generic;

namespace AzureSearch.Core.Models.Indexes
{
    public class CategoryIndex
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        [SimpleField]
        public List<FeatureDocument> Features { get; set; }
    }
}
