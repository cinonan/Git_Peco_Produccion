using AzureSearch.Models.Publico.Models;
using AzureSearch.Models.Publico.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEAM.AzureSearch.WebApp.Services
{
    public interface IAzureSearchService
    {
        public void GetOtherFeatures(ref SearchDataModel model, List<FilterDataModel> otherFeatures);
        public Task<List<FilterItemModel>> SearchByAgreementStatus(SearchDataModel model, string status);
        public Task<List<PublicoProductDocument>> DownLoadAsync(SearchDataModel model);
        public Task<SearchDataModel> RunQueryAsync(SearchDataModel model);
        public Task<List<FilterDataModel>> RunQueryForOtherFeaturesAsync(SearchDataModel model);
        public Task<float[]> GetEmbeddingAsync(string text, string mode = "query");
    }
}
