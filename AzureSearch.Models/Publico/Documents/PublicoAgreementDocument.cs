using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace AzureSearch.Models.Publico.Documents
{
    public class PublicoAgreementDocument
    {
        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Status { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string SearchText { get; set; }

        // Campo para almacenar el hash del contenido del documento, usado para detectar cambios.
        [SimpleField(IsFilterable = true)]
        public string ContentHash { get; set; }
    }
}
