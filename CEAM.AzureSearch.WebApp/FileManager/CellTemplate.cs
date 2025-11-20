using System;
using static CEAM.AzureSearch.WebApp.Utils.ExcelUtil;

namespace CEAM.AzureSearch.WebApp.FileManager
{
    public class CellTemplate
    {
        public Object Value { get; set; }
        public CellDataType Type { get; set; }
        public string Format { get; set; }
        public bool AllowEmpty { get; set; }
        public int? MergeX { get; set; }
        public int? MergeY { get; set; }
    }
}
