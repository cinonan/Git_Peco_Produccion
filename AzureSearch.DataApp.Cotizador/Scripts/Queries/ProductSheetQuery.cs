namespace AzureSearch.DataApp.Cotizador.Scripts
{
    public class ProductSheetQuery
    {

        public string GetProductSheets(int? limit)
        {
            string _limit = limit.HasValue ? "top " + limit.ToString() : string.Empty;

			string query = "";
			query += "select {0} ";
			query += " agreement.ID_AcuerdoCatalogo as [AgreementId], ";
			query += " agreement.c_denominacion as [AgreementName], ";
			query += " CASE WHEN agreement.C_Estado = 'OPERACIONES' THEN 'VIGENTE' ELSE 'NO VIGENTE' END AS [AgreementStatus], ";
			query += " catalogue.ID_AcuerdoCatalogo as [CatalogueId], ";
			query += " catalogue.c_denominacion as [CatalogueName], ";
			query += " ps.id_Catalogoproducto AS [ProductSheetId], ";
			query += " ps.c_Descripcion AS [ProductSheetName], ";
			query += " ps.D_FechaPublicacion as [ProductSheetPublishedDate], ";
            query += " ps.A_ModificacionFecha as [ProductSheetUpdatedDate], ";

            query += " ps.C_Estado as [ProductSheetStatus], ";
            query += " ps.C_Imagen as [ProductSheetImage], ";
			query += " ps.C_ArchivoDescriptivo as [ProductSheetFile], ";
			query += " category.ID_Categoria AS [CategoryId], ";
			query += " category.c_denominacion as [CategoryName] ";

			query += "from T_AcuerdoCatalogo as agreement WITH (NOLOCK) ";
			query += "inner join T_AcuerdoCatalogo as catalogue WITH (NOLOCK) on catalogue.N_AcuerdoPadre = agreement.ID_AcuerdoCatalogo ";
			query += "inner join T_CatalogoProducto as ps WITH (NOLOCK) on catalogue.ID_AcuerdoCatalogo = ps.N_AcuerdoCatalogo ";
			query += "inner join T_Categoria as category WITH (NOLOCK) on category.ID_Categoria = ps.N_Categoria ";
			query += "where ";
			query += " agreement.n_Acuerdopadre IS NULL AND agreement.C_Estado IN ('OPERACIONES','CERRADO') AND ";
			query += " catalogue.n_Acuerdopadre IS NOT NULL AND catalogue.C_Estado IN ('ACTIVO') AND ";
			query += " ps.C_Estado IN ('OFERTADA','SUSPENDIDA') AND ";
			query += " category.C_Estado IN ('ACTIVO'); ";

			return string.Format(query, _limit);
        }

        public string GetFeatures(string productSheetIds)
        {
            string query = "";
            query += "SELECT ";
            query += "CATFICPROD.N_CatalogoProducto AS [ProductSheetId], ";
            query += "carac.id_Caracteristica AS [FeatureTypeId], ";
            query += "carac.c_Descripcion AS [FeatureTypeName], ";
            query += "carac.C_RequiereSubValor AS [FeatureTypeRequiredSubValue], ";
            query += "val.id_valcaracteristica AS [FeatureValueId], ";
            query += "val.c_Valor AS [FeatureValueName], ";
            query += "val.C_SubValorIMG AS [FeatureValueImg] ";
            query += "FROM T_CatFichaProducto AS CATFICPROD WITH(NOLOCK) ";
            query += "INNER JOIN T_Caracteristica AS carac WITH(NOLOCK) ON carac.id_Caracteristica = CATFICPROD.n_Caracteristica ";
            query += "INNER JOIN T_ValCaracteristica AS val WITH (NOLOCK) ON val.id_valcaracteristica = CATFICPROD.n_valcaracteristica ";
            query += "WHERE ";
            query += "CATFICPROD.C_Estado = 'ACTIVO' AND ";
            query += "carac.C_Estado = 'ACTIVO' AND ";
            query += "carac.C_TipoCaracteristica = 'GENERICA' AND ";
            query += "val.C_Estado = 'ACTIVO' AND ";
            query += "CATFICPROD.N_CatalogoProducto IN ({0}); ";

            return string.Format(query, productSheetIds);
        }

        public string GetDepartments(string productSheetIds)
        {
            string query = "";
            query += "select distinct ";
            query += "po.N_CatalogoProducto AS[ProductSheetId], ";
            query += "ubigeo.C_Nombre AS[DepartamentName] ";
            query += "from as_ProductoOfertado as po WITH(NOLOCK) ";
            query += "inner join as_CoberturaProveedor as cp WITH(NOLOCK) on po.n_ultimasuscripcion = cp.N_UltimaSuscripcion ";
            query += "inner join T_Ubigeo as ubigeo  WITH(NOLOCK) on ubigeo.ID_Ubigeo = cp.C_Ubigeo; ";
            //query += "where N_CatalogoProducto in (select top 10000 N_CatalogoProducto from as_ProductoOfertado); ";

            if (string.IsNullOrWhiteSpace(productSheetIds)) query += "where N_CatalogoProducto in (" + productSheetIds + "); ";
            return query;
        }

        public string LoadDepartments_01_FichaProducto_CreateTable()
        {
            return "IF OBJECT_ID('as_FichaProducto', 'U') IS NULL create table as_FichaProducto(id_Catalogoproducto bigint); ";
        }

        public string LoadDepartments_02_ProductoOfertado_CreateTable()
        {
            return "IF OBJECT_ID('as_ProductoOfertado', 'U') IS NULL create table as_ProductoOfertado(N_CatalogoProducto bigint, N_UltimaSuscripcion bigint); ";
        }

        public string LoadDepartments_03_CoberturaProveedor_CreateTable()
        {
            return "IF OBJECT_ID('as_CoberturaProveedor', 'U') IS NULL create table as_CoberturaProveedor (N_UltimaSuscripcion bigint, C_Ubigeo varchar(8)); ";
        }

        public string LoadDepartments_01_FichaProducto()
        {
            string query = "";
            query += "delete from as_FichaProducto; ";
            query += "insert into as_FichaProducto ";
            query += "select ";
            query += "ps.id_Catalogoproducto AS[ProductSheetId] ";
            query += "from T_AcuerdoCatalogo as agreement WITH(NOLOCK) ";
            query += "inner join T_AcuerdoCatalogo as catalogue WITH(NOLOCK) on catalogue.N_AcuerdoPadre = agreement.ID_AcuerdoCatalogo ";
            query += "inner join T_CatalogoProducto as ps WITH(NOLOCK) on catalogue.ID_AcuerdoCatalogo = ps.N_AcuerdoCatalogo ";
            query += "inner join T_Categoria as category WITH(NOLOCK) on category.ID_Categoria = ps.N_Categoria ";
            query += "where ";
            query += "agreement.n_Acuerdopadre IS NULL AND agreement.C_Estado IN('OPERACIONES','CERRADO') AND ";
            query += "catalogue.n_Acuerdopadre IS NOT NULL AND catalogue.C_Estado IN('ACTIVO') AND ";
            query += "ps.C_Estado IN('OFERTADA','SUSPENDIDA') AND ";
            query += "category.C_Estado IN('ACTIVO'); ";
            return query;
        }

        public string LoadDepartments_02_ProductoOfertado()
        {
            string query = "";
            query += "delete from as_ProductoOfertado; ";
            query += "insert into as_ProductoOfertado ";
            query += "select po.N_CatalogoProducto, po.n_ultimasuscripcion ";
            query += "from T_ProductoOfertado as po with(nolock) ";
            query += "where ";
            query += "po.N_CatalogoProducto in (SELECT id_Catalogoproducto from as_FichaProducto) and ";
            query += "po.C_Estado = 'VIGENTE'; ";
            return query;
        }

        public string LoadDepartments_03_CoberturaProveedor()
        {
            string query = "";
            query += "delete from as_CoberturaProveedor; ";
            query += "insert into as_CoberturaProveedor ";
            query += "select N_Suscripcion, C_Ubigeo from t_coberturaproveedor where N_Estado = 1; ";
            return query;
        }

        //public string GetDepartments(string productSheetIds)
        //{
        //    string query = "";
        //    query += "select distinct ";
        //    query += "po.N_CatalogoProducto AS [ProductSheetId], ";
        //    query += "ubigeo.C_Nombre AS [DepartamentName]";
        //    query += "from T_productoofertado as po WITH(NOLOCK) ";
        //    query += "inner join t_coberturaproveedor as cp WITH(NOLOCK) on po.n_ultimasuscripcion = cp.N_Suscripcion ";
        //    query += "inner join T_Ubigeo as ubigeo  WITH(NOLOCK) on ubigeo.ID_Ubigeo = cp.C_Ubigeo ";
        //    query += "where N_CatalogoProducto in ({0}); ";

        //    return string.Format(query, productSheetIds);
        //}

    }
}
