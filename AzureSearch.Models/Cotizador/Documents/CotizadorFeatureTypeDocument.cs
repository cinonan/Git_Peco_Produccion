using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Models.Cotizador.Documents
{
    public class CotizadorFeatureTypeDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Name { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string TypeCondition { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string RequiredSubValue { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public List<CotizadorFeatureValueDocument> Values { get; set; }
    }
}
