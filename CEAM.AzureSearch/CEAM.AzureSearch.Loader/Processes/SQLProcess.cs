using System;
using System.Collections.Generic;
using System.Text;
using CEAM.AzureSearch.Loader.Data.Repositories;
using CEAM.AzureSearch.Models.Entities;
using CEAM.AzureSearch.Models.Documents;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using CEAM.AzureSearch.Loader.Helpers;
using CEAM.AzureSearch.Models.Indexes;

namespace CEAM.AzureSearch.Loader.Processes
{
    public class SQLProcess
    {
        private string path;
        private string indexName;
        private ProductSheetRepository repository;
        private int block = 1000;

        public SQLProcess()
        {
            repository = new ProductSheetRepository();
        }

        private void ConfigurationSetup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            indexName = configuration["AzureSearch:Load:IndexName"];
            path = configuration["AzureSearch:Load:Directory"];
        }

        public async Task GenerateJsonFilesAsync()
        {
            Console.WriteLine("Load temp data for departments -> FichaProducto...");
            await repository.LoadDepartments_01_FichaProducto();
            Console.WriteLine("Load temp data for departments -> ProductoOfertado...");
            await repository.LoadDepartments_02_ProductoOfertado();
            Console.WriteLine("Load temp data for departments -> CoberturaProveedor...");
            await repository.LoadDepartments_03_CoberturaProveedor();

            Console.WriteLine("Get Product Sheets...");
            var psList = await repository.GetProductSheetListAsync();

            Console.WriteLine("Total Product Sheets: " + psList.Count.ToString());
            var arrId = psList.GroupBy(g => new { g.ProductSheetId }).Select(x => x.Key.ProductSheetId).ToArray();

            bool condition = true;
            int min = block * -1;
            int index = 1;

            ConfigurationSetup();
            GetDirectory();

            Console.WriteLine("Get Departments for all productSheets...");
            var allDepartmentList = await repository.GetDepartmentListAsync("");

            Console.WriteLine("Get Departments for all productSheets...");
            while (condition)
            {
                min += block;

                if ((min + block) >= arrId.Length)
                {
                    block = arrId.Length - min;
                    condition = false;
                }

                string[] elements = new string[block];
                Array.Copy(arrId, min, elements, 0, block);

                var list = psList.Where(w => elements.Contains(w.ProductSheetId)).ToList();
                var param = String.Join(",", elements.Select(x => x.ToString()).ToArray());
                var featureList = await repository.GetFeatureListAsync(param);

                if (featureList != null && featureList.Any())
                {
                    var newIds = featureList.GroupBy(g => new { g.ProductSheetId }).Select(s => s.Key.ProductSheetId).ToArray();

                    list.Where(w => newIds.Contains(w.ProductSheetId))
                        .Select(w => { w.Features = featureList.Where(f => f.ProductSheetId == w.ProductSheetId).ToList(); return w; })
                        .ToList();
                }

                //var departmentList = new List<DepartmentEntity>(); //await repository.GetDepartmentListAsync(param);
                var departmentList = allDepartmentList.Where(w => elements.Contains(w.ProductSheetId)).ToList();

                if (departmentList != null && departmentList.Any())
                {
                    var departments = departmentList.GroupBy(g => new { g.ProductSheetId })
                                                            .Select(s => new { 
                                                                s.Key.ProductSheetId, 
                                                                Departments = s.OrderBy(o => o.DepartamentName).Select(d => d.DepartamentName).Distinct().ToArray()}).ToList();

                    list.Select(s =>
                    {
                        s.Departments = departments.Where(w => w.ProductSheetId == s.ProductSheetId).Select(w => w.Departments).FirstOrDefault();
                        return s;
                    }).ToList();
                }

                list.Where(w => w.Departments == null || w.Departments.Length == 0)
                    .Select(s => { s.Departments = new string[] { "Sin Clasificar" }; return s; }).ToList();

                var documents = GetDocuments(list);
                var documentJson = new RequestDocument<ProductSheetDocument>();
                documentJson.value = documents;
                var json = JsonSerializer.Serialize(documentJson);

                //var jsonList = new List<string>();
                //documents.ForEach(i => { jsonList.Add(JsonSerializer.Serialize(i)); });
                //var json = "{\"value\": [" + String.Join(",\r\n", jsonList.ToArray()) + "]}";

                var jsonFileName = indexName + "-" + index.ToString().PadLeft(4, '0') + ".json";
                path = Path.Combine(path, indexName);
                var filePath = Path.Combine(path, jsonFileName);
                File.WriteAllText(filePath, json);

                Console.WriteLine(String.Format(" -> file: {0} - records: {1}", jsonFileName, (min + block).ToString()));

                index++;
            }
        }

