using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using AzureSearch.Models.Publico.Documents;
using AzureSearch.Models.Publico.Models;
using CEAM.AzureSearch.WebApp.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;


// ====== Añade estos using ======
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using CEAM.AzureSearch.WebApp.Utils;

using AzureSearch.WebApp.Publico.Utils.Query; // ← nuevo

namespace CEAM.AzureSearch.WebApp.Services
{
    public static class GlobalVariables
    {
        public static int ResultsPerPage { get { return 12; } }
        public static int MaxPageRange { get { return 10; } }
        public static int PageRangeDelta { get { return 2; } }
    }

    public class AzureSearchService : IAzureSearchService
    {
        #region "Properties"
        private static SearchIndexClient _indexClient;
        private static SearchClient _searchClient;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;


        // ====== Añade este cache estático en tu clase (persistente por proceso) ======
        private static readonly ConcurrentDictionary<string, int> _kwSizeCache = new();
        private readonly HttpClient _http;
        private readonly IQueryNormalizer _normalizer;
        #endregion

        #region "Configuration"
        public AzureSearchService(IConfiguration config, IQueryNormalizer normalizer)
        {
            _normalizer = normalizer;
            var baseUrl = config["EmbeddingsApi:BaseUrl"]
                ?? throw new InvalidOperationException("EmbeddingsApi:BaseUrl no configurado");
            var timeoutSec = int.TryParse(config["EmbeddingsApi:TimeoutSeconds"], out var s) ? s : 20;

            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(timeoutSec)
            };

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        }

        private void ConfigureSearch()
        {
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();

            string asUri = _configuration["AzureSearch:Uri"];
            string asQueryApiKey = _configuration["AzureSearch:QueryApiKey"];
            string asIndexName = _configuration["AzureSearch:IndexName"];

            // Create a service and index client.
            _indexClient = new SearchIndexClient(new Uri(asUri), new AzureKeyCredential(asQueryApiKey));
            _searchClient = _indexClient.GetSearchClient(asIndexName);
        }

        private void ConfigurePagination(ref SearchDataModel model, ref int page, ref int leftMostPage)
        {
            model.SearchText = string.IsNullOrWhiteSpace(model.SearchText) ? "" : model.SearchText.Trim();
            model.SearchTextPrevious = string.IsNullOrWhiteSpace(model.SearchTextPrevious) ? "" : model.SearchTextPrevious.Trim();

            if (model.SearchText != model.SearchTextPrevious)
            {
                model.IsNewSearch = true;
                if (model.From == "Result") model.ClientFilter = new FilterBy();
            }

            if (model.IsNewSearch) model.Pagination.Paging = "0";

            if (string.IsNullOrWhiteSpace(model.Pagination.Paging))
            {
                model.Pagination.Page = 0;
            }
            else
            {
                switch (model.Pagination.Paging)
                {
                    case "prev":
                        model.Pagination.Page = model.Pagination.Page - 1; //(int)TempData["page"] - 1;
                        break;
                    case "next":
                        model.Pagination.Page = model.Pagination.Page + 1; // (int)TempData["page"] + 1;
                        break;
                    default:
                        model.Pagination.Page = int.Parse(model.Pagination.Paging);
                        break;
                }
            }

            page = model.Pagination.Page;
            leftMostPage = model.Pagination.LeftMostPage;
        }

        private void ConfigureUrl(ref SearchDataModel model)
        {
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();

            string imageUrl = _configuration["ImageUrl"];
            string fileUrl = _configuration["FileUrl"];

            model.ImageUrl = imageUrl;
            model.FileUrl = fileUrl;
        }

        private void ConfigureSearchOptions(ref SearchOptions options, string searchText)
        {
            options.QueryType = SearchQueryType.Full; //SearchQueryType.Simple;
            options.SearchMode = SearchMode.All;
            /*
            if (string.IsNullOrWhiteSpace(searchText))
                options.SearchMode = SearchMode.All;
            else
            {
                var arrWord = searchText.Split(" ");
                if (arrWord.Length > 1) options.SearchMode = SearchMode.Any;
                else options.SearchMode = SearchMode.All;
            }
            */
            //options.SearchMode = SearchMode.Any; //SearchMode.All;

            //options.QueryType = string.IsNullOrWhiteSpace(model.SearchText) ? SearchQueryType.Simple : SearchQueryType.Full;
            //options.SearchMode = string.IsNullOrWhiteSpace(model.SearchText) ? SearchMode.All : SearchMode.Any;
            //options.SearchMode = SearchMode.Any;            
        }
        #endregion

