using System.Collections.Generic;

namespace AzureSearch.Models
{
    public class RequestModel<T>
    {
        public List<T> value { get; set; }
    }
}
