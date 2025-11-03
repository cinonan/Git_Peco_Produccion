using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace AzureSearch.Models.Publico.Documents
{
    public class PublicoCatalogueDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        //public string SearchText { get; set; }

    }
}
