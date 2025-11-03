SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_Publico_ObtenerDatos]
(
@table varchar(30)
)
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	01/06/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN
	
	if @table = 'Agreement' 
		begin
			select * from [dbo].[TMP_Publico_Agreement];
		end
	else if @table = 'Catalogue'
		begin
			select * from [dbo].[TMP_Publico_Catalogue];
		end
	else if @table = 'Category'
		begin
			select * from [dbo].[TMP_Publico_Category];
		end
	else if @table = 'Product'
		begin
			select * from [dbo].[TMP_Publico_Product];
		end
	else if @table = 'Department'
		begin
			select * from [dbo].[TMP_Publico_ProductoDepartmento];
		end
	else if @table = 'Feature'
		begin
			select * from [dbo].[TMP_Publico_Caracteristica];
		end
	else if @table = 'ProductFeature'
		begin
			select * from [dbo].[TMP_Publico_ProductFeature];
		end
	else if @table = 'FeatureFilter'
		begin
			select * from [dbo].[TMP_Publico_FeatureFilter];
		end


END