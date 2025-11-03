using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AzureSearch.Models.Publico.Models;
using CEAM.AzureSearch.WebApp.Models;
using CEAM.AzureSearch.WebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CEAM.AzureSearch.WebApp.Controllers
{
    public class SearchController : Controller
    {
        public IConfiguration _configuration { get; set; }

        public IExcelService _excelService { get; set; }
        public IAzureSearchService _searchService { get; set; }

        public SearchController(
            IExcelService excelService,
            IAzureSearchService searchService,
            IConfiguration configuration)
        {
            _excelService = excelService;
            _searchService = searchService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            TempData.Clear();

            var model = new SearchDataModel();
            model.From = "Search";
            model.SearchText = "";
            model.IsNewSearch = true;

            var agreementList = await _searchService.SearchByAgreementStatus(model, "VIGENTE");
            model.AgreementFilter = new List<FilterDataModel>();
            model.AgreementFilter.Add(new FilterDataModel { Text = "VIGENTES", Items = agreementList });
            //model.AgreementFilter = agreementList;
            return View("Search", model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchByAgreementStatus([FromForm] SearchDataModel model)
        {
            var agreementFilter = model.SearchText; // == "NO VIGENTES" ? "NO VIGENTE" : "VIGENTE";

            model.SearchText = "*";
            var agreementList = await _searchService.SearchByAgreementStatus(model, agreementFilter);
            return Json(agreementList);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SearchDataModel model)
        {
            try
            {
                ModelState.Clear();
                model.From = "Result";
                // Log breve para debugging
                Debug.WriteLine($"[SearchController] Index POST - SearchText='{model?.SearchText}', ClientFilter.Feature='{model?.ClientFilter?.Feature}'");
                model = await _searchService.RunQueryAsync(model);
                Debug.WriteLine($"[SearchController] RunQueryAsync finished - Results.Count={(model?.Results?.TotalCount ?? 0)}");
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorViewModel { RequestId = "1", Message = ex.Message };
                return View("Error", errorVM);
            }

            TryValidateModel(model); //var aaa = await TryUpdateModelAsync(model);
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchDataModel model)
        {
            try
            {
                ModelState.Clear();
                model.From = "Result";
                Debug.WriteLine($"[SearchController] AJAX Search POST - SearchText='{model?.SearchText}', ClientFilter.Feature='{model?.ClientFilter?.Feature}'");
                model = await _searchService.RunQueryAsync(model);
                Debug.WriteLine($"[SearchController] RunQueryAsync finished - Results.Count={(model?.Results?.TotalCount ?? 0)}");
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorViewModel { RequestId = "1", Message = ex.Message };
                return PartialView("Error", errorVM);
            }

            TryValidateModel(model); //var aaa = await TryUpdateModelAsync(model);
            return PartialView("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> Download(SearchDataModel model)
        {
            //var str = HttpContext.Session.GetString("download");
            //var model = JsonConvert.DeserializeObject<SearchDataModel>(str);
            ModelState.Clear();
            model.From = "Result";
            var list = await _searchService.DownLoadAsync(model);

            string imageUrl = _configuration["ImageUrl"];
            string fileUrl = _configuration["FileUrl"];

            list.ForEach(i =>
            {
                i.Image = imageUrl + i.Image;
                i.File = fileUrl + i.File;
            });

            var response = await _excelService.GenerateExcel(list);
            if (response.Status)
            {
                return new FileStreamResult(response.Object, response.Message)
                {
                    FileDownloadName = "reporte.xlsx"
                };
            }

            return NotFound();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}