using AzureSearch.DataApp.Publico.Scripts.Queries;
using AzureSearch.Models.Publico.Entities;
using AzureSearch.Models.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace AzureSearch.DataApp.Publico.Repositories
{
    public class PublicoProductRepository
    {
        #region "Properties"
        private ProductSheetQuery query;
        private MSSQLSetting dbSettings;
        #endregion

        #region Constructors
        public PublicoProductRepository()
        {
            query = new ProductSheetQuery();
            ConfigurationSetup();
        }
        #endregion

        #region "Public Methods"
        public async Task<bool> LoadData()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_Publico_CargarBase]", commandTimeout: 6000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<bool> LoadDataFeatures()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_Publico_CargarCaracteristica]", commandTimeout: 180000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<bool> LoadDataDepartments()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_Publico_CargarDepartamento]", commandTimeout: 180000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<List<T>> GetData<T>(string table)
        {
            var list = new List<T>();
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@table", table);

                var result = await db.QueryAsync<T>("[dbo].[PA_Publico_ObtenerDatos]", parameters, commandType: CommandType.StoredProcedure, commandTimeout: 6000);
                list = result.AsList();
            }
            return list;
        }

        public async Task<List<PublicoProductEntity>> GetProductSheetListAsync(int? limit = null)
        {
            var psList = new List<PublicoProductEntity>();
            var myQuery = query.GetProductSheets(limit);

            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.QueryAsync<PublicoProductEntity>(myQuery, commandTimeout: 300);
                psList = result.ToList();
            }

            foreach(var ps in psList)
            {
                ps.AgreementName = string.IsNullOrWhiteSpace(ps.AgreementName) ? "" : ps.AgreementName.Trim();
                ps.AgreementStatus = string.IsNullOrWhiteSpace(ps.AgreementStatus) ? "" : ps.AgreementStatus.Trim().ToUpper();
                ps.CatalogueName = string.IsNullOrWhiteSpace(ps.CatalogueName) ? "" : ps.CatalogueName.Trim();
                ps.CategoryName = string.IsNullOrWhiteSpace(ps.CategoryName) ? "" : ps.CategoryName.Trim();
                ps.ProductFile = string.IsNullOrWhiteSpace(ps.ProductFile) ? "" : ps.ProductFile.Trim();
                ps.ProductImage = string.IsNullOrWhiteSpace(ps.ProductImage) ? "" : ps.ProductImage.Trim();
                ps.ProductName = string.IsNullOrWhiteSpace(ps.ProductName) ? "" : ps.ProductName.Trim();
                ps.ProductStatus = string.IsNullOrWhiteSpace(ps.ProductStatus) ? "" : ps.ProductStatus.Trim().ToUpper();
            }
            return psList;
        }

        public async Task<List<PublicoFeatureEntity>> GetFeatureListAsync(string productSheetIds)
        {
            var list = new List<PublicoFeatureEntity>();
            var myQuery = query.GetFeatures(productSheetIds);

            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.QueryAsync<PublicoFeatureEntity>(myQuery, commandTimeout: 300);
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

        public async Task<List<PublicoDepartmentEntity>> GetDepartmentListAsync(string productSheetIds)
        {
            var list = new List<PublicoDepartmentEntity>();
            var myQuery = query.GetDepartments(productSheetIds);

            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.QueryAsync<PublicoDepartmentEntity>(myQuery, commandTimeout : 3600);
                list = result.ToList();
            }

            if (list != null && list.Any())
            {
                list.RemoveAll(x => string.IsNullOrWhiteSpace(x.UbigeoName));
                list.Select(s => { s.UbigeoName = s.UbigeoName.Trim().ToUpper(); return s; }).ToList();
            }

            return list;
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
