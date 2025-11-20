using CEAM.AzureSearch.WebApp.Models;
using System.Collections.Generic;
using System.IO;

namespace CEAM.AzureSearch.WebApp.FileManager
{
    public interface IExcelBase
    {
        BaseResponse<MemoryStream> GenerateWorkBook(List<SheetTemplate> sheetList);
    }
}
