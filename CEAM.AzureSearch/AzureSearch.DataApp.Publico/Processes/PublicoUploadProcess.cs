using Azure;
using Azure.Search.Documents;
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
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AzureSearch.DataApp.Publico.Util;

namespace AzureSearch.DataApp.Publico.Processes
{
    public class PublicoUploadProcess
    {
        #region "Properties"
        private readonly string targetSearchServiceName;
        private readonly string targetAdminKey;
        private readonly string targetIndexName;
        private readonly string targetAgreementIndexName;
        private readonly string targetCatalogueIndexName;
        private readonly string targetCategoryIndexName;
        private readonly string backupDirectory;

        private readonly SearchIndexClient _targetIndexClient;
        private readonly SearchClient _productSearchClient;
        private readonly SearchClient _agreementSearchClient;
        private readonly SearchClient _catalogueSearchClient;
        private readonly SearchClient _categorySearchClient;
        #endregion

        #region "Constructors"
        public PublicoUploadProcess()
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

            _targetIndexClient = new SearchIndexClient(new Uri($"https://{targetSearchServiceName}.search.windows.net"), new AzureKeyCredential(targetAdminKey));
            _productSearchClient = _targetIndexClient.GetSearchClient(targetIndexName);
            _agreementSearchClient = _targetIndexClient.GetSearchClient(targetAgreementIndexName);
            _catalogueSearchClient = _targetIndexClient.GetSearchClient(targetCatalogueIndexName);
            _categorySearchClient = _targetIndexClient.GetSearchClient(targetCategoryIndexName);
        }
        #endregion

        #region "Public Methods"
        public async Task LoadDocuments((
            List<PublicoAgreementIndex>,
            List<PublicoCatalogueIndex>,
            List<PublicoCategoryIndex>,
            List<PublicoProductIndex>) documents, bool forceFullReload = false)
        {
            if (forceFullReload)
            {
                MessageUtil.Write(false, "Iniciando recarga completa de todos los índices...");
                await HandleFullReloadAsync(documents);
            }
            else
            {
                MessageUtil.Write(false, "Iniciando actualización incremental de todos los índices...");
                await HandleIncrementalUpdateAsync(documents);
            }
        }
        #endregion

        #region "Workflow Logic"