        #region "Client Filters"
        private void GetClientFilters(ref string clientFilters, ref SearchDataModel model)
        {
            var agreementList = new List<string>();
            var catalogueList = new List<string>();
            var categoryList = new List<string>();
            var departmentList = new List<string>();
            var featureList = new List<string>();

            if (model.ClientFilter != null)
            {
                if (!string.IsNullOrWhiteSpace(model.ClientFilter.Agreement)) agreementList = JsonSerializer.Deserialize<string[]>(model.ClientFilter.Agreement).ToList();
                if (!string.IsNullOrWhiteSpace(model.ClientFilter.Catalogue)) catalogueList = JsonSerializer.Deserialize<string[]>(model.ClientFilter.Catalogue).ToList();
                if (!string.IsNullOrWhiteSpace(model.ClientFilter.Category)) categoryList = JsonSerializer.Deserialize<string[]>(model.ClientFilter.Category).ToList();
                if (!string.IsNullOrWhiteSpace(model.ClientFilter.Department)) departmentList = JsonSerializer.Deserialize<string[]>(model.ClientFilter.Department).ToList();
                if (!string.IsNullOrWhiteSpace(model.ClientFilter.Feature)) featureList = JsonSerializer.Deserialize<string[]>(model.ClientFilter.Feature).ToList();
            }

            var statusList = new List<string> { model.Status };
            GetClientFilter(ref clientFilters, statusList, "Agreement/Status");
            GetClientFilter(ref clientFilters, agreementList, "Agreement/SearchText");
            GetClientFilter(ref clientFilters, catalogueList, "Catalogue/Name");
            GetClientFilter(ref clientFilters, categoryList, "Category/Name");
            GetClientFilterDepartment(ref clientFilters, departmentList);
            GetClientFilterFeature(ref clientFilters, featureList);

            model.ClientFilter = new FilterBy();
            model.ClientFilter.Agreement = agreementList != null && agreementList.Any() ? JsonSerializer.Serialize(agreementList) : "";
            model.ClientFilter.Catalogue = catalogueList != null && catalogueList.Any() ? JsonSerializer.Serialize(catalogueList) : "";
            model.ClientFilter.Category = categoryList != null && categoryList.Any() ? JsonSerializer.Serialize(categoryList) : "";
            model.ClientFilter.Department = departmentList != null && departmentList.Any() ? JsonSerializer.Serialize(departmentList) : "";
            model.ClientFilter.Feature = featureList != null && featureList.Any() ? JsonSerializer.Serialize(featureList) : "";
        }

        private void GetClientFilter(ref string clientFilters, List<string> filterList, string field)
        {
            string query = "";

            if (filterList != null && filterList.Any())
            {
                var newFilterList = new List<string>();
                foreach (var item in filterList) newFilterList.Add(field + " eq '" + item + "'");
                query = string.Join(" or ", newFilterList.ToArray());
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = "(" + query + ")";
                clientFilters = string.IsNullOrWhiteSpace(clientFilters) ? query : clientFilters + " and " + query;
            }
        }

        private void GetClientFilterDepartment(ref string clientFilters, List<string> departmentList)
        {
            string departmentQuery = "";

            if (departmentList != null && departmentList.Any())
            {
                var filterList = new List<string>();
                foreach (var item in departmentList) filterList.Add("dp eq '" + item + "'");
                departmentQuery = "Departments/any(dp: " + string.Join(" or ", filterList.ToArray()) + ")";
            }

            if (!string.IsNullOrWhiteSpace(departmentQuery))
                clientFilters += (string.IsNullOrWhiteSpace(clientFilters) ? "" : " and ") + "(" + departmentQuery + ")";
        }

        private void GetClientFilterFeature(ref string clientFilters, List<string> featureList)
        {
            string featureQuery = "";

            if (featureList != null && featureList.Any())
            {
                var newFeatureList = featureList.Select(s => new { Type = s.Substring(0, s.IndexOf(StringHelper.Separator)), Value = s })
                                                .ToList()
                                                .GroupBy(g => new { g.Type })
                                                .Select(c => new { Type = c.Key.Type, Values = c.Select(x => x.Value).ToArray() })
                                                .ToList();

                var featureGroupList = new List<string>();

                foreach (var feature in newFeatureList)
                {
                    var filterList = new List<string>();
                    foreach (var item in feature.Values) filterList.Add("cf eq '" + item + "'");
                    featureGroupList.Add("Features/any(cf: " + string.Join(" or ", filterList.ToArray()) + " )");
                }

                featureQuery = string.Join(" and ", featureGroupList.ToArray());
            }

            if (!string.IsNullOrWhiteSpace(featureQuery))
                clientFilters += (string.IsNullOrWhiteSpace(clientFilters) ? "" : " and ") + "(" + featureQuery + ")";
        }
        #endregion

        #region "Server Filters"
        private void GetServerFilters(ref SearchDataModel model)
        {
            var catalogueServerFilter = GetServerFilter(ref model, "Catalogue/Name", "Catálogos", "catalogue", model.ServerFilter.Catalogue, model.ClientFilter.Catalogue);
            var categoryServerFilter = GetServerFilter(ref model, "Category/Name", "Categorías", "category", model.ServerFilter.Category, model.ClientFilter.Category);
            var departmentServerFilter = GetServerFilter(ref model, "Departments", "Departamento de entrega", "department", model.ServerFilter.Department, model.ClientFilter.Department);

            model.CatalogueFilter = catalogueServerFilter.Item1;
            model.CategoryFilter = categoryServerFilter.Item1;
            model.DepartmentFilter = departmentServerFilter.Item1;

            model.ServerFilter.Catalogue = catalogueServerFilter.Item2;
            model.ServerFilter.Category = categoryServerFilter.Item2;
            model.ServerFilter.Department = departmentServerFilter.Item2;

            GetServerFilterAgreement(ref model);
            GetServerFilterFeature(ref model);
        }

