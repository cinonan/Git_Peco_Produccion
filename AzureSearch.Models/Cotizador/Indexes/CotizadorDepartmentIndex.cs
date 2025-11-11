using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace AzureSearch.Models.Cotizador.Indexes
{
    public class CotizadorDepartmentIndex
    {
        [SimpleField(IsKey = true, IsFilterable = true, IsFacetable = true)]
        public string UbigeoId { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string UbigeoName { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public string ID_Ubigeo { get; set; }
    }
}
