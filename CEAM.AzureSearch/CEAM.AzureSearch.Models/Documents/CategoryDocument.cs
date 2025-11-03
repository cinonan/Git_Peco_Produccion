using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Text.Json.Serialization;

namespace AzureSearch.Core.Models.Documents
{
    public class CategoryDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        //public string SearchText { get; set; }
    }
}
