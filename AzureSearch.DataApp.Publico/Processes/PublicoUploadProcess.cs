using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureSearch.DataApp.Publico.Util; // Nueva utilidad de Hashing
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
using System.Threading;
using System.Threading.Tasks;

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

            // Inicializar clientes de índice y de búsqueda
            _targetIndexClient = new SearchIndexClient(new Uri($"https://{targetSearchServiceName}.search.windows.net"), new AzureKeyCredential(targetAdminKey));
            _productSearchClient = _targetIndexClient.GetSearchClient(targetIndexName);
            _agreementSearchClient = _targetIndexClient.GetSearchClient(targetAgreementIndexName);
            _catalogueSearchClient = _targetIndexClient.GetSearchClient(targetCatalogueIndexName);
            _categorySearchClient = _targetIndexClient.GetSearchClient(targetCategoryIndexName);
        }
        #endregion

        #region "Public Methods"
        /// <summary>
        /// Carga los documentos en los índices de Azure AI Search.
        /// Puede realizar una recarga completa o una actualización incremental.
        /// </summary>
        /// <param name="documents">Una tupla que contiene las listas de documentos para cada índice.</param>
        /// <param name="forceFullReload">Si es true, borra y recarga todos los datos. Si es false, realiza una actualización incremental.</param>
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

        /// <summary>
        /// Orquesta una recarga completa (borrar y cargar) de todos los índices.
        /// Calcula y asigna el ContentHash a cada documento antes de la carga.
        /// </summary>
        private async Task HandleFullReloadAsync((
            List<PublicoAgreementIndex>,
            List<PublicoCatalogueIndex>,
            List<PublicoCategoryIndex>,
            List<PublicoProductIndex>) documents)
        {
            // 1. Borrar y recrear todos los índices
            await DeleteIndexIfExistsAsync(targetAgreementIndexName);
            await DeleteIndexIfExistsAsync(targetCatalogueIndexName);
            await DeleteIndexIfExistsAsync(targetCategoryIndexName);
            await DeleteIndexIfExistsAsync(targetIndexName);

            await CreateAgreementIndexAsync();
            await CreateCatalogueIndexAsync();
            await CreateCategoryIndexAsync();
            await CreateProductSheetIndexAsync();

            // 2. Calcular hashes para todos los documentos antes de la carga
            MessageUtil.Write(false, "Calculando hashes para la carga completa...");
            documents.Item1.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));
            documents.Item2.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));
            documents.Item3.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));
            documents.Item4.ForEach(doc => doc.ContentHash = HashingUtil.CalculateContentHash(doc));

            // 3. Cargar los datos en lotes
            await UploadBatchAsync(_agreementSearchClient, documents.Item1, "Acuerdos");
            await UploadBatchAsync(_catalogueSearchClient, documents.Item2, "Catálogos");
            await UploadBatchAsync(_categorySearchClient, documents.Item3, "Categorías");
            await UploadBatchAsync(_productSearchClient, documents.Item4, "Productos");
        }

        /// <summary>
        /// Orquesta una actualización incremental para cada uno de los cuatro índices.
        /// </summary>
        private async Task HandleIncrementalUpdateAsync((
            List<PublicoAgreementIndex>,
            List<PublicoCatalogueIndex>,
            List<PublicoCategoryIndex>,
            List<PublicoProductIndex>) documents)
        {
            // Procesar cada tipo de documento de forma incremental
            await ProcessIndexIncrementallyAsync(_agreementSearchClient, documents.Item1, doc => doc.Id, "Acuerdos");
            await ProcessIndexIncrementallyAsync(_catalogueSearchClient, documents.Item2, doc => doc.Id, "Catálogos");
            await ProcessIndexIncrementallyAsync(_categorySearchClient, documents.Item3, doc => doc.Id, "Categorías");
            await ProcessIndexIncrementallyAsync(_productSearchClient, documents.Item4, doc => doc.Id, "Productos");
        }

        /// <summary>
        /// Lógica genérica para procesar un índice de forma incremental.
        /// Compara los hashes de los documentos nuevos con los existentes para determinar qué cargar, qué actualizar y qué eliminar.
        /// </summary>
        private async Task ProcessIndexIncrementallyAsync<T>(SearchClient searchClient, List<T> newDocuments, Func<T, string> getId, string indexFriendlyName) where T : class
        {
            MessageUtil.Write(false, $"--- Procesando Índice: {indexFriendlyName} ---");

            // Paso 1: Obtener el estado actual (IDs y Hashes) desde Azure Search
            MessageUtil.Write(false, $"Paso 1: Obteniendo estado actual del índice '{searchClient.IndexName}'...");
            var existingHashes = await GetCurrentHashesAsync(searchClient);
            MessageUtil.Write(true, $"Se encontraron {existingHashes.Count} documentos existentes.");

            // Paso 2: Calcular hashes para los nuevos documentos y clasificarlos
            MessageUtil.Write(false, "Paso 2: Calculando hashes y clasificando documentos...");
            var toUpload = new List<T>();
            var unchangedCount = 0;
            var newDocumentIds = new HashSet<string>();

            // Obtener la propiedad ContentHash una sola vez para eficiencia
            var hashProperty = typeof(T).GetProperty("ContentHash");

            foreach (var doc in newDocuments)
            {
                var id = getId(doc);
                newDocumentIds.Add(id);

                if (hashProperty != null)
                {
                    var newHash = HashingUtil.CalculateContentHash(doc);
                    hashProperty.SetValue(doc, newHash); // Asignar el nuevo hash al documento

                    if (existingHashes.TryGetValue(id, out var existingHash))
                    {
                        if (newHash != existingHash)
                        {
                            toUpload.Add(doc); // Documento modificado
                        }
                        else
                        {
                            unchangedCount++; // Documento sin cambios
                        }
                    }
                    else
                    {
                        toUpload.Add(doc); // Documento nuevo
                    }
                }
            }
            MessageUtil.Write(true, $"{toUpload.Count} documentos para cargar/actualizar, {unchangedCount} sin cambios.");

            // Paso 3: Identificar los documentos que deben ser eliminados
            MessageUtil.Write(false, "Paso 3: Identificando documentos para eliminar...");
            var toDelete = existingHashes.Keys.Where(id => !newDocumentIds.Contains(id))
                                              .Select(id => new { Id = id }).ToList();
            MessageUtil.Write(true, $"{toDelete.Count} documentos para eliminar.");

            // Paso 4: Ejecutar las operaciones por lotes en Azure Search
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

        /// <summary>
        /// Obtiene un diccionario de todos los documentos existentes en un índice, mapeando su ID a su ContentHash.
        /// </summary> 
        public static async Task<Dictionary<string, string>> GetCurrentHashesAsync(
            SearchClient searchClient,
            CancellationToken cancellationToken = default)
        {
            var hashes = new Dictionary<string, string>(capacity: 262144); // pre-allocate algo grande
            var options = new SearchOptions
            {
                Select = { "Id", "ContentHash" },
                Size = 1000 // page size máximo recomendable por la API (1k)
            };

            try
            {
                // wildcard search para todo el índice.
                Response<SearchResults<JsonDocument>> response =
                    await searchClient.SearchAsync<JsonDocument>("*", options, cancellationToken).ConfigureAwait(false);

                AsyncPageable<SearchResult<JsonDocument>> results = response.Value.GetResultsAsync();

                // await foreach hace auto-paging detrás de cámaras
                await foreach (SearchResult<JsonDocument> result in results.WithCancellation(cancellationToken))
                {
                    var doc = result.Document;
                    if (doc.RootElement.TryGetProperty("Id", out var idElement) &&
                        doc.RootElement.TryGetProperty("ContentHash", out var hashElement))
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
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Índice inexistente: aviso y devuelve vacio.
                MessageUtil.Write(false, $"Advertencia: El índice '{searchClient.IndexName}' no existe. Se tratará como una carga inicial.");
            }
            // Si quieres manejar otros códigos, agrega más catch.
            return hashes;
        }
        /// <summary>
        /// Carga o actualiza un lote de documentos en un índice específico usando el SDK de Azure.
        /// </summary>
        private async Task UploadBatchAsync<T>(SearchClient searchClient, List<T> documents, string indexFriendlyName)
        {
            if (documents == null || !documents.Any()) return;


            MessageUtil.Write(false, $"Azure Search: {indexFriendlyName} -> Iniciando carga de {documents.Count} registros...");

            const int batchSize = 1000;

            // Si el índice no es "Productos", o si la cantidad es menor al tamaño del lote, cargar todo de una vez.
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

            // Lógica de carga por lotes solo para el índice de Productos
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

                    // Opcional: Revisar si algún documento específico falló dentro del lote
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
                    // Captura excepciones de la solicitud de Azure para dar más detalles
                    MessageUtil.Write(false, $"  Error en lote de carga {i + 1} para {indexFriendlyName}: {ex.Message}");
                    // Podrías decidir si continuar con el siguiente lote o detenerte.
                    // Por ahora, continuaremos.
                }
                catch (Exception ex)
                {
                    MessageUtil.Write(false, $"  Error general en lote de carga {i + 1} para {indexFriendlyName}: {ex.Message}");
                }
            }
            MessageUtil.Write(true, $"Azure Search: {indexFriendlyName} -> Carga por lotes finalizada.");
        }

        /// <summary>
        /// Elimina un lote de documentos de un índice específico usando el SDK de Azure.
        /// </summary>
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
            MessageUtil.Write(false, "Azure Search: Delete index -> " + indexName);
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
                   new SimpleField(nameof(PublicoProductIndex.ContentHash), SearchFieldDataType.String) { IsFilterable = true},
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
                           new SimpleField(nameof(PublicoAgreementDocument.ContentHash), SearchFieldDataType.String) { IsFilterable = true},
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
                           new SimpleField(nameof(PublicoCatalogueDocument.Name), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoCatalogueDocument.ContentHash), SearchFieldDataType.String) { IsFilterable = true},
                       }
                   },

                   // Campo complejo Category
                   new SearchField(nameof(PublicoProductIndex.Category), SearchFieldDataType.Complex)
                   {
                       Fields =
                       {
                           new SimpleField(nameof(PublicoCategoryDocument.Id), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoCategoryDocument.Name), SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                           new SimpleField(nameof(PublicoCategoryDocument.ContentHash), SearchFieldDataType.String) { IsFilterable = true},
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
