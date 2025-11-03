SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PA_CotizadorBuscadorCargarFichaProducto]
AS
/*
=============================================================================================
	FECHA			USUARIO				VERSION		REQ
=============================================================================================
	19/04/2021		Willman Rojas		1.0			Creación del Stored Procedure
=============================================================================================
*/
BEGIN
	
	delete from TMP_Cotizador_Product;

	insert into TMP_Cotizador_Product
	select
		ps.id_Catalogoproducto AS ProductId,
		ps.c_Descripcion AS ProductName,
		case when ps.D_FechaPublicacion is null then '' else convert(varchar(10), ps.D_FechaPublicacion, 103) end as ProductPublishedDate,
		case when ps.A_ModificacionFecha is null then '' else convert(varchar(10), ps.A_ModificacionFecha, 103) end as ProductUpdatedDate,
		ps.C_Estado as ProductStatus,
		ps.C_Imagen as ProductImage, 
		ps.C_ArchivoDescriptivo as [ProductFile],
		ps.N_AcuerdoCatalogo as CatalogueId,
		ps.N_Categoria as CategoryId
	from T_CatalogoProducto as ps WITH(NOLOCK)
	inner join TMP_Cotizador_Category as cat on ps.N_AcuerdoCatalogo = cat.CatalogueId and ps.N_Categoria = cat.CategoryId
	where 
	--	ps.N_AcuerdoCatalogo =
	--	ps.N_Categoria = '+@N_Categoria+'
		ps.c_estado = 'OFERTADA'
	--	and c.ID_Categoria=b.N_Categoria
		and exists(
		select top 1 1 
			from T_CatalogoProductoPrecioCotizado j WITH(NOLOCK)
			where j.id_CatalogoProductoPrecioCotizado = ps.id_CatalogoProducto
			and isnull(j.n_precioestimado,0) <> 0
			and j.n_estado = 1--Activo
		);

	select 1;
END