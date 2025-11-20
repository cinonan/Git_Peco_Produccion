namespace AzureSearch.Models.Publico.Entities
{
    public class PublicoCatalogueEntity
    {
        public long CatalogueId { get; set; }
        public string CatalogueName { get; set; }
        public string CatalogueStatus { get; set; }
        public long AgreementId { get; set; }
    }
}