        public List<AgreementIndex> GetAgreements(List<ProductSheetEntity> productSheetList)
        {
            var list = productSheetList.GroupBy(s => new { s.AgreementId })
                                       .Select(s => new AgreementIndex
                                       {
                                           Id = s.Key.AgreementId,
                                           Name = s.First().AgreementName,
                                           Status = s.First().AgreementStatus,
                                           SearchText = StringHelper.RemoveDiacritics(s.First().AgreementStatus) + StringHelper.Separator + StringHelper.RemoveDiacritics(s.First().AgreementName.Trim()),
                                           Catalogues = s.GroupBy(s2 => new { s2.CatalogueId })
                                                         .Select(s2 => new CatalogueDocument
                                                         {
                                                             Id = s2.Key.CatalogueId,
                                                             Name = s2.First().CatalogueName
                                                         }).ToList()
                                       }).ToList();

            return list;
        }

        public List<CatalogueIndex> GetCatalogues(List<ProductSheetEntity> productSheetList)
        {
            var list = productSheetList.GroupBy(s => new { s.CatalogueId })
                                       .Select(s => new CatalogueIndex
                                       {
                                           Id = s.Key.CatalogueId,
                                           Name = s.First().CatalogueName,
                                           Categories = s.GroupBy(t => new { t.CategoryId })
                                                         .Select(t => new CategoryDocument
                                                         {
                                                             Id = t.Key.CategoryId,
                                                             Name = t.First().CategoryName
                                                         }).ToList()
                                       }).ToList();

            return list;
        }

        public List<CategoryIndex> GetCategories(List<ProductSheetEntity> productSheetList)
        {
            //var list = productSheetList.GroupBy(s => new { s.CategoryId })
            //                           .Select(s => new CategoryIndex
            //                           {
            //                               Id = s.Key.CategoryId,
            //                               Name = s.First().CategoryName,
            //                               Features = s.First().Features == null ? new List<FeatureDocument>() :
            //                                        s.Select(a => a.Features).ToList()
            //                                            .GroupBy(t => new { t.FeatureTypeId })
            //                                            .Select(t => new FeatureDocument
            //                                            {
            //                                                Id = t.Key.FeatureTypeId,
            //                                                Name = t.First().FeatureTypeName,
            //                                                Values = t.Select(u => u.FeatureValueId + "|" + u.FeatureValueName).ToArray()
            //                                            }).ToList()
            //                           }).ToList();

            var list = productSheetList.GroupBy(s => new { s.CategoryId })
                                       .Select(s => new 
                                       {
                                           Id = s.Key.CategoryId,
                                           Name = s.First().CategoryName,
                                           Features = s.Select(a => a.Features).ToList()
                                       }).ToList();

            var categoryList = new List<CategoryIndex>();

            foreach(var item in list)
            {
                var featureList = new List<FeatureEntity>();
                foreach(var features in item.Features)
                {
                    featureList.AddRange(features);
                }

                var categoryIndex = new CategoryIndex
                {
                    Id = item.Id,
                    Name = item.Name,
                    Features = featureList
                                .GroupBy(t => new { t.FeatureTypeId })
                                .Select(t => new FeatureDocument
                                {
                                    Id = t.Key.FeatureTypeId,
                                    Name = t.First().FeatureTypeName,
                                    Values = t.Select(u => u.FeatureValueId + "|" + u.FeatureValueName).Distinct().ToArray()
                                }).ToList()
                };

                categoryList.Add(categoryIndex);
            }

            return categoryList;
        }


