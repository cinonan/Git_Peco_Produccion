using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
//using System.Text.Json.Serialization;

namespace AzureSearch.Core.Models.Documents
{
    public class FeatureDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Name { get; set; }

        //[SearchableField(IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string[] Values { get; set; }
    }
}
