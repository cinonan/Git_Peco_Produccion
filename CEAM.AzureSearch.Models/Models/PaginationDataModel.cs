using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Models
{
    public class PaginationDataModel
    {        
        // The current page being displayed.
        public int CurrentPage { get; set; }

        // The total number of pages of results.
        public int PageCount { get; set; }

        // The left-most page number to display.
        public int LeftMostPage { get; set; }

        // The number of page numbers to display - which can be less than MaxPageRange towards the end of the results.
        public int PageRange { get; set; }

        // Used when page numbers, or next or prev buttons, have been selected.
        public string Paging { get; set; }
        public int Page { get; set; }

    }
}
