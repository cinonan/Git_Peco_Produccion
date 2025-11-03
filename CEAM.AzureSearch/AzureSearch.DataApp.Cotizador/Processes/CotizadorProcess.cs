using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AzureSearch.Models;
using AzureSearch.Models.Cotizador.Documents;
using AzureSearch.Models.Cotizador.Entities;
using AzureSearch.Models.Cotizador.Indexes;
using AzureSearch.Utils;
using AzureSearch.DataApp.Cotizador.Repositories;
using AzureSearch.Models.Publico.Entities;
using System.Xml.Linq;
using static Dapper.SqlMapper;

namespace AzureSearch.DataApp.Cotizador.Processes
{
    public class CotizadorProcess
    {
        private CotizadorRepository _cotizadorRepository { get; set; }
        private List<CotizadorAgreementEntity> agreements { get; set; }
        private List<CotizadorCatalogueEntity> catalogues { get; set; }
        private List<CotizadorCategoryEntity> categories { get; set; }
        private List<CotizadorProductFeatureEntity> productfeatures { get; set; }
        private List<CotizadorProductEntity> products { get; set; }
        private List<CotizadorDepartmentEntity> departments { get; set; }
        private List<CotizadorDepartmentFilterEntity> departmentsFilter { get; set; }
        private List<CotizadorProductIndex> productIndexList { get; set; }
        private List<CotizadorAgreementIndex> agreementIndexList { get; set; }
        private List<CotizadorDepartmentIndex> departmentIndexList { get; set; }
        private string cotizadorAgreementIndex { get; set; }
        private string cotizadorProductIndex { get; set; }
        private string cotizadorDepartmentIndex { get; set; }
        private string searchServiceName { get; set; }
        private string adminKey { get; set; }
        private SearchIndexClient indexClient;

        private void Setup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            searchServiceName = configuration["AzureSearch:Load:ServiceName"];
            adminKey = configuration["AzureSearch:Load:AdminKey"];
            cotizadorAgreementIndex = configuration["AzureSearch:Load:CotizadorAgreementIndex"];
            cotizadorProductIndex = configuration["AzureSearch:Load:CotizadorProductIndex"];
            cotizadorDepartmentIndex = configuration["AzureSearch:Load:CotizadorDepartmentIndex"];
            indexClient = new SearchIndexClient(new Uri("https://" + searchServiceName + ".search.windows.net"), new AzureKeyCredential(adminKey));
        }

        private async Task DeleteIndexIfExistsAsync(string indexName)
        {
            MessageUtil.Write(false, "Azure Search: Delete index -> " + indexName + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));

