namespace AzureSearch.Core.Models.Entities
{
    public class ProductFeatureEntity
    {
        public long ProductId { get; set; }
		public long FeatureTypeId { get; set; }
		public string FeatureTypeName { get; set; }
		public string FeatureTypeCondition { get; set; }
		public string RequiredSubValue { get; set; }
		public long FeatureValueId { get; set; }
		public string FeatureValueName { get; set; }
		public string FeatureValueNameSub { get; set; }
	}
}
