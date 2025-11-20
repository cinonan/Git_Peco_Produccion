using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Entities
{
    public class FeatureEntity
    {
        public string Id { get; set; }
        public string ProductSheetId { get; set; }
        public string FeatureTypeId { get; set; }
        public string FeatureTypeName { get; set; }
        public string FeatureTypeRequiredSubValue { get; set; }
        public string FeatureValueId { get; set; }
        public string FeatureValueName { get; set; }
        public string FeatureValueImg { get; set; }
    }
}
