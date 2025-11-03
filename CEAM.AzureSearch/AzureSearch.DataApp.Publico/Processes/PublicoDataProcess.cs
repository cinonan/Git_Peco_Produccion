using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureSearch.Utils;
using AzureSearch.Models.Publico.Documents;
using AzureSearch.Models.Publico.Entities;
using AzureSearch.Models.Publico.Indexes;
using AzureSearch.DataApp.Publico.Repositories;

namespace AzureSearch.DataApp.Publico.Processes
{
    public class PublicoDataProcess
    {
        private int block = 1000;
        private PublicoProductRepository _publicoRepository { get; set; }
        private List<PublicoAgreementEntity> agreements { get; set; }
        private List<PublicoCatalogueEntity> catalogues { get; set; }
        private List<PublicoCategoryEntity> categories { get; set; }
        private List<PublicoProductEntity> products { get; set; }
        private List<PublicoFeatureEntity> features { get; set; }
        private List<PublicoDepartmentEntity> departments { get; set; }


        public PublicoDataProcess()
        {
            _publicoRepository = new PublicoProductRepository();

            agreements = new List<PublicoAgreementEntity>();
            catalogues = new List<PublicoCatalogueEntity>();
            categories = new List<PublicoCategoryEntity>();
            products = new List<PublicoProductEntity>();
            features = new List<PublicoFeatureEntity>();
            departments = new List<PublicoDepartmentEntity>();

        }

        private async Task LoadDataAsync()
        {
            MessageUtil.Write(false, "Load Agreements, Catalogues, Categories and Products");
            await _publicoRepository.LoadData();

            MessageUtil.Write(false, "Load Features");
            await _publicoRepository.LoadDataFeatures();

            MessageUtil.Write(false, "Load Departments");
            await _publicoRepository.LoadDataDepartments();
        }

