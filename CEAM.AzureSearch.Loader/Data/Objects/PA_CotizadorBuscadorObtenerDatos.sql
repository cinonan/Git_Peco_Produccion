SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_CotizadorBuscadorObtenerDatos]
(
@table varchar(30)
)
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	19/04/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN
	
	if @table = 'Agreement' 
		begin
			select * from [dbo].[TMP_Cotizador_Agreement];
		end
	else if @table = 'Catalogue'
		begin
			select * from [dbo].[TMP_Cotizador_Catalogue];
		end
	else if @table = 'Category'
		begin
			select * from [dbo].[TMP_Cotizador_Category];
		end
	else if @table = 'ProductFeature'
		begin
			select * from [dbo].[TMP_Cotizador_ProductFeature];
		end
	else if @table = 'Product'
		begin
			select * from [dbo].[TMP_Cotizador_Product];
		end
	else if @table = 'FeatureFilter'
		begin
			select * from [dbo].[TMP_Cotizador_FeatureFilter];
		end

END