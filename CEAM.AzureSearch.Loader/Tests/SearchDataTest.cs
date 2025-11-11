using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using CEAM.AzureSearch.Models.Documents;
using CEAM.AzureSearch.Models.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEAM.AzureSearch.Loader.Tests
{
    public class SearchDataTest
    {
        #region "Properties"
        private static SearchIndexClient _indexClient;
        private static SearchClient _searchClient;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;
        #endregion

        #region "Configuration"
        private void ConfigureSearch()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            string searchServiceUri = configuration["AzureSearch:Test:Uri"];
            string queryApiKey = configuration["AzureSearch:Test:QueryApiKey"];
            string indexName = configuration["AzureSearch:Test:IndexName"];

            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(queryApiKey));
            _searchClient = _indexClient.GetSearchClient(indexName);
        }
        #endregion

        #region "Private Methods"
        private Tuple<string, string, List<Tuple<string, string[]>>> GetIds(string searchText)
        {
            string catalogueId = "";
            string categoryId = "";
            var features = new List<Tuple<string, string[]>>();

            var arr01 = searchText.Split('¬');
            var arr02 = arr01[4].Split('^');

            foreach (var item in arr02)
            {
                var arr03 = item.Split('-');
                var arr04 = arr03[1].Split(',');
                features.Add(new Tuple<string, string[]>(arr03[0], arr04));
            }

            catalogueId = arr01[0];
            categoryId = arr01[1];

            return new Tuple<string, string, List<Tuple<string, string[]>>>(catalogueId, categoryId, features);
        }
        private string SerializeProductSheets(SearchDataModel model)
        {
            string text = "";
            string sepProductSheet = "¬";
            string sepField = "^";
            string sepFeature = "¯"; 
            string sepFeatureType = "|";
            string sepFeatureValue = ",";
            
            var list = model.Results.GetResults().ToList();

            for(var c = 0; c <= list.Count - 1; c++)
            {
                var marca = list[c].Document.FeatureTypeList.Where(w => w.Text == "MARCA").Select(s => string.Join(" ", s.Values.Select(s2 => s2.Text).ToArray())).FirstOrDefault();
                var modelo = list[c].Document.FeatureTypeList.Where(w => w.Text == "MODELO").Select(s => string.Join(" ", s.Values.Select(s2 => s2.Text).ToArray())).FirstOrDefault();
                var marcaModelo = ((string.IsNullOrWhiteSpace(marca) ? "" : marca) + " " + (string.IsNullOrWhiteSpace(modelo) ? "" : modelo)).Trim();

                var arrFeatureNo = list[c].Document.FeatureTypeList.Where(w => w.IsRequiredSubValue == "NO")
                                                                    .Select(s => s.Text + sepFeatureType +
                                                                        string.Join(sepFeatureValue, s.Values.Select(s2 => s2.Text).ToArray())).ToArray();

                var arrFeatureYes = list[c].Document.FeatureTypeList.Where(w => w.IsRequiredSubValue == "SI")
                                                                    .Select(s => s.Text + sepFeatureType +
                                                                        string.Join(sepFeatureValue, s.Values.Select(s2 => s2.Text).ToArray())).ToArray();

                var featureNo = (arrFeatureNo.Any() ? string.Join(sepFeature, arrFeatureNo) : "");
                var featureYes = (arrFeatureYes.Any() ? string.Join(sepFeature, arrFeatureYes) : "");

                text += !string.IsNullOrWhiteSpace(text) ? sepProductSheet : "";
                text += list[c].Document.Id + sepField;
                text += marcaModelo + sepField;
                text += list[c].Document.Name + sepField;
                text += list[c].Document.Image + sepField;
                text += list[c].Document.File + sepField;
                text += list[c].Document.Category.Name + sepField;
                text += list[c].Document.Category.Id + sepField;                
                text += featureNo + sepField;
                text += featureYes + sepField;
            }

            return text;
        }
        #endregion

        #region "Public Methods"
        public async Task<string> SearchAsync(SearchDataModel model, string filterText) //"70¬10615¬agua demo¬1000¬6177-251458^6178-251929"
        {
            //"126¬11059¬portatil demo¬1000¬12616-536551,536631^12617-536665,536680,536658^12618-536702,536703^12633-537033,537034,537035,537036"
            //filterText = "126¬11059¬portatil demo¬1000¬12616-536551,536631^12617-536665,536680,536658^12618-536702,536703^12633-537033";
            filterText = "60¬10542¬cable demo¬1000¬4216-158273,154105^4219-154125,154126^4225-154133,154191,181161";

            var ids = GetIds(filterText);

            ConfigureSearch();

            var filters = "";
            filters += "(Catalogue/Id eq '" + ids.Item1 + "') and ";
            filters += "(Category/Id eq '" + ids.Item2 + "')";

            if (ids.Item3 != null && ids.Item3.Any())
            {                
                foreach (var type in ids.Item3)
                {
                    var featureFilter = "(FeatureTypeList/any(ft: ft/Id eq '" + type.Item1 + "' and ft/Values/any(fti: search.in(fti/Id, '" + string.Join(",", type.Item2) + "'))))";
                    filters += string.IsNullOrWhiteSpace(filters) ? "" : " and ";
                    filters += featureFilter;
                }
            }

            var options = new SearchOptions
            {
                QueryType = SearchQueryType.Simple,
                Filter = filters,
                SearchMode = SearchMode.All,
                IncludeTotalCount = true
            };

            model.Results = await _searchClient.SearchAsync<ProductSheetDocument>("", options).ConfigureAwait(false);

            var text = SerializeProductSheets(model);
            return text;
        }
        #endregion
    }
}
