SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_CotizadorBuscadorCargarCaracteristicasProducto]
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	19/04/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN
	
	delete from TMP_Cotizador_ProductFeature;

	insert into TMP_Cotizador_ProductFeature
	SELECT
		CATFICPROD.N_CatalogoProducto AS ProductSheetId,
		carac.id_Caracteristica AS FeatureTypeId,
		carac.c_Descripcion AS FeatureTypeName,
		carac.C_TipoCaracteristica as FeatureTypeCondition,
		carac.C_RequiereSubValor AS RequiredSubValue,
		val.id_valcaracteristica AS FeatureValueId,
		val.c_Valor AS FeatureValueName,
		val.C_SubValorIMG AS FeatureValueNameSub	
	FROM T_CatFichaProducto AS CATFICPROD WITH(NOLOCK) 
	INNER JOIN TMP_Cotizador_Product AS ps on ps.ProductId = CATFICPROD.N_CatalogoProducto
	INNER JOIN T_Caracteristica AS carac WITH(NOLOCK) ON carac.id_Caracteristica = CATFICPROD.n_Caracteristica 
	INNER JOIN T_ValCaracteristica AS val WITH (NOLOCK) ON val.id_valcaracteristica = CATFICPROD.n_valcaracteristica 
	WHERE 
		CATFICPROD.C_Estado = 'ACTIVO' AND 
		carac.C_Estado = 'ACTIVO' AND 
		--carac.C_TipoCaracteristica = 'GENERICA' AND 
		val.C_Estado = 'ACTIVO'; --AND 
		--CATFICPROD.N_CatalogoProducto IN (select ProductId from @table_product); 

	select 1;
END