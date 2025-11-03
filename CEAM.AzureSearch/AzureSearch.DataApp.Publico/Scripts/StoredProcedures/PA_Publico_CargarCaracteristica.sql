CREATE OR ALTER PROCEDURE [dbo].[PA_Publico_CargarCaracteristica]
AS
/*
=============================================================================================
	FECHA			USUARIO				      VERSION		            REQ
=============================================================================================
	01/06/2021		Willman Rojas		        1.0			    Creación del Stored Procedure
	10/08/2021		Catterin Ubillus	        1.1			    Agregar tabla de log
	03-09-2021		Catterin Ubillus	        1.2			    Quitar el tipo GENERICA segun la logica anterior
    27/11/2023      Rogger Torres Gonzales      1.3				R-011272-2022-Registro-caracteristicas-no-obligatoria
=============================================================================================
*/
BEGIN

	DECLARE @UsuarioProcesa VARCHAR(255) = 'SISTEMA-SERVIDOR';
	DECLARE @FechaCierre DATE = dbo.getfecha();
	
	DELETE FROM TMP_Publico_Caracteristica;

	INSERT INTO TMP_Publico_Caracteristica (
		ProductId,
		FeatureTypeId,
		FeatureTypeName,
		FeatureTypeRequiredSubValue,
		FeatureValueId,
		FeatureValueName,
		FeatureValueImg,
        FeatureType)
	SELECT
		CATFICPROD.N_CatalogoProducto AS [ProductId],  
		carac.id_Caracteristica AS [FeatureTypeId],  
		carac.C_Denominacion AS [FeatureTypeName],  
		carac.C_RequiereSubValor AS [FeatureTypeRequiredSubValue],  
		val.id_valcaracteristica AS [FeatureValueId],  
		val.c_Valor AS [FeatureValueName],  
		val.C_SubValorIMG AS [FeatureValueImg],
		carac.C_TipoCaracteristica AS [FeatureType]
	FROM T_CatFichaProducto AS CATFICPROD WITH(NOLOCK)  
	INNER JOIN T_Caracteristica AS carac WITH(NOLOCK) ON carac.id_Caracteristica = CATFICPROD.n_Caracteristica  
	INNER JOIN T_ValCaracteristica AS val WITH (NOLOCK) ON val.id_valcaracteristica = CATFICPROD.n_valcaracteristica  
	WHERE   CATFICPROD.C_Estado = 'ACTIVO' 
    AND     carac.C_Estado = 'ACTIVO' 
	--AND     carac.C_TipoCaracteristica = 'GENERICA' 
	AND     val.C_Estado = 'ACTIVO' 
	AND     CATFICPROD.N_CatalogoProducto IN (select ProductId from TMP_PUBLICO_Product)
		
	INSERT INTO T_ProcesosAutomaticosLog 
	VALUES(@UsuarioProcesa, @FechaCierre, 'PA_Publico_CargarCaracteristica', '2. Procesa de job Buscador Publico - Carga caracteristicas', '@UsuarioProcesa : '+@UsuarioProcesa +', @FechaCierre: '+CONVERT(VARCHAR(10), @FechaCierre, 103), 'Procesos ejecutado satisfactoriamente, ' + CONVERT(VARCHAR, @@ROWCOUNT) + ' registros procesados', @UsuarioProcesa, dbo.GetFecha(), 'SISTEMA');

	SELECT 1
END
