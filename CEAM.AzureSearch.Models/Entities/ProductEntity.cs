namespace AzureSearch.Core.Models.Entities
{
    public class ProductEntity
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductPublishedDate { get; set; }
        public string ProductUpdatedDate { get; set; }
        public string ProductStatus { get; set; }
        public string ProductImage { get; set; }
        public string ProductFile { get; set; }
        public long CatalogueId { get; set; }
        public long CategoryId { get; set; }
    }
}
