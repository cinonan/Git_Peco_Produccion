using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Models
{

    public class FilterItemModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Count { get; set; }
        public string IsChecked { get; set; }
        public string Value { get; set; }
    }

    public class FilterDataModel
    {
        public string Text { get; set; }
        public int Count { get; set; }
        public string IsChecked { get; set; }
        public string Value { get; set; }
        public List<FilterItemModel> Items { get; set; }

        public FilterDataModel() {
            Items = new List<FilterItemModel>();
        }
    }
}
