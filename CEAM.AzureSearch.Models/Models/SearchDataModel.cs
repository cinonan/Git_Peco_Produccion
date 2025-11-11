using System;
using System.Collections.Generic;
using System.Text;
using Azure.Search.Documents.Models;
using AzureSearch.Core.Models.Documents;

namespace AzureSearch.Core.Models.Models
{
    public class FilterBy
    {
        public string Agreement { get; set; }
        public string Catalogue { get; set; }
        public string Category { get; set; }
        public string Feature { get; set; }
        public string Department { get; set; }
    }

    public class SearchDataModel
    {
        public string ImageUrl { get; set; }
        public string FileUrl { get; set; }

        public string DownloadId { get; set; }
        public bool IsDownload { get; set; }
        public string From { get; set; }
        public string SearchText { get; set; }
        public string SearchTextPrevious { get; set; }
        public string Status { get; set; }

        public bool IsNewSearch { get; set; }

        public FilterBy ClientFilter { get; set; }
        public FilterBy ServerFilter { get; set; }
        public PaginationDataModel Pagination { get; set; }

        public List<FilterDataModel> AgreementFilter { get; set; }
        public FilterDataModel CatalogueFilter { get; set; }
        public FilterDataModel CategoryFilter { get; set; }
        public List<FilterDataModel> FeatureFilter { get; set; }
        public FilterDataModel DepartmentFilter { get; set; }

        // The list of results.
        public SearchResults<ProductSheetDocument> Results;
    }
}