        private async Task GetDataAsync()
        {
            MessageUtil.Write(false, "Get Agreements...");
            agreements = await _publicoRepository.GetData<PublicoAgreementEntity>("Agreement");
            MessageUtil.Write(true, "Get Agreements -> " + agreements.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Catalogues...");
            catalogues = await _publicoRepository.GetData<PublicoCatalogueEntity>("Catalogue");
            MessageUtil.Write(true, "Get Catalogues -> " + catalogues.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Categories");
            categories = await _publicoRepository.GetData<PublicoCategoryEntity>("Category");
            MessageUtil.Write(true, "Get Categories -> " + categories.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Products");
            products = await _publicoRepository.GetData<PublicoProductEntity>("Product");
            MessageUtil.Write(true, "Get Products -> " + products.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Features");
            features = await _publicoRepository.GetData<PublicoFeatureEntity>("Feature");
            MessageUtil.Write(true, "Get Features -> " + features.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Departments");
            departments = await _publicoRepository.GetData<PublicoDepartmentEntity>("Department");
            MessageUtil.Write(true, "Get Departments -> " + departments.Count.ToString() + " records");

            /*
            if (products != null)
            {
                foreach (var ps in products)
                {
                    ps.AgreementName = string.IsNullOrWhiteSpace(ps.AgreementName) ? "" : ps.AgreementName.Trim();
                    ps.AgreementStatus = string.IsNullOrWhiteSpace(ps.AgreementStatus) ? "" : ps.AgreementStatus.Trim().ToUpper();
                    ps.CatalogueName = string.IsNullOrWhiteSpace(ps.CatalogueName) ? "" : ps.CatalogueName.Trim();
                    ps.CategoryName = string.IsNullOrWhiteSpace(ps.CategoryName) ? "" : ps.CategoryName.Trim();
                    ps.ProductFile = string.IsNullOrWhiteSpace(ps.ProductFile) ? "" : ps.ProductFile.Trim();
                    ps.ProductImage = string.IsNullOrWhiteSpace(ps.ProductImage) ? "" : ps.ProductImage.Trim();
                    ps.ProductName = string.IsNullOrWhiteSpace(ps.ProductName) ? "" : ps.ProductName.Trim();
                    ps.ProductStatus = string.IsNullOrWhiteSpace(ps.ProductStatus) ? "" : ps.ProductStatus.Trim().ToUpper();
                }
            }
            */
        }

        public async Task<(
            List<PublicoAgreementIndex>,
            List<PublicoCatalogueIndex>,
            List<PublicoCategoryIndex>,
            List<PublicoProductIndex>)> GetDocuments(int? limit = null)
        {
            //await LoadDataAsync();
            await GetDataAsync();

            var documentList = new List<PublicoProductIndex>();

            var arrId = products.GroupBy(g => new { g.ProductId }).Select(x => x.Key.ProductId).ToArray();

            List<PublicoAgreementIndex> agreementIndexList = GetAgreementIndexList();
            List<PublicoCatalogueIndex> catalogueIndexList = GetCatalogueIndexList();

            MessageUtil.Write(false, "Get documents -> 0 of " + products.Count().ToString());

            bool condition = true;
            int min = block * -1;
            int index = 1;

            while (condition)
            {
                min += block;

                if ((min + block) >= arrId.Length)
                {
                    block = arrId.Length - min;
                    condition = false;
                }

                long[] elements = new long[block];
                Array.Copy(arrId, min, elements, 0, block);

                var list = products.Where(w => elements.Contains(w.ProductId)).ToList();
                var param = string.Join(",", elements.Select(x => x.ToString()).ToArray());

                var featureList = features.Where(w => elements.Contains(w.ProductId)).ToList();
                if (featureList != null && featureList.Any())
                {
                    var newIds = featureList.GroupBy(g => new { g.ProductId }).Select(s => s.Key.ProductId).ToArray();

                    list.Where(w => newIds.Contains(w.ProductId))
                        .Select(w => { w.Features = featureList.Where(f => f.ProductId == w.ProductId).ToList(); return w; })
                        .ToList();
                }

                var departmentList = new List<PublicoDepartmentEntity>();
                if (departments != null && departments.Any())
                {
                    departmentList = departments.Where(w => elements.Contains(w.ProductId)).ToList();
                }

                if (departmentList != null && departmentList.Any())
                {
                    var departments = departmentList.GroupBy(g => new { g.ProductId })
                                                            .Select(s => new
                                                            {
                                                                s.Key.ProductId,
                                                                Departments = s.OrderBy(o => o.UbigeoName).Select(d => d.UbigeoName).Distinct().ToArray()
                                                            }).ToList();

                    list.Select(s =>
                    {
                        s.Departments = departments.Where(w => w.ProductId == s.ProductId).Select(w => w.Departments).FirstOrDefault();
                        return s;
                    }).ToList();
                }

                list.Where(w => w.Departments == null || w.Departments.Length == 0)
                    .Select(s => { s.Departments = new string[] { "Sin Clasificar" }; return s; }).ToList();

                var documents = GetProductIndexList(list);
                documentList.AddRange(documents);
                MessageUtil.Write(true, String.Format("Get documents -> {0} of {1}", (min + block).ToString(), products.Count.ToString()));

                index++;
            }

            MessageUtil.Write(false, "Get Categories");
            var categoryList = GetCategoryIndexList(products);

            return (agreementIndexList, catalogueIndexList, categoryList, documentList);
        }

        private List<PublicoAgreementIndex> GetAgreementIndexList()
        {
            var list = agreements.Select(s => new PublicoAgreementIndex
            {
                Id = s.AgreementId.ToString(),
                Name = s.AgreementName,
                Status = s.AgreementStatus,
                SearchText = StringUtil.RemoveDiacritics_Publico(s.AgreementStatus) + StringUtil.Separator + StringUtil.RemoveDiacritics_Publico(s.AgreementName.Trim()),
                Catalogues = catalogues.Where(w => w.AgreementId == s.AgreementId).Select(w => new PublicoCatalogueDocument
                {
                    Id = w.CatalogueId.ToString(),
                    Name = w.CatalogueName
                }).ToList()
            }).ToList();

            return list;
        }

        private List<PublicoCatalogueIndex> GetCatalogueIndexList()
        {
            var list = catalogues.Select(s => new PublicoCatalogueIndex
            {
                Id = s.CatalogueId.ToString(),
                Name = s.CatalogueName,
                Categories = categories.Where(w => w.CatalogueId == s.CatalogueId)
                                                                .Select(w => new PublicoCategoryDocument
                                                                {
                                                                    Id = w.CategoryId.ToString(),
                                                                    Name = w.CategoryName
                                                                }).ToList()
            }).ToList();

            return list;
        }

        private List<PublicoProductIndex> GetProductIndexList(List<PublicoProductEntity> entities)
        {
            var documents = new List<PublicoProductIndex>();

            try
            {
                entities.ForEach(entity =>
                {
                    var featureTypeList = new List<PublicoFeatureTypeDocument> { };
                    var featureByTypeList = new List<PublicoFeatureDocument> { };

                    if (entity.Features != null && entity.Features.Any())
                    {
                        featureTypeList = entity.Features.GroupBy(g => new { g.FeatureTypeId, g.FeatureTypeName })
                                                  .Select(s => new PublicoFeatureTypeDocument
                                                  {
                                                      Id = s.Key.FeatureTypeId.ToString(),
                                                      Text = s.Key.FeatureTypeName,
                                                      IsRequiredSubValue = s.First().FeatureTypeRequiredSubValue,
                                                      Values = new List<PublicoFeatureValueDocument>()
                                                  }).ToList();

                        foreach (var item in featureTypeList)
                        {
                            item.Values = entity.Features.Where(w => w.FeatureTypeId == long.Parse(item.Id))
                                                         .Select(s => new PublicoFeatureValueDocument
                                                         {
                                                             Id = s.FeatureValueId.ToString(),
                                                             Text = s.FeatureValueName,
                                                             ValueImg = s.FeatureValueImg,
                                                             FeatureType = s.FeatureType
                                                         })
                                                         .ToList();
                        }

                        featureByTypeList = entity.Features.GroupBy(g => new { g.FeatureTypeId, g.FeatureTypeName })
                                                  .Select(s => new PublicoFeatureDocument
                                                  {
                                                      Id = s.Key.FeatureTypeId.ToString(),
                                                      Name = s.Key.FeatureTypeName
                                                  }).ToList();

                        foreach (var item in featureByTypeList)
                        {
                            item.Values = entity.Features.Where(w => w.FeatureTypeId == long.Parse(item.Id))
                                                         .Select(s => s.FeatureValueName).ToArray();
                        }

                        string[] features = entity.Features.Select(s => s.FeatureTypeName + StringUtil.Separator + s.FeatureValueName + StringUtil.Separator + s.FeatureType).ToArray();

                        var agreementEntity = agreements.Where(a => a.AgreementId == long.Parse(entity.AgreementId)).FirstOrDefault();
                        var agreement = new PublicoAgreementDocument
                        {
                            Id = entity.AgreementId,
                            Name = agreementEntity.AgreementName,
                            Status = agreementEntity.AgreementStatus,
                            SearchText = StringUtil.RemoveDiacritics_Publico(agreementEntity.AgreementStatus) + StringUtil.Separator + StringUtil.RemoveDiacritics_Publico(agreementEntity.AgreementName.Trim())
                        };

                        var catalogueEntity = catalogues.Where(a => a.AgreementId == long.Parse(entity.AgreementId))
                                                        .Where(a => a.CatalogueId == long.Parse(entity.CatalogueId)).FirstOrDefault();
                        var catalogue = new PublicoCatalogueDocument
                        {
                            Id = entity.CatalogueId,
                            Name = catalogueEntity.CatalogueName.Trim()
                        };

                        var categoryEntity = categories.Where(a => a.CatalogueId == long.Parse(entity.CatalogueId))
                                                       .Where(a => a.CategoryId == long.Parse(entity.CategoryId)).FirstOrDefault();
                        var category = new PublicoCategoryDocument
                        {
                            Id = entity.CategoryId,
                            Name = categoryEntity.CategoryName.Trim()
                        };

                        documents.Add(new PublicoProductIndex
                        {
                            Id = entity.ProductId.ToString(),
                            Name = entity.ProductName,
                            PublishedDate = entity.ProductPublishedDate, //entity.ProductSheetPublishedDate.HasValue ? entity.ProductSheetPublishedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                            UpdatedDate = entity.ProductUpdatedDate, //entity.ProductSheetUpdatedDate.HasValue ? entity.ProductSheetUpdatedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                            Image = entity.ProductImage,
                            File = entity.ProductFile,
                            Status = entity.ProductStatus,
                            Features = features,
                            FeatureTypeList = featureTypeList,
                            Departments = entity.Departments,
                            SearchText = entity.ProductName,//StringUtil.RemoveDiacritics(entity.ProductName) + " " + featureTypeList.Where(s => s.Text.ToUpper() == "NRO_PARTE" || s.Text.ToUpper() == "CODIGO_DE_IDENTIFICACION_UNICO").Select(s => s.Values.FirstOrDefault().Text).FirstOrDefault(),
                            ProductArray = entity.ProductArray,
                            Agreement = agreement,
                            Catalogue = catalogue,
                            Category = category
                        });
                    }
                    else
                    {
                        MessageUtil.Write(false, "  Error GetProductIndexList ProductId: {0} no tiene características", "" + entity.ProductId);
                    }
                });
            }
            catch (Exception e)
            {
                MessageUtil.Write(false, "  Error GetProductIndexList: {0}", e.Message.ToString());
            }


            return documents;
        }

        public List<PublicoCategoryIndex> GetCategoryIndexList(List<PublicoProductEntity> entities)
        {
            var list = entities.GroupBy(s => new { s.CategoryId })
                                       .Select(s => new
                                       {
                                           Id = s.Key.CategoryId,
                                           Name = s.First().CategoryName,
                                           Features = s.Select(a => a.Features).ToList()
                                       }).ToList();

            var categoryList = new List<PublicoCategoryIndex>();

            foreach (var item in list)
            {
                var featureList = new List<PublicoFeatureEntity>();
                foreach (var features in item.Features)
                {
                    if (features != null) featureList.AddRange(features);
                }

                try
                {
                    var categoryIndex = new PublicoCategoryIndex();
                    categoryIndex.Id = item.Id;
                    categoryIndex.Name = item.Name;
                    categoryIndex.Features = featureList
                                .GroupBy(t => new { t.FeatureTypeId })
                                .Select(t => new PublicoFeatureDocument
                                {
                                    Id = (t.Key.FeatureTypeId == null ? string.Empty : t.Key.FeatureTypeId.ToString()),
                                    Name = t.First().FeatureTypeName,
                                    FeatureType = t.First().FeatureType,
                                    Values = t.Select(u => (u.FeatureValueId == null ? string.Empty : u.FeatureValueId.ToString()) + "|" + u.FeatureValueName).Distinct().ToArray()
                                }).ToList();
                    categoryList.Add(categoryIndex);
                }
                catch (Exception ex)
                {
                    var detalle = ex.Message;
                }
            }
            return categoryList;
        }

    }
}
