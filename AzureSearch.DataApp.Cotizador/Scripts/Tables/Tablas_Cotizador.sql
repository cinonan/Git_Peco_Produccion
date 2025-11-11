/*
declare @tmp table (
	AgreementId bigint, 
	AgreementName varchar(500), 
	CatalogueId bigint, 
	CatalogueName varchar(500))
*/

create table TMP_Cotizador_Agreement (AgreementId bigint, AgreementName varchar(500), AgreementStatus varchar(25));

create table TMP_Cotizador_Catalogue (CatalogueId bigint, CatalogueName varchar(500), CatalogueStatus varchar(25), AgreementId bigint);

create table TMP_Cotizador_Category (CategoryId bigint, CategoryName varchar(500), CatalogueId bigint);

create table TMP_Cotizador_Feature (
	FeatureTypeId bigint, 
	FeatureTypeName varchar(500), 
	PrelacionId int, 
	FeatureValueId bigint, 
	FeatureValueName varchar(500), 
	CategoryId bigint);

create table TMP_Cotizador_FeatureFilter (FeatureFilterId bigint, FeatureFilterName varchar(500), PrelacionId int);

create table TMP_Cotizador_Product (
	ProductId	bigint, 
	ProductName	text, 
	ProductPublishedDate varchar(10), 
	ProductUpdatedDate varchar(10), 
	ProductStatus varchar(25), 
	ProductImage varchar(500), 
	ProductFile varchar(500), 
	CatalogueId bigint, 
	CategoryId bigint);

create table TMP_Cotizador_ProductFeature (
	ProductId bigint,
	FeatureTypeId bigint,
	FeatureTypeName varchar(500),
	FeatureTypeCondition varchar(30),
	RequiredSubValue varchar(10),
	FeatureValueId bigint,
	FeatureValueName varchar(500),
	FeatureValueNameSub varchar(500));