        private async Task HandleFullReloadAsync((
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

            documents.Item1.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));
            documents.Item2.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));
            documents.Item3.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));
            documents.Item4.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));

            await UploadBatchAsync(_agreementSearchClient, documents.Item1, "Acuerdos");
            await UploadBatchAsync(_catalogueSearchClient, documents.Item2, "Catálogos");
            await UploadBatchAsync(_categorySearchClient, documents.Item3, "Categorías");
            await UploadBatchAsync(_productSearchClient, documents.Item4, "Productos");
        }

        private async Task HandleIncrementalUpdateAsync((
            List<PublicoAgreementIndex>,
            List<PublicoCatalogueIndex>,
            List<PublicoCategoryIndex>,
            List<PublicoProductIndex>) documents)
        {
            await ProcessIndexIncrementallyAsync(_agreementSearchClient, documents.Item1, doc => doc.Id, "Acuerdos");
            await ProcessIndexIncrementallyAsync(_catalogueSearchClient, documents.Item2, doc => doc.Id, "Catálogos");
            await ProcessIndexIncrementallyAsync(_categorySearchClient, documents.Item3, doc => doc.Id, "Categorías");
            await ProcessIndexIncrementallyAsync(_productSearchClient, documents.Item4, doc => doc.Id, "Productos");
        }

        private async Task ProcessIndexIncrementallyAsync<T>(SearchClient searchClient, List<T> newDocuments, Func<T, string> getId, string indexFriendlyName) where T : class
        {
            MessageUtil.Write(false, $"--- Procesando Índice: {indexFriendlyName} ---");

            MessageUtil.Write(false, $"Paso 1: Obteniendo estado actual del índice '{searchClient.IndexName}'...");
            var existingHashes = await GetCurrentHashesAsync(searchClient);
            MessageUtil.Write(true, $"Se encontraron {existingHashes.Count} documentos existentes.");

            MessageUtil.Write(false, "Paso 2: Calculando hashes y clasificando documentos...");
            var toUpload = new List<T>();
            var unchangedCount = 0;
            var newDocumentIds = new HashSet<string>();

            var hashProperty = typeof(T).GetProperty("ContentHash");

            foreach (var doc in newDocuments)
            {
                var id = getId(doc);
                newDocumentIds.Add(id);

                if (hashProperty != null)
                {
                    var newHash = HashingUtil.CalculateContentHash(doc);
                    hashProperty.SetValue(doc, newHash);

                    if (existingHashes.TryGetValue(id, out var existingHash))
                    {
                        if (newHash != existingHash)
                        {
                            toUpload.Add(doc);
                        }
                        else
                        {
                            unchangedCount++;
                        }
                    }
                    else
                    {
                        toUpload.Add(doc);
                    }
                }
            }
            MessageUtil.Write(true, $"{toUpload.Count} documentos para cargar/actualizar, {unchangedCount} sin cambios.");

            MessageUtil.Write(false, "Paso 3: Identificando documentos para eliminar...");
            var toDelete = existingHashes.Keys.Where(id => !newDocumentIds.Contains(id))
                                              .Select(id => new { Id = id }).ToList();
            MessageUtil.Write(true, $"{toDelete.Count} documentos para eliminar.");

            if (toUpload.Any())
            {
                await UploadBatchAsync(searchClient, toUpload, indexFriendlyName);
            }
            if (toDelete.Any())
            {
                await DeleteBatchAsync(searchClient, toDelete, indexFriendlyName);
            }

            MessageUtil.Write(false, $"--- Finalizado: {indexFriendlyName} ---");
        }

        #endregion

        #region "Azure Search Operations"
        private async Task<Dictionary<string, string>> GetCurrentHashesAsync(SearchClient searchClient)
        {
            var hashes = new Dictionary<string, string>();
            var options = new SearchOptions
            {
                Select = { "Id", "ContentHash" },
                Size = 1000
            };

            try
            {
                var searchResult = await searchClient.SearchAsync<JsonDocument>("*", options);
                foreach (var page in searchResult.Value.GetResults().AsPages())
                {
                    foreach (var result in page.Values)
                    {
                        var doc = result.Document;
                        if (doc.RootElement.TryGetProperty("Id", out var idElement) && doc.RootElement.TryGetProperty("ContentHash", out var hashElement))
                        {
                            var id = idElement.GetString();
                            var hash = hashElement.GetString();
                            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(hash))
                            {
                                hashes[id] = hash;
                            }
                        }
                    }
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                 MessageUtil.Write(false, $"Advertencia: El índice '{searchClient.IndexName}' no existe. Se tratará como una carga inicial.");
            }
            return hashes;
        }

        private async Task UploadBatchAsync<T>(SearchClient searchClient, List<T> documents, string indexFriendlyName)
        {
            if (documents == null || !documents.Any()) return;

            MessageUtil.Write(false, $"Azure Search: {indexFriendlyName} -> Iniciando carga de {documents.Count} registros...");

            const int batchSize = 1000;

            if (indexFriendlyName != "Productos" || documents.Count <= batchSize)
            {
                try
                {
                    await searchClient.UploadDocumentsAsync(documents, new IndexDocumentsOptions { ThrowOnAnyError = true });
                    MessageUtil.Write(true, $"Azure Search: {indexFriendlyName} -> {documents.Count} registros procesados exitosamente en una sola operación.");
                }
                catch (Exception ex)
                {
                    MessageUtil.Write(false, $"  Error en lote de carga único para {indexFriendlyName}: {ex.Message}");
                }
                return;
            }

            int totalBatches = (int)Math.Ceiling((double)documents.Count / batchSize);
            MessageUtil.Write(false, $"Se procesarán {totalBatches} lotes de hasta {batchSize} registros cada uno.");

            for (int i = 0; i < totalBatches; i++)
            {
                var batch = documents.Skip(i * batchSize).Take(batchSize).ToList();
                if (!batch.Any()) continue;

                MessageUtil.Write(false, $"  Procesando lote {i + 1} de {totalBatches} ({batch.Count} registros)...");
                try
                {
                    var response = await searchClient.UploadDocumentsAsync(batch, new IndexDocumentsOptions { ThrowOnAnyError = true });

                    if (response.Value.Results.Any(r => !r.Succeeded))
                    {
                        var failedDocs = response.Value.Results.Where(r => !r.Succeeded);
                        MessageUtil.Write(false, $"    ¡Advertencia! {failedDocs.Count()} documentos fallaron en el lote {i + 1}. Primer error: {failedDocs.First().ErrorMessage}");
                    }
                    else
                    {
                        MessageUtil.Write(true, $"    Lote {i + 1} completado exitosamente.");
                    }
                }
                catch (RequestFailedException ex)
                {
                    MessageUtil.Write(false, $"  Error en lote de carga {i + 1} para {indexFriendlyName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageUtil.Write(false, $"  Error general en lote de carga {i + 1} para {indexFriendlyName}: {ex.Message}");
                }
            }
             MessageUtil.Write(true, $"Azure Search: {indexFriendlyName} -> Carga por lotes finalizada.");
        }

        private async Task DeleteBatchAsync<T>(SearchClient searchClient, List<T> documents, string indexFriendlyName) where T : class
        {
            if (documents == null || !documents.Any()) return;

            MessageUtil.Write(false, $"Azure Search: {indexFriendlyName} -> Eliminando {documents.Count} registros...");
            try
            {
                await searchClient.DeleteDocumentsAsync(documents, new IndexDocumentsOptions { ThrowOnAnyError = true });
                MessageUtil.Write(true, $"Azure Search: {indexFriendlyName} -> {documents.Count} registros eliminados exitosamente.");
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, $"  Error en lote de eliminación para {indexFriendlyName}: {ex.Message}");
            }
        }
        #endregion

        #region "Configuration and Index Management"
        private async Task DeleteIndexIfExistsAsync(string indexName)
        {
            MessageUtil.Write(false,"Azure Search: Delete index -> " + indexName);
            try
            {
                await _targetIndexClient.DeleteIndexAsync(indexName);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                MessageUtil.Write(false, "El índice no existía, no se requiere eliminación.");
            }
        }

        private async Task CreateAgreementIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + targetAgreementIndexName);
            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetAgreementIndexName, builder.Build(typeof(PublicoAgreementIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, ex.Message);
                throw;
            }
        }

        private async Task CreateCatalogueIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + targetCatalogueIndexName);
            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetCatalogueIndexName, builder.Build(typeof(PublicoCatalogueIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, ex.Message);
                throw;
            }
        }

        private async Task CreateCategoryIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + targetCategoryIndexName);
            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetCategoryIndexName, builder.Build(typeof(PublicoCategoryIndex)));
                await _targetIndexClient.CreateIndexAsync(definition);
            }
            catch (Exception ex)
            {
                MessageUtil.Write(false, ex.Message);
                throw;
            }
        }

        private async Task CreateProductSheetIndexAsync()
        {
            MessageUtil.Write(false, "Azure Search: Create index -> " + targetIndexName);
            try
            {
                FieldBuilder builder = new FieldBuilder();
                var definition = new SearchIndex(targetIndexName, builder.Build(typeof(PublicoProductIndex)));

                definition.VectorSearch = new VectorSearch
                {
                    Profiles = { new VectorSearchProfile("my-vector-profile", "exhaustive-knn-algorithm") },
                    Algorithms = { new ExhaustiveKnnAlgorithmConfiguration("exhaustive-knn-algorithm") }
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
