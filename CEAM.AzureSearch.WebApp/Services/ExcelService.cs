using AzureSearch.Models.Publico.Documents;
using CEAM.AzureSearch.WebApp.FileManager;
using CEAM.AzureSearch.WebApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CEAM.AzureSearch.WebApp.Utils.ExcelUtil;

namespace CEAM.AzureSearch.WebApp.Services
{
    public class ExcelService: IExcelService
    {
        private IExcelBase _excelBase { get; set; }

        public ExcelService(IExcelBase excelBase)
        {
            _excelBase = excelBase;
        }


        public async Task<BaseResponse<MemoryStream>> GenerateExcel(List<PublicoProductDocument> list)
        {
            var sheets = new List<SheetTemplate>();
            sheets.Add(new SheetTemplate { Id = 1, Name = "Reporte", IsAutoFilter = true, Header = GetHeader(), Body = GetBody(list) });
            var stream = _excelBase.GenerateWorkBook(sheets);
            return stream;
        }

        private List<List<CellTemplate>> GetHeader()
        {
            var headers = new List<List<CellTemplate>>();
            var header = new List<CellTemplate>();
            header.Add(new CellTemplate { Value = "Acuerdo Marco", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Catálogo", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Categoría", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Descripción Ficha-Producto", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Marca", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Nro. Parte o Código Único de Identificación", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Ficha Técnica", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Imagen", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            header.Add(new CellTemplate { Value = "Estado Ficha - Producto", Type = CellDataType.Text, Format = null, AllowEmpty = false });
            headers.Add(header);

            return headers;
        }

        private List<object[]> GetBody(List<PublicoProductDocument> list)
        {
            var rows = new List<object[]>();

            if (list != null && list.Any())
            {
                list.ForEach(x =>
                {
                    var marca = "";
                    var parte = "";

                    marca = x.FeatureTypeList.Where(a => a.Text.ToUpper() == "MARCA").Select(a => a.Values.FirstOrDefault().Text).FirstOrDefault();
                    parte = ExtractPartNumber(x.Name);

                    rows.Add(new object[]{
                        x.Agreement.Name,
                        x.Catalogue.Name,
                        x.Category.Name,
                        x.Name,
                        string.IsNullOrWhiteSpace(marca) ? "" : marca,
                        string.IsNullOrWhiteSpace(parte) ? "" : parte,
                        x.File,
                        x.Image,
                        x.Status
                    });
                });
            }

            return rows;
        }

        private string ExtractPartNumber(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "";
            }

            string[] keywords = { "nro parte", "codigo de identificacion unico" };
            description = description.ToLower();

            foreach (var keyword in keywords)
            {
                int keywordIndex = description.IndexOf(keyword);
                if (keywordIndex != -1)
                {
                    int startIndex = keywordIndex + keyword.Length;

                    // Skip any separator characters like ':' or '-'
                    while (startIndex < description.Length && (description[startIndex] == ':' || description[startIndex] == '-' || char.IsWhiteSpace(description[startIndex])))
                    {
                        startIndex++;
                    }

                    int endIndex = description.IndexOf(',', startIndex);
                    if (endIndex == -1)
                    {
                        endIndex = description.Length;
                    }

                    string result = description.Substring(startIndex, endIndex - startIndex).Trim();
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return "";
        }
    }
}