        private List<FilterDataModel> GetServerAgreement(SearchDataModel model)
        {
            int counter = 1;
            var agreementServerList = model.Results.Facets["Agreement/SearchText"].Select(x => new {
                BaseFeature = x.Value.ToString(),
                BaseCount = int.Parse(x.Count.ToString()),
                Type = x.Value.ToString().Substring(0, x.Value.ToString().IndexOf(StringHelper.Separator)),
                Value = x.Value.ToString().Substring(x.Value.ToString().IndexOf(StringHelper.Separator) + 1),
            }).ToList()
                .GroupBy(g => new { g.Type })
                .OrderBy(o => o.Key.Type)
                .Select(s => new FilterDataModel
                {
                    Text = s.Key.Type + " (" + s.Sum(c => c.BaseCount).ToString() + ")",
                    Count = s.Sum(c => c.BaseCount),
                    IsChecked = "",
                    Value = s.Key.Type,
                    Items = s.OrderBy(b => b.Value)
                                .Select(n => new FilterItemModel
                                {
                                    Id = "agreement" + (counter++).ToString(),
                                    Text = n.Value + " (" + n.BaseCount.ToString() + ")",
                                    Count = n.BaseCount,
                                    Value = n.BaseFeature,
                                    IsChecked = ""
                                }).ToList(),
                })
                .OrderBy(o => o.Value)
                .ToList();

            agreementServerList.ForEach(i =>
            {
                i.Items = i.Items.OrderByDescending(o => o.Count).ToList();
            });

            return agreementServerList;
        }

