select 
 ps.id_Catalogoproducto AS [ProductSheetId]
from T_AcuerdoCatalogo as agreement WITH (NOLOCK) 
inner join T_AcuerdoCatalogo as catalogue WITH (NOLOCK) on catalogue.N_AcuerdoPadre = agreement.ID_AcuerdoCatalogo 
inner join T_CatalogoProducto as ps WITH (NOLOCK) on catalogue.ID_AcuerdoCatalogo = ps.N_AcuerdoCatalogo 
inner join T_Categoria as category WITH (NOLOCK) on category.ID_Categoria = ps.N_Categoria 
where 
 agreement.n_Acuerdopadre IS NULL AND agreement.C_Estado IN ('OPERACIONES','CERRADO') AND 
 catalogue.n_Acuerdopadre IS NOT NULL AND catalogue.C_Estado IN ('ACTIVO') AND 
 ps.C_Estado IN ('OFERTADA','SUSPENDIDA') AND 
 category.C_Estado IN ('ACTIVO'); 

--================================================================================================
--Fichas
--================================================================================================
DROP TABLE IF EXISTS as_FichaProducto;
create table as_FichaProducto (id_Catalogoproducto bigint);
insert into as_FichaProducto
select 
 ps.id_Catalogoproducto AS [ProductSheetId]
from T_AcuerdoCatalogo as agreement WITH (NOLOCK) 
inner join T_AcuerdoCatalogo as catalogue WITH (NOLOCK) on catalogue.N_AcuerdoPadre = agreement.ID_AcuerdoCatalogo 
inner join T_CatalogoProducto as ps WITH (NOLOCK) on catalogue.ID_AcuerdoCatalogo = ps.N_AcuerdoCatalogo 
inner join T_Categoria as category WITH (NOLOCK) on category.ID_Categoria = ps.N_Categoria 
where 
 agreement.n_Acuerdopadre IS NULL AND agreement.C_Estado IN ('OPERACIONES','CERRADO') AND 
 catalogue.n_Acuerdopadre IS NOT NULL AND catalogue.C_Estado IN ('ACTIVO') AND 
 ps.C_Estado IN ('OFERTADA','SUSPENDIDA') AND 
 category.C_Estado IN ('ACTIVO'); 


--================================================================================================
--Producto Ofertado
--================================================================================================
DROP TABLE IF EXISTS as_ProductoOfertado;
create table as_ProductoOfertado (N_CatalogoProducto bigint, N_UltimaSuscripcion bigint);
insert into as_ProductoOfertado
select po.N_CatalogoProducto, po.n_ultimasuscripcion 
from T_ProductoOfertado as po with (nolock) 
where
po.N_CatalogoProducto in (SELECT id_Catalogoproducto from as_FichaProducto) and
po.C_Estado = 'VIGENTE';

select * from as_ProductoOfertado

--================================================================================================
--Cobertura Proveedor
--================================================================================================
DROP TABLE IF EXISTS as_CoberturaProveedor;
create table as_CoberturaProveedor (N_UltimaSuscripcion bigint, C_Ubigeo varchar(8));
go
insert into as_CoberturaProveedor
select N_Suscripcion, C_Ubigeo from t_coberturaproveedor where N_Estado = 1;


select top 10000 * from as_ProductoOfertado

select distinct
po.N_CatalogoProducto AS [ProductSheetId], 
ubigeo.C_Nombre AS [DepartamentName]
from as_ProductoOfertado as po WITH(NOLOCK) 
inner join as_CoberturaProveedor as cp WITH(NOLOCK) on po.n_ultimasuscripcion = cp.N_UltimaSuscripcion
inner join T_Ubigeo as ubigeo  WITH(NOLOCK) on ubigeo.ID_Ubigeo = cp.C_Ubigeo 
where N_CatalogoProducto in (select top 10000 N_CatalogoProducto from as_ProductoOfertado);