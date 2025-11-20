using CEAM.AzureSearch.WebApp.Models;
using CEAM.AzureSearch.WebApp.Utils;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static CEAM.AzureSearch.WebApp.Utils.ExcelUtil;

namespace CEAM.AzureSearch.WebApp.FileManager
{
    public class ExcelBase: IExcelBase
    {
        private readonly IConfiguration _configuration;

        public ExcelBase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BaseResponse<MemoryStream> GenerateWorkBook(List<SheetTemplate> sheetList)
        {
            var stream = new MemoryStream();
            var response = new BaseResponse<MemoryStream>();

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    foreach (var sheet in sheetList)
                    {
                        IXLWorksheet worksheet = workbook.AddWorksheet();
                        worksheet.Name = sheet.Name;
                        worksheet.Style.Font.SetFontSize(9).Font.SetFontName("Calibri");

                        if (sheet.TabColor != null) worksheet.TabColor = sheet.TabColor;

                        int colCount = 0;
                        int rowCount = 0;

                        if (sheet.Header != null && sheet.Header.Any())
                        {
                            //worksheet.SheetView.Freeze(1, 0);
                            var header = sheet.Header.Select(x => x.Select(a => a.Value).ToArray()).ToList();

                            worksheet.Cell(1, 1).InsertData(header);

                            //Merge cells
                            foreach (var rowHeader in sheet.Header)
                            {
                                rowCount++;
                                colCount = 0;
                                foreach (var columnHeader in rowHeader)
                                {
                                    colCount++;

                                    if (columnHeader != null && columnHeader.MergeX.HasValue && columnHeader.MergeX.Value > 1)
                                        worksheet.Range(worksheet.Cell(Columns[colCount] + rowCount.ToString()), worksheet.Cell(Columns[colCount + (columnHeader.MergeX.Value - 1)] + rowCount.ToString())).Merge();
                                }
                            }
                        }

                        int firstCellRow = sheet.Header.Count + 1;
                        int lastCellRow = sheet.Header.Count + (sheet.Body != null && sheet.Body.Any() ? sheet.Body.Count() : 0);

                        worksheet.Range(1, 1, firstCellRow, colCount).CellsUsed().Style
                                 .Font.SetBold(true)
                                 .Font.SetFontColor(XLColor.Black)
                                 .Fill.SetBackgroundColor(XLColor.LightGray)
                                 .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                 //.Border.SetTopBorder(XLBorderStyleValues.Thin)
                                 //.Border.SetTopBorderColor(XLColor.BlueGray)
                                 //.Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                 //.Border.SetLeftBorderColor(XLColor.BlueGray)
                                 //.Border.SetRightBorder(XLBorderStyleValues.Thin)
                                 //.Border.SetRightBorderColor(XLColor.BlueGray)
                                 .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                 .Border.SetBottomBorderColor(XLColor.Black);

                        if (sheet.Body != null)
                        {
                            if (sheet.Body.Count > 1000000)
                            {
                                bool condition = true;
                                int block = 1000000;
                                int min = block * -1;

                                while (condition)
                                {
                                    min += block;

                                    if ((min + block) >= sheet.Body.Count)
                                    {
                                        block = sheet.Body.Count - min;
                                        condition = false;
                                    }

                                    var body = sheet.Body.GetRange(min, block);
                                    worksheet.Cell(min + 1, 1).InsertData(body);
                                    condition = false; //Error cuando supera el millon de registros, se debe revisar
                                }
                            }
                            else
                            {
                                worksheet.Cell(firstCellRow, 1).InsertData(sheet.Body);
                            }
                        }

                        worksheet.Range(firstCellRow - 1, 1, lastCellRow, colCount).SetAutoFilter();
                        worksheet.Columns(1, colCount).AdjustToContents(double.Parse("10"), double.Parse("25"));

                        if (sheet.Header != null && sheet.Header.Any() && sheet.Body != null)
                        {
                            colCount = 0;
                            foreach (var columnHeader in sheet.Header[sheet.Header.Count - 1])
                            {
                                colCount++;
                                if (columnHeader.Type == CellDataType.Number)
                                {
                                    worksheet.Range(firstCellRow, colCount, lastCellRow, colCount)
                                        .Style
                                        .NumberFormat.SetFormat(columnHeader.Format)
                                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                                }
                                else if (columnHeader.Type == CellDataType.Date)
                                {
                                    worksheet.Range(firstCellRow, colCount, lastCellRow, colCount)
                                        .Style
                                        .DateFormat.SetFormat(columnHeader.Format)
                                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                                }
                                else
                                {
                                    worksheet.Range(firstCellRow, colCount, lastCellRow, colCount)
                                        .SetDataType(XLDataType.Text)
                                        .Style
                                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                                }
                            }
                        }
                    }

                    workbook.SaveAs(stream);
                    stream.Position = 0;
                }

                response.Status = true;
                response.Object = stream;
                response.Message = ContentType.Excel.XLSX;
            }
            catch (Exception e)
            {
                response.Status = false;
                response.Message = JsonConvert.SerializeObject(e);
            }

            return response;
        }
    }
}
