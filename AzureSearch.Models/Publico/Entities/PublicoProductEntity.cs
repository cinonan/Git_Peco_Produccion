using System;
using System.Collections.Generic;

namespace AzureSearch.Models.Publico.Entities
{
    public class PublicoProductEntity
    {
        public string AgreementId { get; set; } 
        public string AgreementName { get; set; }
        public string AgreementStatus { get; set; }
        public string CatalogueId { get; set; } 
        public string CatalogueName { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductPublishedDate { get; set; }
        public string ProductUpdatedDate { get; set; }
        public string ProductStatus { get; set; }
        public string ProductImage { get; set; }
        public string ProductFile { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<PublicoFeatureEntity> Features { get; set; }
        public string[] Departments { get; set; }
        // Campo en SQL (VARBINARY)
        public byte[] ProductVector { get; set; }
        // Propiedad calculada para acceder como float[]
        public float[] ProductArray
        {
            get
            {
                if (ProductVector == null) return Array.Empty<float>();
                float[] result = new float[ProductVector.Length / sizeof(float)];
                Buffer.BlockCopy(ProductVector, 0, result, 0, ProductVector.Length);
                return result;
            }
        }
    }
}
