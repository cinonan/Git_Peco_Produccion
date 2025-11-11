using AzureSearch.Models.Settings;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AzureSearch.DataApp.Cotizador.Repositories
{
    public class CotizadorRepository
    {
        private MSSQLSetting dbSettings;
        private void ConfigurationSetup()
        {
            dbSettings = new MSSQLSetting();

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            dbSettings.Server = configuration["DBSettings:Server"];
            dbSettings.Database = configuration["DBSettings:DB"];
            dbSettings.User = configuration["DBSettings:User"];
            dbSettings.Password = configuration["DBSettings:Pass"];
        }

        public CotizadorRepository()
        {
            ConfigurationSetup();
        }

        public async Task<bool> LoadData()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_CotizadorBuscadorCargarBase]", commandTimeout: 6000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<bool> LoadProducts()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_CotizadorBuscadorCargarFichaProducto]", commandTimeout: 6000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<bool> LoadFeatureFilters()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_CotizadorBuscadorCargarFiltrosCaracteristica]", commandTimeout: 6000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<bool> LoadProductFilters()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_CotizadorBuscadorCargarCaracteristicasProducto]", commandTimeout: 6000);
                condition = result.ToString() == "1" ? true : false;
            }
            return condition;
        }

        public async Task<bool> LoadDataDepartments()
        {
            bool condition = false;
            using (IDbConnection db = new SqlConnection(dbSettings.GetConnectionString))
            {
                var result = await db.ExecuteScalarAsync("[dbo].[PA_Cotizador_CargarDepartamento]", commandTimeout: 180000);
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

                var result = await db.QueryAsync<T>("[dbo].[PA_CotizadorBuscadorObtenerDatos]", parameters, commandType: CommandType.StoredProcedure, commandTimeout: 6000);
                list = result.AsList();
            }
            return list;
        }
    }
}
