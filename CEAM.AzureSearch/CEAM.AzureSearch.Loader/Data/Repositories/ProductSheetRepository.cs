using CEAM.AzureSearch.Loader.Data.Queries;
using CEAM.AzureSearch.Models.Entities;
using CEAM.AzureSearch.Models.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace CEAM.AzureSearch.Loader.Data.Repositories
{
    public class ProductSheetRepository
    {
        #region "Properties"
        private ProductSheetQuery query;
        private MSSQLSetting dbSettings;
        #endregion

        #region Constructors
        public ProductSheetRepository()
        {
            query = new ProductSheetQuery();
            ConfigurationSetup();
        }
        #endregion

        #region "Public Methods"
        public async Task<List<ProductSheetEntity>> GetProductSheetListAsync(int? limit = null)
        {
            var psList = new List<ProductSheetEntity>();
            var myQuery = query.GetProductSheets(limit);

            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.QueryAsync<ProductSheetEntity>(myQuery, commandTimeout: 300);
                psList = result.ToList();
            }

            foreach(var ps in psList)
            {
                ps.AgreementName = string.IsNullOrWhiteSpace(ps.AgreementName) ? "" : ps.AgreementName.Trim();
                ps.AgreementStatus = string.IsNullOrWhiteSpace(ps.AgreementStatus) ? "" : ps.AgreementStatus.Trim().ToUpper();
                ps.CatalogueName = string.IsNullOrWhiteSpace(ps.CatalogueName) ? "" : ps.CatalogueName.Trim();
                ps.CategoryName = string.IsNullOrWhiteSpace(ps.CategoryName) ? "" : ps.CategoryName.Trim();
                ps.ProductSheetFile = string.IsNullOrWhiteSpace(ps.ProductSheetFile) ? "" : ps.ProductSheetFile.Trim();
                ps.ProductSheetImage = string.IsNullOrWhiteSpace(ps.ProductSheetImage) ? "" : ps.ProductSheetImage.Trim();
                ps.ProductSheetName = string.IsNullOrWhiteSpace(ps.ProductSheetName) ? "" : ps.ProductSheetName.Trim();
                ps.ProductSheetStatus = string.IsNullOrWhiteSpace(ps.ProductSheetStatus) ? "" : ps.ProductSheetStatus.Trim().ToUpper();
            }

            return psList;
        }

        public async Task<List<FeatureEntity>> GetFeatureListAsync(string productSheetIds)
        {
            var list = new List<FeatureEntity>();
            var myQuery = query.GetFeatures(productSheetIds);

            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.QueryAsync<FeatureEntity>(myQuery, commandTimeout: 300);
                list = result.ToList();
            }

            if (list != null && list.Any())
            {
                list.RemoveAll(x => string.IsNullOrWhiteSpace(x.FeatureTypeName));
                list.RemoveAll(x => string.IsNullOrWhiteSpace(x.FeatureValueName));
                list.Select(s => {
                    s.FeatureTypeName = s.FeatureTypeName.Trim().ToUpper();
                    s.FeatureValueName = s.FeatureValueName.Trim().ToUpper();
                    s.FeatureTypeRequiredSubValue = string.IsNullOrWhiteSpace(s.FeatureTypeRequiredSubValue) ? "NO" : s.FeatureTypeRequiredSubValue.Trim().ToUpper();
                    return s; }).ToList();
            }

            return list;
        }

        public async Task<List<DepartmentEntity>> GetDepartmentListAsync(string productSheetIds)
        {
            var list = new List<DepartmentEntity>();
            var myQuery = query.GetDepartments(productSheetIds);

            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.QueryAsync<DepartmentEntity>(myQuery, commandTimeout : 3600);
                list = result.ToList();
            }

            if (list != null && list.Any())
            {
                list.RemoveAll(x => string.IsNullOrWhiteSpace(x.DepartamentName));
                list.Select(s => { s.DepartamentName = s.DepartamentName.Trim().ToUpper(); return s; }).ToList();
            }

            return list;
        }
        
        public async Task LoadDepartments_01_FichaProducto()
        {
            var queryCreate = query.LoadDepartments_01_FichaProducto_CreateTable();
            var queryLoad = query.LoadDepartments_01_FichaProducto();
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                await db.QueryAsync(queryCreate);
                await db.QueryAsync(queryLoad, commandTimeout: 1800000);
            }
        }

        public async Task LoadDepartments_02_ProductoOfertado()
        {
            var queryCreate = query.LoadDepartments_02_ProductoOfertado_CreateTable();
            var queryLoad = query.LoadDepartments_02_ProductoOfertado();
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                await db.QueryAsync(queryCreate);
                await db.QueryAsync(queryLoad, commandTimeout: 1800000);
            }
        }

        public async Task LoadDepartments_03_CoberturaProveedor()
        {
            var queryCreate = query.LoadDepartments_03_CoberturaProveedor_CreateTable();
            var queryLoad = query.LoadDepartments_03_CoberturaProveedor();
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                await db.QueryAsync(queryCreate);
                await db.QueryAsync(queryLoad, commandTimeout: 1800000);
            }
        }
        #endregion

        #region "Private Methods"
        private void ConfigurationSetup()
        {
            query = new ProductSheetQuery();
            dbSettings = new MSSQLSetting();

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            dbSettings.Server = configuration["DBSettings:Server"];
            dbSettings.Database = configuration["DBSettings:DB"];
            dbSettings.User = configuration["DBSettings:User"];
            dbSettings.Password = configuration["DBSettings:Pass"];
        }
        #endregion
    }
}
