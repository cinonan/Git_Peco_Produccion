using Azure.Search.Documents.Indexes;
using System.Collections.Generic;

namespace AzureSearch.Core.Models.Documents
{
    public class FeatureTypeDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Text { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string IsRequiredSubValue { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public List<FeatureValueDocument> Values { get; set; }
    }
}
