using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;
using CEAM.AzureSearch.Models.Documents;
using System.IO;
using System.Text.Json;
using CEAM.AzureSearch.Loader.Helpers;
using System.Net;
using CEAM.AzureSearch.Models.Indexes;

namespace CEAM.AzureSearch.Loader.Processes
{
    public class AzureSearchProcess
    {
        #region "Properties"
        private string targetSearchServiceName;
        private string targetAdminKey;
        private string targetIndexName;
        private string targetAgreementIndexName;
        private string targetCatalogueIndexName;
        private string targetCategoryIndexName;
        private string backupDirectory;

        private SearchIndexClient _targetIndexClient;
        #endregion

        #region "Constructors"
        public AzureSearchProcess()
        {
            ConfigurationSetup();
        }
        #endregion

        #region "Public Methods"
        public async Task LoadDocuments((
            List<AgreementIndex>,
            List<CatalogueIndex>,
            List<CategoryIndex>,
            List<ProductSheetDocument>) documents)
        {
            await DeleteIndexIfExistsAsync(targetAgreementIndexName);
            await DeleteIndexIfExistsAsync(targetCatalogueIndexName);
            await DeleteIndexIfExistsAsync(targetCategoryIndexName);
            await DeleteIndexIfExistsAsync(targetIndexName);

            await CreateAgreementIndexAsync();
            await CreateCatalogueIndexAsync();
            await CreateCategoryIndexAsync();
            await CreateProductSheetIndexAsync();

            SendAgreements(documents.Item1);
            SendCatalogues(documents.Item2);
            SendCategories(documents.Item3);
            SendProductSheets(documents.Item4);            
        }
        #endregion

        #region "Private Methods"
        private void ConfigurationSetup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            targetSearchServiceName = configuration["AzureSearch:Load:ServiceName"];
            targetAdminKey = configuration["AzureSearch:Load:AdminKey"];
            targetIndexName = configuration["AzureSearch:Load:IndexName"];
            targetAgreementIndexName = configuration["AzureSearch:Load:AgreementIndexName"];
            targetCatalogueIndexName = configuration["AzureSearch:Load:CatalogueIndexName"];
            targetCategoryIndexName = configuration["AzureSearch:Load:CategoryIndexName"];

            backupDirectory = configuration["AzureSearch:Load:Directory"];
            _targetIndexClient = new SearchIndexClient(new Uri("https://" + targetSearchServiceName + ".search.windows.net"), new AzureKeyCredential(targetAdminKey));
        }
        #endregion

        #region "Restore Index and Documents"

