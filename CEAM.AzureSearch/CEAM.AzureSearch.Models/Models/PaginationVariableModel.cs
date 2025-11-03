using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Models
{
    public class PaginationVariableModel
    {

        public static int ResultsPerPage
        {
            get
            {
                return 12;
            }
        }
        public static int MaxPageRange
        {
            get
            {
                return 10;
            }
        }

        public static int PageRangeDelta
        {
            get
            {
                return 2;
            }
        }
    }
}
