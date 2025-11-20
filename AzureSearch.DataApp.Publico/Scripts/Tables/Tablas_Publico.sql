create table TMP_Publico_Agreement (
	AgreementId bigint,
	AgreementName varchar(255),
	AgreementStatus varchar(100)
);

create table TMP_Publico_Catalogue (
	CatalogueId bigint,
	CatalogueName varchar(255),
	AgreementId bigint
);

create table TMP_Publico_Category (
	CategoryId bigint,
	CategoryName varchar(255),
	CatalogueId bigint
);

create table TMP_Publico_Product (
	AgreementId bigint,
	CatalogueId bigint,
	CategoryId bigint,
	ProductId bigint, 
	ProductName varchar(3000), 
	ProductPublishedDate varchar(10), 
	ProductUpdatedDate varchar(10), 
	ProductStatus varchar(20), 
	ProductImage varchar(255), 
	ProductFile varchar(255)
);


create table TMP_Publico_ProductoDepartmento (ProductId bigint, UbigeoId varchar(8), UbigeoName varchar(100));	
create table TMP_Publico_ProductoOfertado(N_CatalogoProducto bigint, N_UltimaSuscripcion bigint);
create table TMP_Publico_CoberturaProveedor (N_UltimaSuscripcion bigint, C_Ubigeo varchar(8));

create table TMP_Publico_Caracteristica (
	ProductId bigint,  
	FeatureTypeId bigint,
	FeatureTypeName varchar(255),
	FeatureTypeRequiredSubValue varchar(5),
	FeatureValueId bigint,
	FeatureValueName varchar(1000),
	FeatureValueImg varchar(255)
);