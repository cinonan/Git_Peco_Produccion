using Azure.Search.Documents.Indexes;

namespace AzureSearch.Models.Cotizador.Documents
{
    public class CotizadorAgreementDocument
    {
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Id { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string Name { get; set; }

        //[SimpleField]
        //public string Status { get; set; }
    }
}