        private void GetServerFilterAgreement(ref SearchDataModel model)
        {
            List<FilterDataModel> masterAgreementList;

            // Determinar si se debe crear una nueva lista maestra o usar la existente.
            if (model.IsNewSearch || string.IsNullOrWhiteSpace(model.ServerFilter.Agreement))
            {
                masterAgreementList = GetServerAgreement(model) ?? new List<FilterDataModel>();
            }
            else
            {
                try
                {
                    masterAgreementList = JsonSerializer.Deserialize<List<FilterDataModel>>(model.ServerFilter.Agreement) ?? new List<FilterDataModel>();
                }
                catch
                {
                    masterAgreementList = GetServerAgreement(model) ?? new List<FilterDataModel>();
                }
            }

            // Obtener los resultados de facetas "frescos" para actualizar contadores.
            var freshFacets = GetServerAgreement(model) ?? new List<FilterDataModel>();
            var freshCountsLookup = freshFacets
                .SelectMany(g => g.Items ?? Enumerable.Empty<FilterItemModel>())
                .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Value))
                .ToDictionary(i => i.Value, i => i.Count, StringComparer.OrdinalIgnoreCase);

            // Obtener la lista de filtros seleccionados por el cliente.
            var clientAgreementList = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.ClientFilter.Agreement))
            {
                try { clientAgreementList = JsonSerializer.Deserialize<List<string>>(model.ClientFilter.Agreement) ?? new List<string>(); }
                catch { clientAgreementList = new List<string>(); }
            }

            // Iterar sobre la lista maestra para actualizar contadores y estado.
            foreach (var group in masterAgreementList)
            {
                if (group?.Items == null) continue;

                foreach (var item in group.Items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Value)) continue;

                    // Actualizar contador.
                    item.Count = freshCountsLookup.TryGetValue(item.Value, out var newCount) ? newCount : 0;
                    var displayText = item.Value.Substring(item.Value.IndexOf(StringHelper.Separator) + 1);
                    item.Text = $"{displayText} ({item.Count})";

                    // Marcar como seleccionado.
                    item.IsChecked = clientAgreementList.Contains(item.Value) ? "checked" : "";
                }

                // Recalcular contadores de grupo y estado.
                // El recuento del grupo ahora refleja el número total de opciones, no las que coinciden con la búsqueda.
                var totalOptionsCount = group.Items.Count();
                group.Count = totalOptionsCount;
                group.Text = $"{group.Value} ({group.Count})";
                group.IsChecked = group.Items.Any(i => i.IsChecked == "checked") ? "menu-open" : "";
            }

            // Asignar y persistir la lista maestra actualizada.
            model.AgreementFilter = masterAgreementList;
            model.ServerFilter.Agreement = JsonSerializer.Serialize(masterAgreementList);
        }

        private List<FilterDataModel> GetServerFeature(SearchDataModel model)
        {
            var counter = 1;
            //var data = model.Results;

            var featureServerList = model.Results.Facets["Features"].Select(x => new
            {
                BaseFeature = x.Value.ToString(),
                BaseCount = int.Parse(x.Count.ToString()),
                Type = x.Value.ToString().Split(StringHelper.Separator)[0],
                Value = x.Value.ToString().Split(StringHelper.Separator)[1],
                FeatureType = x.Value.ToString().Split(StringHelper.Separator)[2],
            }).ToList()
                .GroupBy(g => new { g.Type })
                .OrderBy(o => o.Key.Type)
                .Select(s => new FilterDataModel
                {
                    Text = s.Key.Type + " (" + s.Sum(c => c.BaseCount).ToString() + ")",
                    Count = s.Sum(c => c.BaseCount),
                    IsChecked = "",
                    Value = s.Key.Type,
                    Items = s.OrderBy(b => b.Value)
                                .Select(n => new FilterItemModel
                                {
                                    Id = "feature" + (counter++).ToString(),
                                    Text = n.Value + " (" + n.BaseCount.ToString() + ")",
                                    Count = n.BaseCount,
                                    Value = n.BaseFeature,
                                    FeatureType = n.FeatureType,
                                    IsChecked = ""
                                })
                                .Where(item => item.FeatureType.Equals("GENERICA") || item.FeatureType.Equals("REQUERIDA"))
                                .ToList(),
                })
                .OrderBy(b1 => b1.Value)
                .ToList();

            if (featureServerList != null && featureServerList.Any())
            {
                featureServerList.ForEach(i =>
                {
                    i.Items = i.Items.OrderByDescending(o => o.Count).ToList();
                    i.ShowFeature = (i.Items.Any(item => item.FeatureType.Equals("GENERICA", StringComparison.OrdinalIgnoreCase)) ? 1 : 0);
                });

                featureServerList = featureServerList.OrderByDescending(o => o.Count).ToList();
            }

            return featureServerList;
        }

        private void GetServerFilterFeature(ref SearchDataModel model)
        {
            List<FilterDataModel> masterFeatureList;

            // Determinar si se debe crear una nueva lista maestra o usar la existente.
            if (model.IsNewSearch || string.IsNullOrWhiteSpace(model.ServerFilter.Feature))
            {
                // Es una búsqueda nueva, se genera la lista completa desde los facets.
                masterFeatureList = GetServerFeature(model) ?? new List<FilterDataModel>();
            }
            else
            {
                // Es una búsqueda por filtro, se reutiliza la lista maestra guardada.
                try
                {
                    masterFeatureList = JsonSerializer.Deserialize<List<FilterDataModel>>(model.ServerFilter.Feature) ?? new List<FilterDataModel>();
                }
                catch
                {
                    // Fallback: si la deserialización falla, regenerar.
                    masterFeatureList = GetServerFeature(model) ?? new List<FilterDataModel>();
                }
            }

            // Obtener los resultados de facetas "frescos" para actualizar contadores.
            var freshFacets = GetServerFeature(model) ?? new List<FilterDataModel>();
            var freshCountsLookup = freshFacets
                .SelectMany(g => g.Items ?? Enumerable.Empty<FilterItemModel>())
                .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Value))
                .ToDictionary(i => i.Value, i => i.Count, StringComparer.OrdinalIgnoreCase);

            // Obtener la lista de filtros seleccionados por el cliente.
            var clientFeatureList = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.ClientFilter?.Feature))
            {
                try { clientFeatureList = JsonSerializer.Deserialize<List<string>>(model.ClientFilter.Feature) ?? new List<string>(); }
                catch { clientFeatureList = new List<string>(); }
            }

            // Iterar sobre la lista maestra para actualizar contadores y estado.
            foreach (var group in masterFeatureList)
            {
                if (group?.Items == null) continue;

                foreach (var item in group.Items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Value)) continue;

                    // Actualizar contador (es 0 si no está en los resultados frescos).
                    item.Count = freshCountsLookup.TryGetValue(item.Value, out var newCount) ? newCount : 0;

                    var displayText = item.Value.Split(StringHelper.Separator)[1];
                    item.Text = $"{displayText} ({item.Count})";

                    // Marcar como seleccionado si está en la lista del cliente.
                    item.IsChecked = clientFeatureList.Contains(item.Value) ? "checked" : "";
                }

                // Recalcular contadores de grupo y estado.
                group.Count = group.Items.Sum(i => i.Count);
                group.Text = $"{group.Value} ({group.Count})";
                group.IsChecked = group.Items.Any(i => i.IsChecked == "checked") ? "menu-open" : "";
                group.Items = group.Items.OrderByDescending(i => i.Count).ToList();
            }

            masterFeatureList = masterFeatureList.OrderByDescending(g => g.Count).ToList();

            // Asignar y persistir la lista maestra actualizada.
            model.FeatureFilter = masterFeatureList;
            model.ServerFilter.Feature = JsonSerializer.Serialize(masterFeatureList);
        }

        private (FilterDataModel, string) GetServerFilter(ref SearchDataModel model, string field, string textName, string itemName, string serverFilter, string clientFilter)
        {
            var filterModel = new FilterDataModel();
            var filterServerList = new List<string[]>();
            var clientFilterList = new List<string>();

            if (!string.IsNullOrWhiteSpace(clientFilter)) clientFilterList = JsonSerializer.Deserialize<List<string>>(clientFilter);

            filterServerList = model.Results.Facets[field].Select(x => new string[] { x.Value.ToString(), x.Count.ToString() }).OrderBy(o => o[0]).ToList();

            if (filterServerList != null)
            {
                var filterCount = model.Results.Facets[field].Select(x => new { Name = x.Value.ToString(), Count = x.Count.ToString() }).ToList();
                filterModel = new FilterDataModel { Text = $"{textName} ({filterServerList.Where(w => int.Parse(w[1]) > 0).ToList().Count})" };

                for (var c = 0; c < filterServerList.Count; c++)
                {
                    var isChecked = "";
                    var itemId = itemName + c.ToString();
                    var itemText = filterServerList[c][0];
                    int itemCount = 0;

                    var count = filterCount.Where(w => w.Name == filterServerList[c][0]).Select(s => s.Count).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(count)) itemCount = int.Parse(count);

                    if (clientFilterList != null && clientFilterList.Any())
                        isChecked = clientFilterList.Contains(filterServerList[c][0]) ? "checked" : "";

                    var item = new FilterItemModel { Id = itemId, Text = itemText, IsChecked = isChecked, Count = itemCount };
                    filterModel.Items.Add(item);
                }
            }

            filterModel.Items = filterModel.Items.OrderByDescending(o => o.Count).ToList();
            var filters = filterServerList != null ? JsonSerializer.Serialize(filterServerList) : "";

            return (filterModel, filters);
        }

        public void GetOtherFeatures(ref SearchDataModel model, List<FilterDataModel> otherFeatures)
        {
            // Si no hay otros features calculados o no hay featureFilter, no hacemos nada
            if (otherFeatures == null || !otherFeatures.Any() || model.FeatureFilter == null || !model.FeatureFilter.Any())
                return;

            // Mapa rápido de Value -> FilterItemModel (desde otherFeatures)
            var otherMap = new Dictionary<string, FilterItemModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var of in otherFeatures)
            {
                if (of.Items == null) continue;
                foreach (var it in of.Items)
                {
                    if (it == null || string.IsNullOrWhiteSpace(it.Value)) continue;
                    otherMap[it.Value] = new FilterItemModel
                    {
                        Id = it.Id,
                        Text = it.Text,
                        Count = it.Count,
                        IsChecked = it.IsChecked,
                        Value = it.Value,
                        FeatureType = it.FeatureType
                    };
                }
            }

            // Lista de features seleccionados por el cliente (para mantener IsChecked si corresponde)
            var clientFeatureList = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.ClientFilter?.Feature))
            {
                try { clientFeatureList = JsonSerializer.Deserialize<List<string>>(model.ClientFilter.Feature) ?? new List<string>(); }
                catch { clientFeatureList = new List<string>(); }
            }

            // Actualizamos únicamente los items que aparecen en otherMap, sin poner todo a cero
            foreach (var ff in model.FeatureFilter)
            {
                if (ff.Items == null) continue;

                foreach (var item in ff.Items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Value)) continue;

                    if (otherMap.TryGetValue(item.Value, out var newItem))
                    {
                        // Actualizamos count y texto con la información real obtenida
                        item.Count = newItem.Count;

                        var displayText = item.Value;
                        var idx = displayText.IndexOf(StringHelper.Separator);
                        if (idx >= 0 && idx + 1 < displayText.Length) displayText = displayText.Substring(idx + 1);
                        item.Text = $"{displayText} ({item.Count})";

                        // Mantener checked según cliente si aplica; fallback al valor recibido
                        item.IsChecked = clientFeatureList.Contains(item.Value) ? "checked" : newItem.IsChecked ?? "";
                    }
                    else
                    {
                        // Si otherMap no contiene el item, dejamos su Count como estaba
                        var displayText = item.Value;
                        var idx = displayText.IndexOf(StringHelper.Separator);
                        if (idx >= 0 && idx + 1 < displayText.Length) displayText = displayText.Substring(idx + 1);
                        item.Text = $"{displayText} ({item.Count})";
                        item.IsChecked = clientFeatureList.Contains(item.Value) ? "checked" : item.IsChecked ?? "";
                    }
                }

                // Recalcular totales del grupo usando los counts actualizados
                ff.Count = ff.Items.Sum(i => i.Count);
                ff.Text = $"{ff.Value} ({ff.Count})";
                // Marcar grupo abierto si alguno de sus items está checked
                ff.IsChecked = ff.Items.Any(i => i.IsChecked == "checked") ? "menu-open" : "";
            }
        }
        #endregion

        #region "Run Searchs"
        public async Task<List<FilterItemModel>> SearchByAgreementStatus(SearchDataModel model, string status)
        {
            string searchText = string.IsNullOrWhiteSpace(model.SearchText) ? "" : model.SearchText.Trim();
            searchText = StringHelper.RemoveDiacritics(searchText);

            ConfigureSearch();

            string filter = $"(Agreement/Status eq '{status}')";

            var searchResult = await ExecuteSearchAsync(searchText, filter, 0);
            model.Results = searchResult.Results;

            if (model.Results == null)
            {
                return new List<FilterItemModel>();
            }

            var facets = model.Results.Facets["Agreement/SearchText"].Select(x => new { Value = x.Value.ToString(), Count = x.Count.ToString() }).ToList();

            var agreementList = new List<FilterItemModel>();

            facets.ForEach(f =>
            {
                var text = f.Value.Substring(f.Value.IndexOf(StringHelper.Separator) + 1);
                var item = new FilterItemModel
                {
                    Id = text.Substring(0, text.IndexOf(" ")),
                    Text = text.Substring(text.IndexOf(" ") + 1),
                    Count = string.IsNullOrWhiteSpace(f.Count) ? 0 : int.Parse(f.Count),
                    Value = f.Value.ToString().Trim()
                };

                agreementList.Add(item);
            });

            return agreementList;
        }

        public async Task<List<PublicoProductDocument>> DownLoadAsync(SearchDataModel model)
        {
            string searchText = string.IsNullOrWhiteSpace(model.SearchText) ? "" : model.SearchText.Trim();
            searchText = StringHelper.RemoveDiacritics(searchText);

            var filters = "";

            ConfigureSearch();

            model.SearchText = string.IsNullOrWhiteSpace(model.SearchText) ? "" : model.SearchText.Trim();
            model.SearchTextPrevious = string.IsNullOrWhiteSpace(model.SearchTextPrevious) ? "" : model.SearchTextPrevious.Trim();

            if (model.SearchText != model.SearchTextPrevious)
            {
                model.IsNewSearch = true;
                if (model.From == "Result") model.ClientFilter = new FilterBy();
            }

            ConfigureUrl(ref model);
            GetClientFilters(ref filters, ref model);

            var (results, _) = await ExecuteSearchAsync(searchText, filters, 0, 99999);

            var list = new List<PublicoProductDocument>();
            if (results != null)
            {
                var items = results.GetResults();
                foreach (SearchResult<PublicoProductDocument> item in items)
                {
                    list.Add(item.Document);
                }
            }

            return list;
        }

        public async Task<float[]> GetEmbeddingAsync(string text, string mode = "query")
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            var payload = new { text, mode };

            using var resp = await _http.PostAsJsonAsync("/embed", payload).ConfigureAwait(false);

            // Manejo básico de errores conocidos
            if ((int)resp.StatusCode == 413)
                throw new InvalidOperationException("El texto excede el límite permitido por la API.");
            if ((int)resp.StatusCode == 429)
                throw new InvalidOperationException("Rate limit excedido. Reintentar según Retry-After.");
            if ((int)resp.StatusCode >= 400)
            {
                var error = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException($"Error {resp.StatusCode}: {error}");
            }

            // Validar content-type
            var ct = resp.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(ct, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Content-Type inesperado: {ct}");

            var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            if (bytes is null || bytes.Length == 0) return Array.Empty<float>();

            // Validar dimensión desde cabecera
            if (resp.Headers.TryGetValues("X-Embedding-Dim", out var dims) &&
                int.TryParse(dims.FirstOrDefault(), out var dimHeader))
            {
                if (dimHeader != 768)
                    throw new InvalidOperationException($"Dimensión inesperada: {dimHeader}. Se esperaba 768.");
            }
            else
            {
                // Fallback por tamaño
                if (bytes.Length % sizeof(float) != 0)
                    throw new InvalidOperationException($"Tamaño de payload inválido: {bytes.Length} bytes.");
                var inferredDim = bytes.Length / sizeof(float);
                if (inferredDim != 768)
                    throw new InvalidOperationException($"Dimensión inferida {inferredDim} distinta de 768.");
            }

            // Convertir bytes → float[]
            float[] result = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

            return result;
        }

        // ================== Helpers ==================

        private SearchOptions BuildBaseOptions(string filters, int page, int? size = null, bool isHybrid = false)
        {
            var options = new SearchOptions
            {
                Filter = filters,
                Skip = page * GlobalVariables.ResultsPerPage,
                Size = size ?? GlobalVariables.ResultsPerPage,
                IncludeTotalCount = true,

                // --- AFINAMIENTO BM25 para consultas largas (para búsquedas Keyword e Híbrida) ---
                // QueryType.Full habilita la sintaxis de Lucene (necesaria para SearchMode.All)
                QueryType = SearchQueryType.Full,
                // SearchMode.All requiere que TODAS las palabras de la consulta coincidan
                // en el documento para ser rankeado. Esto reduce las coincidencias de baja calidad.
                SearchMode = SearchMode.All
            };

            // Campos devueltos (Se mantienen los campos existentes para consistencia)
            options.Select.Add("Id");
            options.Select.Add("Name");
            options.Select.Add("Image");
            options.Select.Add("File");
            options.Select.Add("PublishedDate");
            options.Select.Add("UpdatedDate");
            options.Select.Add("Status");
            options.Select.Add("Agreement/Name");
            options.Select.Add("Catalogue/Name");
            options.Select.Add("Category/Name");
            options.Select.Add("FeatureTypeList");
            options.Select.Add("SearchText");

            // Facets válidos (Necesarios para que el servidor los calcule sobre el resultado final RRF)
            options.Facets.Add("Agreement/SearchText,count:2000");
            options.Facets.Add("Catalogue/Name,count:100");
            options.Facets.Add("Category/Name,count:1000");
            options.Facets.Add("Departments,count:100");
            options.Facets.Add("Features,count:10000");

            return options;
        }
        private async Task<(SearchResults<PublicoProductDocument> Results, bool IsHybrid)> RunKeywordSearchAsync(string query, string filters, int page, int? size = null)
        {
            var options = BuildBaseOptions(filters, page, size);
            var resp = await _searchClient.SearchAsync<PublicoProductDocument>(query, options).ConfigureAwait(false);
            return (resp.Value, false);
        }

        // ====== Helpers deterministas (añadir a tu clase) ======
        private static string MakeKey(string query, string filters)
        {
            // clave estable por búsqueda (no incluye 'page')
            query = (query ?? string.Empty).Trim().ToLowerInvariant();
            filters = (filters ?? string.Empty).Trim().ToLowerInvariant();
            return $"{query}|{filters}";
        }

        // NOTA: Debes declarar o tener disponible el Dictionary estático para el cache de embeddings.
        private static readonly ConcurrentDictionary<string, float[]> _embeddingCache = new();
        // El cache de 'keywordSize' ya NO es necesario, pero se mantiene 'embeddingCache'.


        // Genera un tamaño para keyword (Size) aleatorio y sesgado por #palabras.
        // - Más palabras (→maxWords): el tamaño tiende al máximo (maxSize).
        // - Menos palabras (→minWords): el tamaño tiende al mínimo (minSize).

        private static int SampleKeywordSize(
            int wordCount,
            int minWords = 3,
            int maxWords = 10,
            int minSize = 49,
            int maxSize = 997,
            Random? rng = null)
        {
            rng ??= new Random();

            // Normaliza wordCount a [0,1]
            if (maxWords <= minWords) maxWords = minWords + 1;
            double t = (wordCount - minWords) / (double)(maxWords - minWords);
            t = Math.Clamp(t, 0d, 1d);

            // Exponente de sesgo: con más palabras (t→1) elevamos U a potencia mayor → valor pequeño (cerca de minSize)
            double p = 1.5 + 3.5 * t;          // ∈ [1.5, 5.0]
            double u = rng.NextDouble();       // U ~ (0,1)

            // Cuando p es pequeño, skew es pequeño (cerca de 0).
            //double skew = 1 - Math.Pow(1 - u, p);
            double skew = Math.Pow(u, p);      // sesgo hacia 0 cuando t es grande

            // Interpolación entre minSize y maxSize (evita extremos exactos)
            double raw = minSize + skew * (maxSize - minSize);
            int size = (int)Math.Round(raw);
            size = Math.Clamp(size, minSize + 1, maxSize - 1);

            return Math.Max(1, size);
        }

        private static int MakeDeterministicSeed(string key)
        {
            // seed estable a partir de SHA256(key)
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
            // toma 4 bytes para un Int32
            int seed = BitConverter.ToInt32(hash, 0);
            return seed == 0 ? 1 : seed; // evita 0
        }
        /// <summary>
        /// Ejecuta una búsqueda híbrida unificada, delegando la fusión y el re-ranking (RRF) al servidor.
        /// </summary>
        public async Task<SearchDataModel> RunQueryAsync(SearchDataModel model)
        {
            string searchText = string.IsNullOrWhiteSpace(model.SearchText) ? "" : model.SearchText.Trim();
            searchText = StringHelper.RemoveDiacritics(searchText);

            var filters = "";
            int page = 0;
            int leftMostPage = 0;

            ConfigureSearch();
            ConfigurePagination(ref model, ref page, ref leftMostPage);
            ConfigureUrl(ref model);
            GetClientFilters(ref filters, ref model);

            var otherFeatures = new List<FilterDataModel>();
            if (model.ClientFilter.Feature != "[]" && !string.IsNullOrWhiteSpace(model.ClientFilter.Feature))
            {
                otherFeatures = await RunQueryForOtherFeaturesAsync(model);
            }

            var (results, isHybrid) = await ExecuteSearchAsync(searchText, filters, page);
            model.Results = results;

            if (model.Results == null)
            {
                // If there are no results, return the model without further processing.
                // This prevents null reference exceptions in pagination and filtering.
                return model;
            }

            // ===== Paginación =====
            model.Pagination.IsHybridPagination = isHybrid;

            long totalCount = model.Results?.TotalCount ?? 0L;

            if (isHybrid)
            {
                // Para búsquedas híbridas, el TotalCount es inherentemente inexacto.
                // La bandera IsHybridPagination indicará al frontend que use una paginación simple (Anterior/Siguiente).
            }

            model.Pagination.PageCount = (int)((totalCount + GlobalVariables.ResultsPerPage - 1) / GlobalVariables.ResultsPerPage);
            model.Pagination.CurrentPage = page;

            if (page == 0) leftMostPage = 0;
            else if (page <= leftMostPage)
                leftMostPage = Math.Max(page - GlobalVariables.PageRangeDelta, 0);
            else if (page >= leftMostPage + GlobalVariables.MaxPageRange - 1)
                leftMostPage = Math.Min(page - GlobalVariables.PageRangeDelta, model.Pagination.PageCount - GlobalVariables.MaxPageRange);

            model.Pagination.LeftMostPage = leftMostPage;
            model.Pagination.PageRange = Math.Min(model.Pagination.PageCount - leftMostPage, GlobalVariables.MaxPageRange);
            model.Pagination.Page = page;
            model.SearchTextPrevious = model.SearchText;

            GetServerFilters(ref model);
            GetOtherFeatures(ref model, otherFeatures);

            return model;
        }

        private async Task<(SearchResults<PublicoProductDocument> Results, bool IsHybrid)> RunHybridSearchAsync(
            string query,
            string filters,
            int page,
            int? size = null,
            string searchText = "",
            double vectorSimilarityThreshold = 0.83999
        )
        {
            // --- 1) Opciones base (incluye paginación final y facets) ---
            var searchOptions = BuildBaseOptions(filters, page, size, isHybrid: true);

            // --- 2) Embedding vectorial (usando cache) ---
            var embeddingKey = MakeKey(query, string.Empty);
            float[] embedding = null;

            string nombreArchivo = "Log.WebApp.Publico-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            string prefix = "Query: ";

            try
            {
                if (!_embeddingCache.TryGetValue(embeddingKey, out embedding))
                {
                    // Intentar obtener embedding
                    embedding = await GetEmbeddingAsync(query).ConfigureAwait(false);
                    _embeddingCache[embeddingKey] = embedding;
                    FileLog.GuardarArchivo(nombreArchivo, prefix + searchText);
                }
            }
            catch (Exception ex)
            {
                // Si GetEmbeddingAsync falla (ej. 400/413, timeout o red), registramos y caemos a keyword.
                // Esto evita que la app se rompa por completo.
                Console.WriteLine($"[WARN] Falla al obtener embedding: {ex.Message}");
                return await RunKeywordSearchAsync(query, filters, page, size);
            }

            // Si no hay embedding (ej. query inválida), caemos a keyword
            if (embedding is null || embedding.Length == 0)
            {
                return await RunKeywordSearchAsync(query, filters, page, size);
            }

            // --- 3) Determinación del tamaño de KNN ---
            int wordCount = System.Text.RegularExpressions.Regex.Matches(query ?? string.Empty, @"\S+").Count;
            string key = MakeKey(query, filters);

            if (!_kwSizeCache.TryGetValue(key, out int Knn))
            {
                var seed = MakeDeterministicSeed(key);
                var rng = new Random(seed);
                Knn = SampleKeywordSize(wordCount: wordCount, minSize: 48, maxSize: 120);//minSize: 200, maxSize: 400/ minSize: 48, maxSize: 72
                _kwSizeCache[key] = Knn;
            }

            // --- 4) Configuración de Búsqueda Vectorial (KNN) ---
            searchOptions.VectorSearch = new VectorSearchOptions
            {
                Queries =
        {
            new VectorizedQuery(embedding)
            {
                KNearestNeighborsCount = Knn,
                Fields = { "ProductArray" },
                // Threshold opcional (comentado)
                Threshold = new VectorSimilarityThreshold(vectorSimilarityThreshold)
            }
        }
            };

            // --- 5) Ejecutar búsqueda híbrida ---
            try
            {
                var resp = await _searchClient.SearchAsync<PublicoProductDocument>(query, searchOptions).ConfigureAwait(false);
                return (resp.Value, true);
            }
            catch (Exception ex)
            {
                // Si Azure Search falla (transitorio u otro), registramos y caemos a keyword
                Console.WriteLine($"[WARN] Falla en Azure Search: {ex.Message}");
                return await RunKeywordSearchAsync(query, filters, page, size);
            }
        }

        private async Task<(SearchResults<PublicoProductDocument> Results, bool IsHybrid)> ExecuteSearchAsync(string searchText, string filters, int page, int? size = null)
        {
            string nombreArchivo = "Log.WebApp.Publico-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            int wordCount = System.Text.RegularExpressions.Regex.Matches(searchText, @"\S+").Count;

            (SearchResults<PublicoProductDocument> Results, bool IsHybrid) result = (null, false);

            try
            {
                var canonicalQuery = _normalizer.Normalize(searchText);
                var queryForBoth = canonicalQuery;

                if (string.IsNullOrWhiteSpace(queryForBoth) || queryForBoth.Length > 200)
                {
                    result = await RunKeywordSearchAsync(queryForBoth, filters, page, size);
                }
                else if (wordCount == 1)
                {
                    result = await RunKeywordSearchAsync(queryForBoth, filters, page, size);
                }
                else
                {
                    result = await RunHybridSearchAsync(queryForBoth, filters, page, size, searchText);
                }
            }
            catch (Exception e)
            {
                FileLog.GuardarArchivo(nombreArchivo, "Error: " + e.Message + " - " + e.Source);
            }

            return result;
        }

        public async Task<List<FilterDataModel>> RunQueryForOtherFeaturesAsync(SearchDataModel model)
        {
            string searchText = string.IsNullOrWhiteSpace(model.SearchText) ? "" : model.SearchText.Trim();
            searchText = StringHelper.RemoveDiacritics(searchText);

            var filters = "";
            int page = 0;
            int leftMostPage = 0;
            ConfigurePagination(ref model, ref page, ref leftMostPage);
            GetClientFilters(ref filters, ref model);

            model.Pagination.Page = 0;
            model.Pagination.LeftMostPage = 0;

            var searchResult = await ExecuteSearchAsync(searchText, filters, page);
            model.Results = searchResult.Results;
            model.SearchTextPrevious = searchText;

            var features = new List<string>();

            //model.Results.GetResults().ToList().ForEach(d => features.AddRange(d.Document.Features));

            if (model.Results != null)
            {
                var results = model.Results.GetResults();

                foreach (var result in results)
                {
                    if (result.Document.Features != null)
                    {
                        features.AddRange(result.Document.Features);
                    }
                }
            }

            int fsCounter = 1;
            var featureList = JsonSerializer.Deserialize<List<string>>(model.ClientFilter.Feature);
            var newFeatureList = features.Select(s => new
            {
                Type = s.Substring(0, s.IndexOf(StringHelper.Separator)),
                Text = s.Substring(s.IndexOf(StringHelper.Separator) + 1),
                Value = s
            }).ToList()
              .GroupBy(g => new { g.Type })
              .Select(f => new FilterDataModel
              {
                  Value = f.Key.Type,
                  Text = f.Key.Type + " (0)",
                  Count = 0,
                  IsChecked = "",
                  Items = f.GroupBy(g2 => new { g2.Text })
                           .Select(f2 => new FilterItemModel
                           {
                               Id = "feature" + (fsCounter++).ToString(),
                               Text = f2.Key.Text + " (" + f2.Count().ToString() + ")",
                               IsChecked = featureList.Contains(f2.First().Value) ? "checked" : "",
                               Value = f2.First().Value,
                               Count = f2.Count()
                           }).ToList()
              }).OrderBy(o => o.Value).ToList();

            newFeatureList.ForEach(n =>
            {
                int count = n.Items.Select(s => s.Count).Sum();
                n.Count = count;
                n.Text = n.Value + " (" + count.ToString() + ")";
            });

            return newFeatureList;
        }


        #endregion

    }
}