using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureSearch.Core.Models.Documents
{
    public class ProductSheetDocument
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

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Departments { get; set; }

        //[SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EsLucene, IsFilterable = true, IsFacetable = true)]
        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string[] Features { get; set; }

        public AgreementDocument Agreement { get; set; }
        public CatalogueDocument Catalogue { get; set; }
        public CategoryDocument Category { get; set; }
        //public List<FeatureDocument> FeatureByTypeList { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<FeatureTypeDocument> FeatureTypeList { get; set; }
    }
}
