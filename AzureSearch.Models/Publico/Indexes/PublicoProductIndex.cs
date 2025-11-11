using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Collections.Generic;
using AzureSearch.Models.Publico.Documents;

namespace AzureSearch.Models.Publico.Indexes
{
    public class PublicoProductIndex
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        [SimpleField]
        public string Name { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene)]
        public string SearchText { get; set; }

        // Campo vectorial: no searchable normal, se configurará en la definición del índice
        public float[] ProductArray { get; set; }

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

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Departments { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Features { get; set; }

        public PublicoAgreementDocument Agreement { get; set; }
        public PublicoCatalogueDocument Catalogue { get; set; }
        public PublicoCategoryDocument Category { get; set; }
        //public List<FeatureDocument> FeatureByTypeList { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<PublicoFeatureTypeDocument> FeatureTypeList { get; set; }

        // Campo para almacenar el hash del contenido del documento, usado para detectar cambios.
        [SimpleField(IsFilterable = true)]
        public string ContentHash { get; set; }
    }
}