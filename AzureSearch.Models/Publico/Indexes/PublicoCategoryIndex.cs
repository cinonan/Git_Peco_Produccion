using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using AzureSearch.Models.Publico.Documents;
using System.Collections.Generic;

namespace AzureSearch.Models.Publico.Indexes
{
    public class PublicoCategoryIndex
    {
        [SimpleField(IsKey = true, IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }

        [SimpleField]
        public List<PublicoFeatureDocument> Features { get; set; }

        // Campo para almacenar el hash del contenido del documento, usado para detectar cambios.
        [SimpleField(IsFilterable = true)]
        public string ContentHash { get; set; }
    }
}
