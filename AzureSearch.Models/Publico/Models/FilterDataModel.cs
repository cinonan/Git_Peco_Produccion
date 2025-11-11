using System.Collections.Generic;

namespace AzureSearch.Models.Publico.Models
{

    public class FilterItemModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Count { get; set; }
        public string IsChecked { get; set; }
        public string Value { get; set; }
        public string FeatureType { get; set; }
    }

    public class FilterDataModel
    {
        public string Text { get; set; }
        public int Count { get; set; }
        public string IsChecked { get; set; }
        public string Value { get; set; }
        public int ShowFeature { get; set; }
        public string FeatureType { get; set; }
        public List<FilterItemModel> Items { get; set; }

        public FilterDataModel() {
            Items = new List<FilterItemModel>();
        }
    }
}
