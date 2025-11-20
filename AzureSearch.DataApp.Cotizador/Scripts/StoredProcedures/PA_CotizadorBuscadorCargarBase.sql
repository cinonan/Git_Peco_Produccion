SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_CotizadorBuscadorCargarBase]
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	19/04/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN
	
	delete from TMP_Cotizador_Agreement;
	delete from TMP_Cotizador_Catalogue;
	delete from TMP_Cotizador_Category;

	declare @tmp table (
		AgreementId bigint, 
		AgreementName varchar(500), 
		AgreementStatus varchar(25),
		CatalogueId bigint, 
		CatalogueName varchar(500),
		CatalogueStatus varchar(25));

	insert into @tmp
	select
		acuerdo.id_acuerdoCatalogo acuerdo_id,
		acuerdo.c_codigoAcuerdoCatalogo acuerdo_nombre,
		acuerdo.C_Estado as acuerdo_estado,
		catalogo.ID_AcuerdoCatalogo catalogo_id,
		catalogo.C_Denominacion as catalogo_nombre,
		catalogo.C_Estado as catalogo_estado
	from T_AcuerdoCatalogo acuerdo WITH(NOLOCK)
	inner join t_AcuerdoCatalogo catalogo WITH(NOLOCK) on catalogo.N_AcuerdoPadre = acuerdo.id_AcuerdoCatalogo
	where catalogo.n_acuerdopadre=acuerdo.id_AcuerdoCatalogo
	and acuerdo.C_Estado in ('OPERACIONES')
	AND acuerdo.C_TipoAcuerdo = 'BIENES'
	and catalogo.C_Estado = 'ACTIVO' 
	and exists (select top 1 1
				from T_CatalogoProductoPrecioCotizado j WITH(NOLOCK)
				join t_catalogoproducto k WITH(NOLOCK) on k.id_catalogoproducto=j.ID_CatalogoProductoPrecioCotizado
				where k.n_acuerdocatalogo=catalogo.id_acuerdocatalogo and j.n_estado=1);

	-- Agreements
	insert into TMP_Cotizador_Agreement
	select distinct AgreementId, AgreementName, AgreementStatus from @tmp;

	-- Catalogues
	insert into TMP_Cotizador_Catalogue
	select distinct CatalogueId, CatalogueName, CatalogueStatus, AgreementId from @tmp;

	-- Categories
	insert into TMP_Cotizador_Category
	select 
		a.ID_Categoria as CategoryId, 
		a.C_CodigoCategoria as CategoryName,
		a.n_catalogo as CatalogueId
	from T_Categoria a WITH(NOLOCK)
	inner join TMP_Cotizador_Catalogue as cat on cat.CatalogueId = a.N_Catalogo
	where 
		a.c_estado = 'ACTIVO'
		and exists (select top 1 1
					from T_CatalogoProductoPrecioCotizado j WITH(NOLOCK)
					join t_catalogoproducto k WITH(NOLOCK) on k.id_catalogoproducto=j.ID_CatalogoProductoPrecioCotizado
					where k.n_categoria=a.ID_Categoria and j.n_estado=1);

	select 1;
END