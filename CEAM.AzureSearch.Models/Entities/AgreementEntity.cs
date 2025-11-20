using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearch.Core.Models.Entities
{
    public class AgreementEntity
    {
        public long AgreementId { get; set; }
        public string AgreementName { get; set; }
        public string AgreementStatus { get; set; }
    }
}
