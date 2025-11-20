using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using CEAM.AzureSearch.Loader.Data.Repositories;
using CEAM.AzureSearch.Loader.Helpers;
using CEAM.AzureSearch.Loader.Utils;
using CEAM.AzureSearch.Models.Documents;
using CEAM.AzureSearch.Models.Entities;
using CEAM.AzureSearch.Models.Indexes;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CEAM.AzureSearch.Loader.Processes
{
    public class CotizadorProcess
    {
        private CotizadorRepository _cotizadorRepository { get; set; }

        private List<AgreementEntity> agreements { get; set; }
        private List<CatalogueEntity> catalogues { get; set; }
        private List<CategoryEntity> categories { get; set; }
        private List<ProductFeatureEntity> productfeatures { get; set; }
        private List<ProductEntity> products { get; set; }
        private List<FeatureFilterEntity> featurefilters { get; set; }


        private List<CotizadorProductIndex> productIndexList { get; set; }
        private List<CotizadorAgreementIndex> agreementIndexList { get; set; }

        private string cotizadorAgreementIndex { get; set; }
        private string cotizadorProductIndex { get; set; }
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

            indexClient = new SearchIndexClient(new Uri("https://" + searchServiceName + ".search.windows.net"), new AzureKeyCredential(adminKey));
        }

        private async Task DeleteIndexIfExistsAsync(string indexName)
        {
            Console.WriteLine("Azure Search: Delete index -> " + indexName);

            try
            {
                //await indexClient.GetIndexAsync(indexName);
                await indexClient.DeleteIndexAsync(indexName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task CreateProductIndexAsync()
        {
            Console.WriteLine("Azure Search: Create index -> " + cotizadorProductIndex);
            try
            {
                FieldBuilder builder = new FieldBuilder();
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
            Console.WriteLine("Azure Search: Create index -> " + cotizadorAgreementIndex);

            try
            {
                FieldBuilder builder = new FieldBuilder();
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
            Console.WriteLine("Azure Search: Import Agreements");

            try
            {
                Uri ServiceUri = new Uri("https://" + searchServiceName + ".search.windows.net");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", adminKey);
                Uri uri = new Uri(ServiceUri, "/indexes/" + cotizadorAgreementIndex + "/docs/index");

                var documentJson = new RequestDocument<CotizadorAgreementIndex> { value = agreementIndexList };
                var json = JsonSerializer.Serialize(documentJson);
                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("  -> Uploaded Agreements: {0}", agreementIndexList.Count.ToString());

                Console.WriteLine("  -> Agreements was uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }
        private void SendProducts()
        {
            Console.WriteLine("Azure Search: Import product sheets");

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

                    var documentJson = new RequestDocument<CotizadorProductIndex> { value = productIndexList.GetRange(min, block) };
                    var json = JsonSerializer.Serialize(documentJson);

                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("  -> Uploaded documents: {0}", (min + block).ToString());
                }

                Console.WriteLine("  -> documents was uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }

        public CotizadorProcess()
        {
            Setup();
            _cotizadorRepository = new CotizadorRepository();

            agreements = new List<AgreementEntity>();
            catalogues = new List<CatalogueEntity>();
            categories = new List<CategoryEntity>();
            products = new List<ProductEntity>();
            productfeatures = new List<ProductFeatureEntity>();
            featurefilters = new List<FeatureFilterEntity>();

            agreementIndexList = new List<CotizadorAgreementIndex>();
            productIndexList = new List<CotizadorProductIndex>();
        }

        private async Task LoadDataAsync()
        {
            MessageUtil.Write("Load Agreements, Catalogues and Categories");
            await _cotizadorRepository.LoadData();

            MessageUtil.Write("Load Products");
            await _cotizadorRepository.LoadProducts();

            //MessageUtil.Write("Load Feature Filters");
            //await _cotizadorRepository.LoadFeatureFilters();

            MessageUtil.Write("Load Product Filters");
            await _cotizadorRepository.LoadProductFilters();
        }

        private async Task GetDataAsync()
        {
            MessageUtil.Write("Get Agreements");
            agreements = await _cotizadorRepository.GetData<AgreementEntity>("Agreement");

            MessageUtil.Write("Get Catalogues");
            catalogues = await _cotizadorRepository.GetData<CatalogueEntity>("Catalogue");

            MessageUtil.Write("Get Categories");
            categories = await _cotizadorRepository.GetData<CategoryEntity>("Category");

            MessageUtil.Write("Get Products");
            products = await _cotizadorRepository.GetData<ProductEntity>("Product");

            //MessageUtil.Write("Get Feature Filters");
            //featurefilters = await _cotizadorRepository.GetData<FeatureFilterEntity>("FeatureFilter");

            MessageUtil.Write("Get Product Features");
            productfeatures = await _cotizadorRepository.GetData<ProductFeatureEntity>("ProductFeature");
        }

        private void GetDocuments()
        {
            foreach(var agreement in agreements)
            {
                var newCatalogueList = catalogues.Where(w => w.AgreementId == agreement.AgreementId)
                                                 .Select(w => new CotizadorCatalogueDocument { 
                                                     Id = w.CatalogueId.ToString(),
                                                     Name = w.CatalogueName
                                                 }).ToList();

                foreach(var catalogue in newCatalogueList)
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
                    TextSearch = StringHelper.RemoveDiacritics(agreement.AgreementName),
                    Catalogues = newCatalogueList
                };

                agreementIndexList.Add(index);
            }

            var featureList = productfeatures.GroupBy(g => new { g.ProductId })
                                             .Select(g => new { 
                                                 ProductId = g.Key.ProductId,
                                                 Types = g.GroupBy(h => new { h.FeatureTypeId })
                                                          .Select(h => new CotizadorFeatureTypeDocument {
                                                              Id = h.Key.FeatureTypeId.ToString(),
                                                              Name = h.First().FeatureTypeName,
                                                              TypeCondition = h.First().FeatureTypeCondition,
                                                              RequiredSubValue = h.First().RequiredSubValue,
                                                              Values = h.Select(i => new CotizadorFeatureValueDocument { 
                                                                    Id = i.FeatureValueId.ToString(),
                                                                    Name = i.FeatureValueName,
                                                                    NameSub = i.FeatureValueNameSub }).ToList()
                                                          }).ToList(),
                                                 Filters = g.Select(s =>
                                                    s.FeatureTypeId.ToString() + StringHelper.Separator +
                                                    s.FeatureTypeName + StringHelper.Separator +
                                                    s.FeatureValueId.ToString() + StringHelper.Separator +
                                                    s.FeatureValueName).ToArray(),
                                                 FilterIds = g.Select(s => 
                                                    s.FeatureTypeId.ToString() + StringHelper.Separator + 
                                                    s.FeatureValueId.ToString()).ToArray()
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
                                          .Select(w => new CotizadorAgreementDocument { 
                                              Id = w.AgreementId.ToString(),
                                              Name = w.AgreementName.Trim()
                                          }).FirstOrDefault();

                var index = new CotizadorProductIndex
                {
                    Id = product.ProductId.ToString(),
                    Name = product.ProductName.Trim(),
                    TextSearch = StringHelper.RemoveDiacritics(product.ProductName),
                    PublishedDate = product.ProductPublishedDate,
                    UpdatedDate = product.ProductUpdatedDate,
                    ImageURL = product.ProductImage,
                    FileURL = product.ProductFile,
                    Status = product.ProductStatus,
                    //Departments = new string[] { },
                    FeatureFilters = featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.Filters).FirstOrDefault(),
                    Features = featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.Types).FirstOrDefault(),
                    FeatureIdFilters = featureList.Where(w => w.ProductId == product.ProductId).Select(w => w.FilterIds).FirstOrDefault(),
                    Agreement = agreement,
                    Catalogue = catalogue,
                    Category = category
                };

                productIndexList.Add(index);
            }

            var a98 = agreementIndexList.Where(w => w.Id == "97").ToList();
            var p98 = productIndexList.Where(w => w.Catalogue.Id == "98").ToList();
            var p99 = productIndexList.Where(w => w.Catalogue.Id == "99").ToList();
        }

        private async Task LoadDocuments()
        {
            await DeleteIndexIfExistsAsync(cotizadorAgreementIndex);
            await DeleteIndexIfExistsAsync(cotizadorProductIndex);

            await CreateAgreementIndexAsync();
            //await CreateCatalogueIndexAsync();
            //await CreateCategoryIndexAsync();
            await CreateProductIndexAsync();

            SendAgreements();
            //SendCatalogues(documents.Item2);
            //SendCategories(documents.Item3);
            SendProducts();
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
