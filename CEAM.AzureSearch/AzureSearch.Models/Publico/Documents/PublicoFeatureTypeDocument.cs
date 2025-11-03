using Azure.Search.Documents.Indexes;
using System.Collections.Generic;

namespace AzureSearch.Models.Publico.Documents
{
    public class PublicoFeatureTypeDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Text { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string IsRequiredSubValue { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public List<PublicoFeatureValueDocument> Values { get; set; }
    }
}
