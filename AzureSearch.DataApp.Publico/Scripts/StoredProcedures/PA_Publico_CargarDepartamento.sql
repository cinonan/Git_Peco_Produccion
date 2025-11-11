SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_Publico_CargarDepartamento]
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	01/06/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN

	truncate table TMP_Publico_CoberturaProveedor;
	truncate table TMP_Publico_ProductoOfertado;
	truncate table TMP_Publico_ProductoDepartmento;

	insert into TMP_Publico_CoberturaProveedor(N_UltimaSuscripcion, C_Ubigeo)
	select N_Suscripcion, C_Ubigeo from t_coberturaproveedor where N_Estado = 1; 

	insert into TMP_Publico_ProductoOfertado(N_CatalogoProducto, N_UltimaSuscripcion)
	select po.N_CatalogoProducto, po.n_ultimasuscripcion
	from T_ProductoOfertado as po with(nolock)
	where
		po.N_CatalogoProducto in (SELECT ProductId from TMP_Publico_Product) and
		po.C_Estado = 'VIGENTE';
	
	insert into TMP_Publico_ProductoDepartmento (ProductId, UbigeoId, UbigeoName)
	select distinct
		po.N_CatalogoProducto,
		cp.C_Ubigeo,
		ubigeo.C_Nombre
    from TMP_Publico_ProductoOfertado as po WITH(NOLOCK) 
    inner join TMP_Publico_CoberturaProveedor as cp WITH(NOLOCK) on po.n_ultimasuscripcion = cp.N_UltimaSuscripcion 
	inner join T_Ubigeo as ubigeo  WITH(NOLOCK) on ubigeo.ID_Ubigeo = cp.C_Ubigeo;

	select 1;
END