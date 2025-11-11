namespace AzureSearch.Models.Cotizador.Entities
{
    public class CotizadorCatalogueEntity
    {
        public long CatalogueId { get; set; }
        public string CatalogueName { get; set; }
        public string CatalogueStatus { get; set; }
        public long AgreementId { get; set; }
    }
}
