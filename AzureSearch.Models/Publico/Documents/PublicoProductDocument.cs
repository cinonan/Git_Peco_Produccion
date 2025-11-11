using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Collections.Generic;

namespace AzureSearch.Models.Publico.Documents
{
    public class PublicoProductDocument
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        [SimpleField]
        public string Name { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        public string SearchText { get; set; }

        [SimpleField]
        public string PublishedDate { get; set; }

        [SimpleField]
        public string UpdatedDate { get; set; }

        [SimpleField]
        public string Image { get; set; }

        [SimpleField]
        public string File { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string Status { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Departments { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Features { get; set; }

        public PublicoAgreementDocument Agreement { get; set; }
        public PublicoCatalogueDocument Catalogue { get; set; }
        public PublicoCategoryDocument Category { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<PublicoFeatureTypeDocument> FeatureTypeList { get; set; }

        // 📌 Campo para embeddings (768 dimensiones)
        [VectorSearchField(
            VectorSearchDimensions = 768,
            VectorSearchProfileName = "my-vector-profile"
        )]
        public float[] ProductArray { get; set; }

        // Campo para almacenar el hash del contenido del documento, usado para detectar cambios.
        [SimpleField(IsFilterable = true)]
        public string ContentHash { get; set; }
    }
}
