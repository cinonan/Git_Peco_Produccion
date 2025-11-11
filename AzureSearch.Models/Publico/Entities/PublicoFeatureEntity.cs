namespace AzureSearch.Models.Publico.Entities
{
    public class PublicoFeatureEntity
    {
        //public string Id { get; set; }
        public long ProductId { get; set; }
        public long? FeatureTypeId { get; set; }
        public string FeatureTypeName { get; set; }
        public string FeatureTypeRequiredSubValue { get; set; }
        public long? FeatureValueId { get; set; }
        public string FeatureValueName { get; set; }
        public string FeatureValueImg { get; set; }
        public string FeatureType { get; set; }
    }
}
