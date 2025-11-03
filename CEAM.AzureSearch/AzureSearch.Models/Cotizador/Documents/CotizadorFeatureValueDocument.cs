using Azure.Search.Documents.Indexes;

namespace AzureSearch.Models.Cotizador.Documents
{
    public class CotizadorFeatureValueDocument
    {
		[SimpleField(IsFacetable = true, IsFilterable = true)]
		public string Id { get; set; }

		[SimpleField(IsFacetable = true, IsFilterable = true)]
		public string Name { get; set; }

		[SimpleField(IsFacetable = true, IsFilterable = true)]
		public string NameSub { get; set; }
	}
}
