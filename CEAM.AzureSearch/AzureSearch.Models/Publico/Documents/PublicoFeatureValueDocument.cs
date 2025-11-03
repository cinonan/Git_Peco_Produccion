using Azure.Search.Documents.Indexes;

namespace AzureSearch.Models.Publico.Documents
{
    public class PublicoFeatureValueDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Text { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string ValueImg { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string FeatureType { get; set; }
    }
}