        private async Task DeleteIndexIfExistsAsync(string indexName)
        {
            Console.WriteLine("Azure Search: Delete index -> " + indexName);

            try
            {
                //await indexClient.GetIndexAsync(indexName);
                await _targetIndexClient.DeleteIndexAsync(indexName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task CreateProductSheetIndexAsync()
        {
            Console.WriteLine("Azure Search: Create index -> " + targetIndexName);
            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetIndexName, builder.Build(typeof(ProductSheetDocument)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        private async Task CreateAgreementIndexAsync()
        {
            Console.WriteLine("Azure Search: Create index -> " + targetAgreementIndexName);

            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetAgreementIndexName, builder.Build(typeof(AgreementIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task CreateCatalogueIndexAsync()
        {
            Console.WriteLine("Azure Search: Create index -> " + targetCatalogueIndexName);

            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetCatalogueIndexName, builder.Build(typeof(CatalogueIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task CreateCategoryIndexAsync()
        {
            Console.WriteLine("Azure Search: Create index -> " + targetCategoryIndexName);

            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetCategoryIndexName, builder.Build(typeof(CategoryIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void SendAgreements(List<AgreementIndex> documents = null)
        {
            Console.WriteLine("Azure Search: Import Agreements");

            try
            {
                Uri ServiceUri = new Uri("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new Uri(ServiceUri, "/indexes/" + targetAgreementIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    var documentJson = new RequestDocument<AgreementIndex> { value = documents };
                    var json = JsonSerializer.Serialize(documentJson);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("  -> Uploaded Agreements: {0}", documents.Count.ToString());
                }

                Console.WriteLine("  -> Agreements was uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }

        private void SendCatalogues(List<CatalogueIndex> documents = null)
        {
            Console.WriteLine("Azure Search: Import Catalogues");

            try
            {
                Uri ServiceUri = new Uri("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new Uri(ServiceUri, "/indexes/" + targetCatalogueIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    var documentJson = new RequestDocument<CatalogueIndex> { value = documents };
                    var json = JsonSerializer.Serialize(documentJson);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("  -> Uploaded Catalogues: {0}", documents.Count.ToString());
                }

                Console.WriteLine("  -> Catalogues was uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }

        private void SendCategories(List<CategoryIndex> documents = null)
        {
            Console.WriteLine("Azure Search: Import Categories");

            try
            {
                Uri ServiceUri = new Uri("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new Uri(ServiceUri, "/indexes/" + targetCategoryIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    var documentJson = new RequestDocument<CategoryIndex> { value = documents };
                    var json = JsonSerializer.Serialize(documentJson);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("  -> Uploaded Categorys: {0}", documents.Count.ToString());
                }

                Console.WriteLine("  -> Categories was uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }

        private void SendProductSheets(List<ProductSheetDocument> documents = null)
        {
            Console.WriteLine("Azure Search: Import product sheets");

            try
            {
                Uri ServiceUri = new Uri("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new Uri(ServiceUri, "/indexes/" + targetIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    bool condition = true;
                    int block = 1000;
                    int min = block * -1;

                    while (condition)
                    {
                        min += block;

                        if ((min + block) >= documents.Count)
                        {
                            block = documents.Count - min;
                            condition = false;
                        }

                        var documentJson = new RequestDocument<ProductSheetDocument> { value = documents.GetRange(min, block) };
                        var json = JsonSerializer.Serialize(documentJson);

                        HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine("  -> Uploaded documents: {0}", (min + block).ToString());
                    }
                }
                else
                {
                    foreach (string fileName in Directory.GetFiles(backupDirectory + "\\" + targetIndexName, targetIndexName + "*.json"))
                    {
                        Console.WriteLine("  -> Uploading documents from file {0}", fileName);
                        string json = File.ReadAllText(fileName);
                        HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine("  -> Uploaded documents from file {0}", fileName);
                    }
                }

                Console.WriteLine("  -> documents was uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }

        private void ModifyJSON()
        {
            try
            {
                foreach (string fileName in Directory.GetFiles(backupDirectory + "\\" + targetIndexName + "_original", targetIndexName + "*.json"))
                {
                    Console.WriteLine("  -> Uploading documents from file {0}", fileName);
                    string json = File.ReadAllText(fileName);
                    string updatedJson = json.Replace("DeliveryDeparments", "DeliveryDepartments");
                    var jsonFile = JsonSerializer.Deserialize<RequestDocument<ProductSheetDocument>>(updatedJson);

                    foreach(var ps in jsonFile.value)
                    {
                        ps.SearchText = StringHelper.RemoveDiacritics(ps.Name);
                        ps.Agreement.SearchText = StringHelper.RemoveDiacritics(ps.Agreement.Name);
                        //ps.Catalogue.SearchText = StringHelper.RemoveDiacritics(ps.Catalogue.Name);
                        //ps.Category.SearchText = StringHelper.RemoveDiacritics(ps.Category.Name);
                        
                        //if (ps.DeliveryDepartments == null || ps.DeliveryDepartments.Length == 0)
                        //{
                        //    Console.WriteLine("sssss");
                        //}
                    }

                    string newJson = JsonSerializer.Serialize(jsonFile);
                    var updatedNewJson = newJson.Replace("\"DeliveryDepartments\":null", "\"DeliveryDepartments\":[]");

                    var filePath = Path.Combine(fileName.Replace("_original",""));
                    File.WriteAllText(filePath, updatedNewJson);

                    Console.WriteLine(String.Format(" -> file: {0}", fileName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error: {0}", ex.Message.ToString());
            }
        }

        #endregion
    }
}
