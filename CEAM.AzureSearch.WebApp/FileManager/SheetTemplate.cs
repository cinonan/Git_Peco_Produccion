using ClosedXML.Excel;
using System.Collections.Generic;

namespace CEAM.AzureSearch.WebApp.FileManager
{
    public class SheetTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<List<CellTemplate>> Header { get; set; }
        public List<object[]> Body { get; set; }
        public bool IsAutoFilter { get; set; }
        public XLColor TabColor { get; set; }
    }
}
