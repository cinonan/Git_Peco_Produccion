using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using AzureSearch.Models;
using AzureSearch.Models.Publico.Documents;
using AzureSearch.Models.Publico.Indexes;
using AzureSearch.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureSearch.DataApp.Publico.Processes
{
    public class PublicoUploadProcess
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
        public PublicoUploadProcess()
        {
            ConfigurationSetup();
        }
        #endregion

        #region "Public Methods"
        public async Task LoadDocuments((
            List<PublicoAgreementIndex>,
            List<PublicoCatalogueIndex>,
            List<PublicoCategoryIndex>,
            List<PublicoProductIndex>) documents)
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
            MessageUtil.Write(false,"Azure Search: Delete index -> " + indexName);

            try
            {
                //await indexClient.GetIndexAsync(indexName);
                await _targetIndexClient.DeleteIndexAsync(indexName);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,ex.Message);
            }
        }

        /*
        private async Task CreateProductSheetIndexAsync()
        {
            MessageUtil.Write(false,"Azure Search: Create index -> " + targetIndexName);
            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetIndexName, builder.Build(typeof(PublicoProductIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,ex.Message);
                throw;
            }
        }
        */
        private async Task CreateAgreementIndexAsync()
        {
            MessageUtil.Write(false,"Azure Search: Create index -> " + targetAgreementIndexName);

            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetAgreementIndexName, builder.Build(typeof(PublicoAgreementIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,ex.Message);
                throw;
            }
        }

        private async Task CreateCatalogueIndexAsync()
        {
            MessageUtil.Write(false,"Azure Search: Create index -> " + targetCatalogueIndexName);

            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetCatalogueIndexName, builder.Build(typeof(PublicoCatalogueIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,ex.Message);
                throw;
            }
        }

        private async Task CreateCategoryIndexAsync()
        {
            MessageUtil.Write(false,"Azure Search: Create index -> " + targetCategoryIndexName);

            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetCategoryIndexName, builder.Build(typeof(PublicoCategoryIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,ex.Message);
                throw;
            }
        }

        private void SendAgreements(List<PublicoAgreementIndex> documents = null)
        {
            MessageUtil.Write(false,"Azure Search: Agreements -> Uploading...");

            try
            {
                Uri ServiceUri = new ("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new ();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new (ServiceUri, "/indexes/" + targetAgreementIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    var documentJson = new RequestModel<PublicoAgreementIndex> { value = documents };
                    var json = JsonSerializer.Serialize(documentJson);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    MessageUtil.Write(true, string.Format("Azure Search: Agreements -> {0} records uploaded", documents.Count.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,"  Error: {0}", ex.Message.ToString());
            }
        }

        private void SendCatalogues(List<PublicoCatalogueIndex> documents = null)
        {
            MessageUtil.Write(false, "Azure Search: Catalogues -> Uploading...");

            try
            {
                Uri ServiceUri = new ("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new ();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new (ServiceUri, "/indexes/" + targetCatalogueIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    var documentJson = new RequestModel<PublicoCatalogueIndex> { value = documents };
                    var json = JsonSerializer.Serialize(documentJson);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    MessageUtil.Write(true, string.Format("Azure Search: Catalogues -> {0} records uploaded", documents.Count.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,"  Error: {0}", ex.Message.ToString());
            }
        }

        private void SendCategories(List<PublicoCategoryIndex> documents = null)
        {
            MessageUtil.Write(false, "Azure Search: Categories -> Uploading...");

            try
            {
                Uri ServiceUri = new ("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new ();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new (ServiceUri, "/indexes/" + targetCategoryIndexName + "/docs/index");

                if (documents != null && documents.Count > 0)
                {
                    var documentJson = new RequestModel<PublicoCategoryIndex> { value = documents };
                    var json = JsonSerializer.Serialize(documentJson);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                    MessageUtil.Write(true, string.Format("Azure Search: Categories -> {0} records uploaded", documents.Count.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,"  Error: {0}", ex.Message.ToString());
            }
        }

        private void SendProductSheets(List<PublicoProductIndex> documents = null)
        {
            MessageUtil.Write(false, "Azure Search: Products -> Uploading...");

            try
            {
                Uri ServiceUri = new ("https://" + targetSearchServiceName + ".search.windows.net");
                HttpClient httpClient = new ();
                httpClient.DefaultRequestHeaders.Add("api-key", targetAdminKey);
                Uri uri = new (ServiceUri, "/indexes/" + targetIndexName + "/docs/index");

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

                        var documentJson = new RequestModel<PublicoProductIndex> { value = documents.GetRange(min, block) };
                        var json = JsonSerializer.Serialize(documentJson);
                        var reintento = 0;
                        while (reintento < 3)
                        {
                            try
                            {
                                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                                response.EnsureSuccessStatusCode();
                                MessageUtil.Write(true, string.Format("Azure Search: Products -> {0} of {1} records uploaded", (min + block).ToString(), documents.Count.ToString()));
                                reintento = 3;
                            }
                            catch (Exception ex)
                            {
                                reintento++;
                                MessageUtil.Write(false, "  Error SendProductSheets.SendSearchRequest..se hara reintento{0}: {1}", "" + reintento, ex.Message.ToString());
                            }
                        }                        
                    }
                }
                else
                {
                    foreach (string fileName in Directory.GetFiles(backupDirectory + "\\" + targetIndexName, targetIndexName + "*.json"))
                    {
                        MessageUtil.Write(false,"  -> Uploading documents from file {0}", fileName);
                        string json = File.ReadAllText(fileName);
                        HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(httpClient, HttpMethod.Post, uri, json);
                        response.EnsureSuccessStatusCode();
                        MessageUtil.Write(false, string.Format("  -> Uploaded documents from file {0}", fileName));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,"  Error: {0}", ex.Message.ToString());
            }
        }

        private void ModifyJSON()
        {
            try
            {
                foreach (string fileName in Directory.GetFiles(backupDirectory + "\\" + targetIndexName + "_original", targetIndexName + "*.json"))
                {
                    MessageUtil.Write(false,"  -> Uploading documents from file {0}", fileName);
                    string json = File.ReadAllText(fileName);
                    string updatedJson = json.Replace("DeliveryDeparments", "DeliveryDepartments");
                    var jsonFile = JsonSerializer.Deserialize<RequestModel<PublicoProductIndex>>(updatedJson);

                    foreach (var ps in jsonFile.value)
                    {
                        ps.SearchText = StringUtil.RemoveDiacritics_Publico(ps.Name);
                        ps.Agreement.SearchText = StringUtil.RemoveDiacritics_Publico(ps.Agreement.Name);
                    }

                    string newJson = JsonSerializer.Serialize(jsonFile);
                    var updatedNewJson = newJson.Replace("\"DeliveryDepartments\":null", "\"DeliveryDepartments\":[]");

                    var filePath = Path.Combine(fileName.Replace("_original", ""));
                    File.WriteAllText(filePath, updatedNewJson);

                    MessageUtil.Write(false,String.Format(" -> file: {0}", fileName));
                }
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false,"  Error: {0}", ex.Message.ToString());
            }
        }

        private async Task CreateProductSheetIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + targetIndexName);

            try
            {
                // Agrega el campo complejo "Agreement" al índice usando SearchField con subcampos
                var fields = new List<SearchField>
               {
                   // Clave primaria
                   new SimpleField(nameof(PublicoProductIndex.Id), SearchFieldDataType.String) { IsKey = true },

                   // Campos simples
                   new SimpleField(nameof(PublicoProductIndex.Name), SearchFieldDataType.String),
                   new SearchField(nameof(PublicoProductIndex.SearchText), SearchFieldDataType.String)
                   {
                       IsSearchable = true,
                       AnalyzerName = LexicalAnalyzerName.Values.EsLucene
                   },

                   // Campo vectorial
                   
                   new SearchField(nameof(PublicoProductIndex.ProductArray), SearchFieldDataType.Collection(SearchFieldDataType.Single))
                   {
                       IsSearchable = true,
                       VectorSearchDimensions = 768,
                       VectorSearchProfileName = "my-vector-profile"
                   },

                   new SimpleField(nameof(PublicoProductIndex.PublishedDate), SearchFieldDataType.String),
                   new SimpleField(nameof(PublicoProductIndex.UpdatedDate), SearchFieldDataType.String),
                   new SimpleField(nameof(PublicoProductIndex.Image), SearchFieldDataType.String),
                   new SimpleField(nameof(PublicoProductIndex.File), SearchFieldDataType.String),

                   new SimpleField(nameof(PublicoProductIndex.Status), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                   new SearchField(nameof(PublicoProductIndex.Departments), SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true, IsFacetable = true },
                   new SearchField(nameof(PublicoProductIndex.Features), SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true, IsFacetable = true },

                   // Campo complejo Agreement
                   new SearchField(nameof(PublicoProductIndex.Agreement), SearchFieldDataType.Complex)
                   {
                       Fields =
                       {
                           new SimpleField(nameof(PublicoAgreementDocument.Id), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoAgreementDocument.Name), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoAgreementDocument.Status), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SearchField(nameof(PublicoAgreementDocument.SearchText), SearchFieldDataType.String)
                           {
                               IsSearchable = true,
                               AnalyzerName = LexicalAnalyzerName.Values.EsLucene,
                               IsFilterable = true,
                               IsFacetable = true
                           }
                       }
                   },

                   // Campo complejo Catalogue
                   new SearchField(nameof(PublicoProductIndex.Catalogue), SearchFieldDataType.Complex)
                   {
                       Fields =
                       {
                           new SimpleField(nameof(PublicoCatalogueDocument.Id), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoCatalogueDocument.Name), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true }
                       }
                   },

                   // Campo complejo Category
                   new SearchField(nameof(PublicoProductIndex.Category), SearchFieldDataType.Complex)
                   {
                       Fields =
                       {
                           new SimpleField(nameof(PublicoCategoryDocument.Id), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoCategoryDocument.Name), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true }
                       }
                   },

                   // Campo complejo FeatureTypeList
                   new SearchField(nameof(PublicoProductIndex.FeatureTypeList), SearchFieldDataType.Collection(SearchFieldDataType.Complex))
                   {
                       Fields =
                       {
                           new SimpleField(nameof(PublicoFeatureTypeDocument.Id), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoFeatureTypeDocument.Text), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoFeatureTypeDocument.IsRequiredSubValue), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SearchField(nameof(PublicoFeatureTypeDocument.Values), SearchFieldDataType.Collection(SearchFieldDataType.Complex))
                           {
                               Fields =
                               {
                                   new SimpleField(nameof(PublicoFeatureValueDocument.Id), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                                   new SimpleField(nameof(PublicoFeatureValueDocument.Text), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                                   new SimpleField(nameof(PublicoFeatureValueDocument.ValueImg), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                                   new SimpleField(nameof(PublicoFeatureValueDocument.FeatureType), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true }

                               }
                           }
                       }
                   },

               };

                var definition = new SearchIndex(targetIndexName, fields)
                {
                    VectorSearch = new VectorSearch
                    {
                        Profiles =
                {
                    new VectorSearchProfile("my-vector-profile", "exhaustive-knn-algorithm")
                },
                        Algorithms =
                {
                    new ExhaustiveKnnAlgorithmConfiguration("exhaustive-knn-algorithm")
                }
                    }
                };
                await _targetIndexClient.CreateIndexAsync(definition);
                MessageUtil.Write(true, $"Azure Search: Index '{targetIndexName}' created successfully.");
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, ex.Message);
                throw;
            }
        }
        #endregion
    }
}