        public async Task<(
            List<AgreementIndex>,
            List<CatalogueIndex>,
            List<CategoryIndex>,
            List<ProductSheetDocument>)> GetAllDocuments(int? limit = null)
        {
            //await LoadTempDataForDepartments();

            var documentList = new List<ProductSheetDocument>();

            Console.WriteLine("Get Product Sheets...");
            var psList = await repository.GetProductSheetListAsync(limit);

            Console.WriteLine("Total Product Sheets: " + psList.Count.ToString());
            var arrId = psList.GroupBy(g => new { g.ProductSheetId }).Select(x => x.Key.ProductSheetId).ToArray();

            Console.WriteLine("Get List of AgreementIndex...");
            var agreementList = GetAgreements(psList);

            Console.WriteLine("Get List of CatalogueIndex...");
            var catalogueList = GetCatalogues(psList);

            Console.WriteLine("Get Departments for all productSheets...");
            var allDepartmentList = await repository.GetDepartmentListAsync(string.Join(",",arrId));

            Console.WriteLine("Get documents from all productSheets...");

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

                string[] elements = new string[block];
                Array.Copy(arrId, min, elements, 0, block);

                var list = psList.Where(w => elements.Contains(w.ProductSheetId)).ToList();
                var param = String.Join(",", elements.Select(x => x.ToString()).ToArray());
                var featureList = await repository.GetFeatureListAsync(param);

                if (featureList != null && featureList.Any())
                {
                    var newIds = featureList.GroupBy(g => new { g.ProductSheetId }).Select(s => s.Key.ProductSheetId).ToArray();

                    list.Where(w => newIds.Contains(w.ProductSheetId))
                        .Select(w => { w.Features = featureList.Where(f => f.ProductSheetId == w.ProductSheetId).ToList(); return w; })
                        .ToList();
                }

                var departmentList = new List<DepartmentEntity>();
                if (allDepartmentList != null && allDepartmentList.Any())
                {
                    departmentList = allDepartmentList.Where(w => elements.Contains(w.ProductSheetId)).ToList();
                }

                if (departmentList != null && departmentList.Any())
                {
                    var departments = departmentList.GroupBy(g => new { g.ProductSheetId })
                                                            .Select(s => new {
                                                                s.Key.ProductSheetId,
                                                                Departments = s.OrderBy(o => o.DepartamentName).Select(d => d.DepartamentName).Distinct().ToArray()
                                                            }).ToList();

                    list.Select(s =>
                    {
                        s.Departments = departments.Where(w => w.ProductSheetId == s.ProductSheetId).Select(w => w.Departments).FirstOrDefault();
                        return s;
                    }).ToList();
                }

                list.Where(w => w.Departments == null || w.Departments.Length == 0)
                    .Select(s => { s.Departments = new string[] { "Sin Clasificar" }; return s; }).ToList();

                var documents = GetDocuments(list);
                documentList.AddRange(documents);
                Console.WriteLine(String.Format(" -> records: {0}", (min + block).ToString()));

                index++;
            }

            Console.WriteLine("Get List of CategoryIndex...");
            var categoryList = GetCategories(psList);

