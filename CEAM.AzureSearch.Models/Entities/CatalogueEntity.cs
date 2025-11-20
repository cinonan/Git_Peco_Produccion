namespace AzureSearch.Core.Models.Entities
{
    public class CatalogueEntity
    {
        public long CatalogueId { get; set; }
        public string CatalogueName { get; set; }
        public string CatalogueStatus { get; set; }
        public long AgreementId { get; set; }
    }
}
