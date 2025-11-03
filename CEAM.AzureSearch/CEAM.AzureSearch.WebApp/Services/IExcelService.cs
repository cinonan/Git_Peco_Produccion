using AzureSearch.Models.Publico.Documents;
using CEAM.AzureSearch.WebApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CEAM.AzureSearch.WebApp.Services
{
    public interface IExcelService
    {
        Task<BaseResponse<MemoryStream>> GenerateExcel(List<PublicoProductDocument> list);
    }
}