            try
            {
                await indexClient.DeleteIndexAsync(indexName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task CreateDepartmentIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + cotizadorDepartmentIndex + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            try
            {
                FieldBuilder builder = new();
                var definition = new SearchIndex(cotizadorDepartmentIndex, builder.Build(typeof(CotizadorDepartmentIndex)));
                await indexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task CreateProductIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + cotizadorProductIndex + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            try
            {
                FieldBuilder builder = new();
                var definition = new SearchIndex(cotizadorProductIndex, builder.Build(typeof(CotizadorProductIndex)));
                await indexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        private async Task CreateAgreementIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + cotizadorAgreementIndex + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));

            try
            {
                FieldBuilder builder = new();
                var definition = new SearchIndex(cotizadorAgreementIndex, builder.Build(typeof(CotizadorAgreementIndex)));
                await indexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        private void SendAgreements()
        {
            MessageUtil.Write(false, "Azure Search: Agreements -> 0 records updated" + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));

            try
            {
                Uri ServiceUri = new("https://" + searchServiceName + ".search.windows.net");
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("api-key", adminKey);
                Uri uri = new(ServiceUri, "/indexes/" + cotizadorAgreementIndex + "/docs/index");

                var documentJson = new RequestModel<CotizadorAgreementIndex> { value = agreementIndexList };
                var json = JsonSerializer.Serialize(documentJson);
                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                response.EnsureSuccessStatusCode();
                MessageUtil.Write(true, string.Format("Azure Search: Agreements -> {0} records updated" + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"), agreementIndexList.Count.ToString()));
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, string.Format("  Error: {0}", ex.Message.ToString()));
            }
        }
        private void SendProducts()
        {
            MessageUtil.Write(false, "Azure Search: Products -> 0 records updated" + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));

            try
            {
                Uri ServiceUri = new Uri("https://" + searchServiceName + ".search.windows.net");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", adminKey);
                Uri uri = new Uri(ServiceUri, "/indexes/" + cotizadorProductIndex + "/docs/index");

                bool condition = true;
                int block = 1000;
                int min = block * -1;

                while (condition)
                {
                    min += block;

                    if ((min + block) >= productIndexList.Count)
                    {
                        block = productIndexList.Count - min;
                        condition = false;
                    }

                    var documentJson = new RequestModel<CotizadorProductIndex> { value = productIndexList.GetRange(min, block) };
                    var json = JsonSerializer.Serialize(documentJson);
                    var reintento = 0;
                    while (reintento < 3)
                    {
                        try
                        {
                            HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                            response.EnsureSuccessStatusCode();
                            MessageUtil.Write(true, string.Format("Azure Search: Products -> {0} of {1} records updated" + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"), (min + block).ToString(), productIndexList.Count.ToString()));
                            reintento = 3;
                        }
                        catch (Exception ex)
                        {
                            reintento++;
                            MessageUtil.Write(false, string.Format("  Error SendProducts.SendSearchRequest..se hara reintento{0}: {1}", reintento, ex.Message.ToString() + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, string.Format("  Error: {0}", ex.Message.ToString() + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss")));
            }
        }

        private void SendDepartments()
        {
            MessageUtil.Write(false, "Azure Search: Departments -> 0 records updated" + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));

            try
            {
                Uri ServiceUri = new("https://" + searchServiceName + ".search.windows.net");
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("api-key", adminKey);
                Uri uri = new(ServiceUri, "/indexes/" + cotizadorDepartmentIndex + "/docs/index");

                var documentJson = new RequestModel<CotizadorDepartmentIndex> { value = departmentIndexList };
                var json = JsonSerializer.Serialize(documentJson);
                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                response.EnsureSuccessStatusCode();
                MessageUtil.Write(true, string.Format("Azure Search: Departments -> {0} records updated" + " " + DateTime.Now.ToString("dd/MM/HH:mm:ss"), departmentIndexList.Count.ToString()));
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, string.Format("  Error: {0}", ex.Message.ToString()));
            }
        }

        public CotizadorProcess()
        {
            Setup();
            _cotizadorRepository = new CotizadorRepository();
            agreements = new List<CotizadorAgreementEntity>();
            catalogues = new List<CotizadorCatalogueEntity>();
            categories = new List<CotizadorCategoryEntity>();
            products = new List<CotizadorProductEntity>();
            productfeatures = new List<CotizadorProductFeatureEntity>();
            agreementIndexList = new List<CotizadorAgreementIndex>();
            productIndexList = new List<CotizadorProductIndex>();
            departmentIndexList = new List<CotizadorDepartmentIndex>();
            departments = new List<CotizadorDepartmentEntity>();
            departmentsFilter = new List<CotizadorDepartmentFilterEntity>();
        }

        private async Task LoadDataAsync()
        {
            MessageUtil.Write(false, "Load Agreements, Catalogues and Categories " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            await _cotizadorRepository.LoadData();

            MessageUtil.Write(false, "Load Products " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            await _cotizadorRepository.LoadProducts();

            MessageUtil.Write(false, "Load Product Filters " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            await _cotizadorRepository.LoadProductFilters();

            MessageUtil.Write(false, "Load Departments " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            await _cotizadorRepository.LoadDataDepartments();
        }

        private async Task GetDataAsync()
        {
            MessageUtil.Write(false, "Get Agreements " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            agreements = await _cotizadorRepository.GetData<CotizadorAgreementEntity>("Agreement");
            MessageUtil.Write(true, "Get Agreements -> " + agreements.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Catalogues " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            catalogues = await _cotizadorRepository.GetData<CotizadorCatalogueEntity>("Catalogue");
            MessageUtil.Write(true, "Get Catalogues -> " + catalogues.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Categories " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            categories = await _cotizadorRepository.GetData<CotizadorCategoryEntity>("Category");
            MessageUtil.Write(true, "Get Categories -> " + products.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Products " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            products = await _cotizadorRepository.GetData<CotizadorProductEntity>("Product");
            MessageUtil.Write(true, "Get Products -> " + products.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Departments " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            departments = await _cotizadorRepository.GetData<CotizadorDepartmentEntity>("Department");
            MessageUtil.Write(true, "Get Departments -> " + departments.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Departments " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            departmentsFilter = await _cotizadorRepository.GetData<CotizadorDepartmentFilterEntity>("DepartmentFilters");
            MessageUtil.Write(true, "Get Departments Filters-> " + departments.Count.ToString() + " records");

            MessageUtil.Write(false, "Get Product Features " + DateTime.Now.ToString("dd/MM/HH:mm:ss"));
            productfeatures = await _cotizadorRepository.GetData<CotizadorProductFeatureEntity>("ProductFeature");
            MessageUtil.Write(true, "Get Product Features -> " + productfeatures.Count.ToString() + " records");
        }

        private void GetDocuments()
        {
            foreach (var departament in departmentsFilter)
            {
                var index = new CotizadorDepartmentIndex
                {
                    UbigeoId = departament.UbigeoId.ToString(),
                    UbigeoName = departament.UbigeoName.Trim(),
                };
                departmentIndexList.Add(index);
            }

            foreach (var agreement in agreements)
            {
                var newCatalogueList = catalogues.Where(w => w.AgreementId == agreement.AgreementId)
                                                 .Select(w => new CotizadorCatalogueDocument
                                                 {
                                                     Id = w.CatalogueId.ToString(),
                                                     Name = w.CatalogueName
                                                 }).ToList();

                foreach (var catalogue in newCatalogueList)
                {
                    var newCategoryList = categories.Where(w => w.CatalogueId == long.Parse(catalogue.Id))
                                                    .Select(w => new CotizadorCategoryDocument
                                                    {
                                                        Id = w.CategoryId.ToString(),
                                                        Name = w.CategoryName
                                                    }).ToList();
                    catalogue.Categories = newCategoryList;
                }

                var index = new CotizadorAgreementIndex
                {
                    Id = agreement.AgreementId.ToString(),
                    Name = agreement.AgreementName.Trim(),
                    Status = agreement.AgreementStatus.Trim(),
                    TextSearch = StringUtil.RemoveDiacritics_Estimador(agreement.AgreementName),
                    Catalogues = newCatalogueList
                };

                agreementIndexList.Add(index);
            }

            var featureList = productfeatures.GroupBy(g => new { g.ProductId })
                                             .Select(g => new
                                             {
                                                 ProductId = g.Key.ProductId,
                                                 Types = g.GroupBy(h => new { h.FeatureTypeId })
                                                          .Select(h => new CotizadorFeatureTypeDocument
                                                          {
                                                              Id = h.Key.FeatureTypeId.ToString(),
                                                              Name = h.First().FeatureTypeName,
                                                              TypeCondition = h.First().FeatureTypeCondition,
                                                              RequiredSubValue = h.First().RequiredSubValue,
                                                              Values = h.Select(i => new CotizadorFeatureValueDocument
                                                              {
                                                                  Id = i.FeatureValueId.ToString(),
                                                                  Name = i.FeatureValueName,
                                                                  NameSub = i.FeatureValueNameSub
                                                              }).ToList()
                                                          }).ToList(),
                                                 Filters = g.Select(s =>
                                                    s.FeatureTypeId.ToString() + StringUtil.Separator +
                                                    s.FeatureTypeName + StringUtil.Separator +
                                                    s.FeatureValueId.ToString() + StringUtil.Separator +
                                                    s.FeatureValueName.ToString() + StringUtil.Separator + s.FeatureTypeCondition
                                                    ).ToArray(),
                                                 FilterIds = g.Select(s =>
                                                    s.FeatureTypeId.ToString() + StringUtil.Separator +
                                                    s.FeatureValueId.ToString()
                                                    //s.FeatureValueId.ToString() + StringUtil.Separator +
                                                    //s.FeatureTypeCondition.ToString()
                                                    ).ToArray()
                                             }).ToList();

            foreach (var product in products)
            {
                var catalogue = catalogues.Where(w => w.CatalogueId == product.CatalogueId)
                                          .Select(w => new CotizadorCatalogueDocument
                                          {
                                              Id = w.CatalogueId.ToString(),
                                              Name = w.CatalogueName.Trim(),
                                              Categories = new List<CotizadorCategoryDocument>()
                                          }).FirstOrDefault();

                var category = categories.Where(w => w.CatalogueId == product.CatalogueId)
                                         .Where(w => w.CategoryId == product.CategoryId)
                                         .Select(w => new CotizadorCategoryDocument
                                         {
                                             Id = w.CategoryId.ToString(),
                                             Name = w.CategoryName.Trim()
                                         }).FirstOrDefault();

                var agreementId = catalogues.Where(w => w.CatalogueId == product.CatalogueId).Select(w => w.AgreementId).FirstOrDefault();

                var agreement = agreements.Where(w => w.AgreementId == agreementId)
                                          .Select(w => new CotizadorAgreementDocument
                                          {
                                              Id = w.AgreementId.ToString(),
                                              Name = w.AgreementName.Trim()
                                          }).FirstOrDefault();

                var productDetail = featureList.Where(w => w.ProductId == product.ProductId).FirstOrDefault();
                var departmentList = departments.Where(w => w.ProductId == product.ProductId).Select(w => w.UbigeoId).Distinct().ToArray();

                if (productDetail != null)
                {
                    var index = new CotizadorProductIndex
                    {
                        Id = product.ProductId.ToString(),
                        Name = product.ProductName.Trim(),
                        TextSearch = StringUtil.RemoveDiacritics_Estimador(product.ProductName) + " " + featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.Types).FirstOrDefault().Where(w => w.Name == "CODIGO DE IDENTIFICACION UNICO" || w.Name == "NRO PARTE").Select(w => w.Values.FirstOrDefault().Name).FirstOrDefault(),
                        PublishedDate = product.ProductPublishedDate,
                        UpdatedDate = product.ProductUpdatedDate,
                        ImageURL = product.ProductImage,
                        FileURL = product.ProductFile,
                        PrecioUnitario = product.PrecioUnitario,
                        CantidadTransacciones = product.CantidadTransacciones,
                        Status = product.ProductStatus,
                        Departments = departmentList,
                        FeatureFilters = featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.Filters).FirstOrDefault(),
                        Features = featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.Types).FirstOrDefault(),
                        FeatureIdFilters = featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.FilterIds).FirstOrDefault(),
                        Agreement = agreement,
                        Catalogue = catalogue,
                        Category = category
                    };
                    productIndexList.Add(index);
                }
            }

            if (productIndexList != null) productIndexList = productIndexList.OrderByDescending(p => p.CantidadTransacciones).ToList();
        }

        private async Task LoadDocuments()
        {
            await DeleteIndexIfExistsAsync(cotizadorAgreementIndex);
            await DeleteIndexIfExistsAsync(cotizadorProductIndex);
            await DeleteIndexIfExistsAsync(cotizadorDepartmentIndex);

            await CreateAgreementIndexAsync();
            await CreateProductIndexAsync();
            await CreateDepartmentIndexAsync();

            SendAgreements();
            SendProducts();
            SendDepartments();
        }

        public async Task Start()
        {
            await LoadDataAsync();
            await GetDataAsync();
            GetDocuments();
            await LoadDocuments();
        }
    }
}