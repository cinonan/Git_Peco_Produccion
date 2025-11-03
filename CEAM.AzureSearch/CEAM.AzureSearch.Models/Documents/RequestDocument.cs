using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Documents
{
    public class RequestDocument<T>
    {
        public List<T> value { get; set; }
    }
}
