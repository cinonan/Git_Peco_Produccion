using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace AzureSearch.Models.Publico.Documents
{
    public class PublicoFeatureDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Name { get; set; }

        public string FeatureType { get; set; }

        //[SearchableField(IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string[] Values { get; set; }
    }
}
