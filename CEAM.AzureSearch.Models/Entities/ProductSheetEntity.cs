using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Entities
{
    public class ProductSheetEntity
    {
        public string AgreementId { get; set; } 
        public string AgreementName { get; set; }
        public string AgreementStatus { get; set; }
        public string CatalogueId { get; set; } 
        public string CatalogueName { get; set; }
        public string ProductSheetId { get; set; }
        public string ProductSheetName { get; set; }
        public DateTime? ProductSheetPublishedDate { get; set; }
        public DateTime? ProductSheetUpdatedDate { get; set; }
        public string ProductSheetStatus { get; set; }
        public string ProductSheetImage { get; set; }
        public string ProductSheetFile { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<FeatureEntity> Features { get; set; }
        public string[] Departments { get; set; }
    }
}