            return (agreementList, catalogueList, categoryList, documentList);
        }

        private async Task LoadTempDataForDepartments()
        {
            Console.WriteLine("Load temp data for departments -> FichaProducto...");
            await repository.LoadDepartments_01_FichaProducto();

            Console.WriteLine("Load temp data for departments -> ProductoOfertado...");
            await repository.LoadDepartments_02_ProductoOfertado();

            Console.WriteLine("Load temp data for departments -> CoberturaProveedor...");
            await repository.LoadDepartments_03_CoberturaProveedor();
        }

        private List<ProductSheetDocument> GetDocuments(List<ProductSheetEntity> entities)
        {
            var documents = new List<ProductSheetDocument>();

            entities.ForEach(entity =>
            {
                var featureTypeList = new List<FeatureTypeDocument> { };
                var featureByTypeList = new List<FeatureDocument> { };

                if (entity.Features != null & entity.Features.Any())
                {
                    featureTypeList = entity.Features.GroupBy(g => new { g.FeatureTypeId, g.FeatureTypeName })
                                              .Select(s => new FeatureTypeDocument
                                              {
                                                  Id = s.Key.FeatureTypeId,
                                                  Text = s.Key.FeatureTypeName,
                                                  IsRequiredSubValue = s.First().FeatureTypeRequiredSubValue,
                                                  Values = new List<FeatureValueDocument>()
                                              }).ToList();

                    foreach(var item in featureTypeList)
                    {
                        item.Values = entity.Features.Where(w => w.FeatureTypeId == item.Id)
                                                     .Select(s => new FeatureValueDocument { 
                                                         Id = s.FeatureValueId,
                                                         Text = s.FeatureValueName,
                                                         ValueImg = s.FeatureValueImg
                                                     })
                                                     .ToList();
                    }

                    featureByTypeList = entity.Features.GroupBy(g => new { g.FeatureTypeId, g.FeatureTypeName })
                                              .Select(s => new FeatureDocument { 
                                                  Id = s.Key.FeatureTypeId, 
                                                  Name = s.Key.FeatureTypeName }).ToList();

                    foreach (var item in featureByTypeList)
                    {
                        item.Values = entity.Features.Where(w => w.FeatureTypeId == item.Id)
                                                     .Select(s => s.FeatureValueName).ToArray();
                    }
                }

                string[] features = entity.Features.Select(s => s.FeatureTypeName + StringHelper.Separator + s.FeatureValueName).ToArray();

                documents.Add(new ProductSheetDocument
                {
                    Id = entity.ProductSheetId,
                    Name = entity.ProductSheetName,
                    PublishedDate = entity.ProductSheetPublishedDate.HasValue ? entity.ProductSheetPublishedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                    UpdatedDate = entity.ProductSheetUpdatedDate.HasValue ? entity.ProductSheetUpdatedDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                    Image = entity.ProductSheetImage,
                    File = entity.ProductSheetFile,
                    Status = entity.ProductSheetStatus,
                    Features = features,
                    FeatureTypeList = featureTypeList,                    
                    //FeatureByTypeList = featureByTypeList,                       
                    Departments = entity.Departments,

                    //SearchText = StringExtensions.SinTildes(entity.ProductSheetName),
                    SearchText = StringHelper.RemoveDiacritics(entity.ProductSheetName),

                    Agreement = new AgreementDocument
                    {
                        Id = entity.AgreementId,
                        Name = entity.AgreementName.Trim(),
                        Status = entity.AgreementStatus,
                        SearchText = StringHelper.RemoveDiacritics(entity.AgreementStatus) + StringHelper.Separator + StringHelper.RemoveDiacritics(entity.AgreementName.Trim())
                    },

                    Catalogue = new CatalogueDocument
                    {
                        Id = entity.CatalogueId,
                        Name = entity.CatalogueName.Trim()
                        //SearchText = StringHelper.RemoveDiacritics(entity.CatalogueName.Trim())
                    },

                    Category = new CategoryDocument
                    {
                        Id = entity.CategoryId,
                        Name = entity.CategoryName.Trim()
                        //SearchText = StringHelper.RemoveDiacritics(entity.CategoryName.Trim())
                    }
                });
            });

            return documents;
        }

        private void GetDirectory()
        {
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles()) { file.Delete(); }
        }
    }
}
