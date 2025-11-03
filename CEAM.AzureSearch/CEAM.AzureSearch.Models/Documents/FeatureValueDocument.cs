using Azure.Search.Documents.Indexes;

namespace AzureSearch.Core.Models.Documents
{
    public class FeatureValueDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Text { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string ValueImg { get; set; }
    }
}
