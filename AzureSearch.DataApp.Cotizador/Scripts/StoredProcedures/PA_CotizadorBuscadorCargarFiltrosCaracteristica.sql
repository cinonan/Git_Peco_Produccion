SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_CotizadorBuscadorCargarFiltrosCaracteristica]
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	19/04/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN
	
	delete from TMP_Cotizador_FeatureFilter;

	insert into TMP_Cotizador_FeatureFilter
	SELECT 
		carac.ID_Caracteristica as FeatureFilterId,
		carac.C_Descripcion as FeatureFilterName,
		carac.N_Prelacion as FeatureFilterPrelacion
	from T_Caracteristica carac WITH(NOLOCK)
	where 
	carac.N_Categoria in (select CategoryId from TMP_Cotizador_Category)
	and carac.C_TipoCaracteristica NOT IN ('OPCIONAL')
	and  exists (select top 1 1  
					from T_CatalogoProducto Cat WITH(NOLOCK) 
					JOIN T_CatFichaProducto Ficha WITH(NOLOCK) ON Ficha.N_CatalogoProducto =  Cat.ID_CatalogoProducto
					JOIN T_ProductoOfertado Oferta WITH(NOLOCK) ON Oferta.N_CatalogoProducto  = Cat.ID_CatalogoProducto
				WHERE carac.C_Estado = 'ACTIVO' 
					AND carac.C_RequiereSubValor='NO' 
					AND carac.C_RequiereVariosValores='NO'
					and Ficha.N_Caracteristica = carac.ID_Caracteristica);

	select 1;
END