SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_Publico_CargarBase]
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	01/06/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN

	delete from TMP_Publico_Agreement;
	delete from TMP_Publico_Catalogue;
	delete from TMP_Publico_Category;
	delete from TMP_Publico_Product;

	insert into TMP_Publico_Agreement (AgreementId, AgreementName, AgreementStatus)
	select
		agreement.ID_AcuerdoCatalogo as [AgreementId], 
		agreement.c_denominacion as [AgreementName], 
		CASE WHEN agreement.C_Estado = 'OPERACIONES' THEN 'VIGENTE' ELSE 'NO VIGENTE' END AS [AgreementStatus]
	from T_AcuerdoCatalogo as agreement WITH (NOLOCK) 
	where 
		agreement.n_Acuerdopadre IS NULL AND agreement.C_Estado IN ('OPERACIONES','CERRADO');

	insert into TMP_Publico_Catalogue (CatalogueId, CatalogueName, AgreementId)
	select
		catalogue.ID_AcuerdoCatalogo as [CatalogueId], 
		catalogue.c_denominacion as [CatalogueName],
		catalogue.N_AcuerdoPadre as [AgreementId]
	from T_AcuerdoCatalogo as catalogue WITH (NOLOCK) 
	inner join TMP_Publico_Agreement as agreement on catalogue.N_AcuerdoPadre = agreement.AgreementId 
	where
		catalogue.n_Acuerdopadre IS NOT NULL AND catalogue.C_Estado IN ('ACTIVO');

	insert into TMP_Publico_Category (CategoryId, CategoryName, CatalogueId)
	select
		category.ID_Categoria AS [CategoryId], 
		category.c_denominacion as [CategoryName],
		category.N_Catalogo AS [CatalogueId]
	from T_Categoria as category WITH (NOLOCK) --on category.ID_Categoria = ps.N_Categoria 
	where
		category.C_Estado IN ('ACTIVO'); 

	insert into TMP_Publico_Product (
		AgreementId, CatalogueId, CategoryId,
		ProductId, ProductName, ProductPublishedDate, 
		ProductUpdatedDate, ProductStatus, ProductImage, ProductFile)
	select 
		agreement.AgreementId as [AgreementId], 
		catalogue.CatalogueId as [CatalogueId], 
		category.CategoryId AS [CategoryId], 
		ps.id_Catalogoproducto AS [ProductId], 
		ps.c_Descripcion AS [ProductName], 
		case when ps.D_FechaPublicacion is null then '' else convert(varchar(10), ps.D_FechaPublicacion, 103) end as [ProductPublishedDate], 
		case when ps.A_ModificacionFecha is null then '' else convert(varchar(10), ps.A_ModificacionFecha, 103) end as [ProductUpdatedDate], 
		ps.C_Estado as [ProductStatus], 
		ps.C_Imagen as [ProductImage], 
		ps.C_ArchivoDescriptivo as [ProductFile] 
	from TMP_Publico_Agreement as agreement WITH (NOLOCK) 
	inner join TMP_Publico_Catalogue as catalogue WITH (NOLOCK) on catalogue.AgreementId = agreement.AgreementId 
	inner join T_CatalogoProducto as ps WITH (NOLOCK) on catalogue.CatalogueId = ps.N_AcuerdoCatalogo 
	inner join TMP_Publico_Category as category WITH (NOLOCK) on category.CategoryId = ps.N_Categoria and category.CatalogueId = catalogue.CatalogueId
	where 
		ps.C_Estado IN ('OFERTADA','SUSPENDIDA');

	delete from TMP_Publico_Agreement Where AgreementId not in (select distinct AgreementId from TMP_Publico_Product);
	delete from TMP_Publico_Catalogue Where CatalogueId not in (select distinct CatalogueId from TMP_Publico_Product);
	delete from TMP_Publico_Category Where CategoryId not in (select distinct CategoryId from TMP_Publico_Product);

	select 1;
END