/*
##############################################################
			TgcSceneImporter			
--------------------------------------------------------------
Importador de escenario estático con format TgcScene.xml
##############################################################

General e Instalación
---------------------
Autor: Matías Leone
Plataforma: 3ds Max 2011
Instalación: Copiar archivo TgcSceneImporter.ms en "..\Autodesk\3ds Max 2011\Scripts\Startup\"
También necesita el archivo MaxScriptUtils.ms. Copiarlo a la carpeta anterior.
Ejecución: 
	Abrir 3ds Max
	Ir a la sección de Scripts de la barra de herramientas de la derecha (Utilities: ícono del Martillo)
	Elegir "MAXScript"
	De la lista inferior de "Utilities" elegir "TGC-Scene Importer"
	Aparecerá abajo todo el conjunto de herramientas del Plugin.


Uso del Plugin
--------------
El plugin levanta archivos XML de formato "-TgcScene.xml" y crea todos los modelos de MAX.
Actualmente no soporta:
	- Lightmaps
	- Normales
	- Portales
	- Instancias de modelos

No soporta que el XML tenga caracteres UTF-8 (por ej: Ñ)
*/


--Incluir utilidades generales
include "MaxScriptUtils.ms"


utility TgcSceneImporter "TGC-Scene Importer"
(

	--######### GLOBALS ###########
	local FILE_EXT = "-TgcScene.xml"

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* Cargar un Bitmap de DiffuseMap y aplicarlo a un Material.
	* Carga tambien su OpacityMap si corresponde
	*/
	function loadBitmapInMaterial mat bitmapFileName aphaBlendEnable = (
		local matBitmap = bitmapTexture filename:bitmapFileName
		mat.diffuseMap = matBitmap
		--mat.mapEnables[2] = true
		--showTextureMap mat bmpTex true

		--Ver si tiene OpacityMap
		if aphaBlendEnable == true then (
			matBitmap.monoOutput = 1 --Alpha
			mat.opacityMap = matBitmap
		)
	)
	
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* Parsea el contenido en string de un tag de la forma: 11.1305 0.0 9.03702 -2.26105 0.0 14.1578 -13.3915 0.0 5.12076
	* y lo convierte en un array de floats
	* plusOne en true suma 1 a todos los valores para convertir indices de DirectX a MAX
	*/
	function parseFloatArray content plusOne = (
		local floatArray = #()
		local strArray = filterString content " "
		for s in strArray do (
			local f = s as float
			if plusOne == true then (
				f = f + 1
			)
			append floatArray f
		)
		
		floatArray
	)
	
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* Parsea el contenido en string de un tag de la forma: 11.1305 0.0 9.03702 -2.26105 0.0 14.1578 -13.3915 0.0 5.12076
	* y lo convierte en un array de point3 tomado de a 3 valores.
	* plusOne en true suma 1 a todos los valores para convertir indices de DirectX a MAX
	* invertYZ en true invierte Y por Z
	*/
	function convertToPoint3Array content plusOne invertYZ= (
		local point3Array = #()
		local strArray = filterString content " "
		
		local i = 1
		while i <= strArray.count do (
			local x = strArray[i] as float
			local y = strArray[i + 1] as float
			local z = strArray[i + 2] as float
			if plusOne == true then (
				x = x + 1
				y = y + 1
				z = z + 1
			)
			if invertYZ == true then (
				local aux = y
				y = z
				z = aux
			)
			append point3Array (point3 x y z)
			i = i + 3
		)

		point3Array
	)
	
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* FUNCION PRINCIPAL
	*
	* Importa todos los meshes del XML "-TgcScene.xml" y crea objetos de MAX
	*/
	function importScene = (
		
		--Obtener path de archivo "-TgcScene.xml"
		local fileName = getOpenFileName caption:"Open Scene file" types:"-TgcScene.xml|*-TgcScene.xml"
		if fileName == undefined then (
			return 0
		)
		
		--Cargar XML
		local doc = XmlDocument()
		doc.loadFromFile fileName
		
		--Llamar garbage collector para limpiar todas las variables temporales del parseo de XML
		gc
		
		--Obtener directorio de texturas
		local texturesExportNode = doc.root.getChild "texturesExport"
		local texturesDir = (substring filename 1 (Utils_lastIndexOf filename "\\")) + (texturesExportNode.getAttributeValue "dir") + "\\"
		
		--Leer Materials
		local materialsNode = doc.root.getChild "materials"
		local mNodes = materialsNode.getChildren "m"
		local createdMaterials = #()
		for mNode in mNodes do (
			
			local matName = mNode.getAttributeValue "name"
			local matType = mNode.getAttributeValue "type"
			
			--Crear Standardmaterial
			local mat = undefined
			if matType == "Standardmaterial" then (

				--Crear material de MAX
				mat = standardmaterial name:matName showInViewport:true
				
				--Cargar Bitmap
				local matBitmapNode = mNode.getChild "bitmap"
				if matBitmapNode != undefined then (
					local matBitmapFileName = texturesDir + matBitmapNode.content
					local alphaBlendEnableString = mNode.getChildContent "alphaBlendEnable"
					local matAlphaBlendEnable = false
					if alphaBlendEnableString != undefined then (
						matAlphaBlendEnable = alphaBlendEnableString as booleanClass
					)
					loadBitmapInMaterial mat matBitmapFileName matAlphaBlendEnable
				)

			)
			--Crear Multimaterial
			else if matType == "Multimaterial" then (
				
				--Crear SubMaterials
				local subMNodes = mNode.getChildren "subM"
				local subMaterials = #()
				for subMNode in subMNodes do (
					
					local subMatName = subMNode.getAttributeValue "name"
					local subMatType = subMNode.getAttributeValue "type"
					if subMatType != "Standardmaterial" then (
						messagebox subMatType
						messagebox ("The SubMaterial: " + subMatName + " is not a StandardMaterial")
						return 0
					)
					
					--Crear material de MAX
					local subMat = standardmaterial name:subMatName showInViewport:true
					
					--Cargar Bitmap
					local subMatBitmapNode = subMNode.getChild "bitmap"
					

					if subMatBitmapNode != undefined then (
						local subMatBitmapFileName = texturesDir + subMatBitmapNode.content
						local alphaBlendEnableString = subMNode.getChildContent "alphaBlendEnable"
						if alphaBlendEnableString != undefined then (
							matAlphaBlendEnable = alphaBlendEnableString as booleanClass
						)
						loadBitmapInMaterial subMat subMatBitmapFileName subMatAlphaBlendEnable
					)
					append subMaterials subMat
				)
				
				--Crear material de MAX
				mat = multimaterial  name:matName showInViewport:true numsubs:(subMaterials.count)
				mat.materialList = subMaterials
			)
			
			append sceneMaterials mat 
			append createdMaterials  mat
		)
		
		
		--Leer Meshes
		local meshesNode = doc.root.getChild "meshes"
		local meshNodes = meshesNode.getChildren "mesh"
		local createdMeshes = #()
		for meshNode in meshNodes do (
			
			--Obtener atributos generales
			local meshName = meshNode.getAttributeValue "name"
			local meshLayerName = meshNode.getAttributeValue "layer"
			local meshType = meshNode.getAttributeValue "type"
			local meshMatId = (meshNode.getAttributeValue "matId" as integer)
			local meshColor = meshNode.getAttributeValue "color"
			
			--Type
			if meshType == "Instance" then (
				messagebox ("Instance meshes are not supported yet. Mesh name: " + meshName)
				return 0
			)
			
			--Craer mesh de MAX y aplicar vertices y coordinatesIdx
			local meshFaces = convertToPoint3Array (meshNode.getChildContent "coordinatesIdx") true false
			local meshVertices = convertToPoint3Array (meshNode.getChildContent "vertices") false true
			local meshMAX = mesh name:meshName vertices:meshVertices faces:meshFaces
			
			--Material, tverts y materialIDs
			if meshMatId != -1 then (
				local meshMaterial = createdMaterials[meshMatId + 1]
				local matClass = classof meshMaterial
		
				--Obtener texCoords y crear array con UVW tverts
				local texCoords = parseFloatArray (meshNode.getChildContent "texCoords") false
				local meshTVerts = #()
				local texCoordsCount = (texCoords.count / 2 - 1)
				for i = 0 to texCoordsCount do (
					local u = texCoords[i  * 2 + 1]
					local v = texCoords[i * 2 + 2]
					v = 1 - v --Invertimos V
					local w = 0
					append meshTVerts (point3 u v w)
				)
				
				--StandardMaterial
				local meshMaterialIDs
				if matClass == StandardMaterial then (
					
					--Crear array con todos matID = 1
					meshMaterialIDs = #()
					for i = 1 to meshFaces.count do (
						append meshMaterialIDs 1
					)
					
				--MultiMaterial
				) else (
					
					--Tomar matIds de mesh
					meshMaterialIDs = parseFloatArray (meshNode.getChildContent "matIds") true
				)

				--Aplicar materialIDs y tverts
				setMesh meshMAX materialIDs:meshMaterialIDs tverts:meshTVerts
				
				--Cargar TVFaces con textCoordsIdx
				buildTVFaces meshMAX 
				local textCoordsIdx = convertToPoint3Array (meshNode.getChildContent "textCoordsIdx") true false
				for i = 1 to textCoordsIdx.count do (
					setTVFace meshMAX i textCoordsIdx[i]
				)
				
				--Aplicar material
				meshMAX.material = meshMaterial
				
			--Si no tiene Material
			) else (
				--Crear igual las TV Faces, de forma generica
				meshMAX.numtverts = meshMAX.numverts
				buildTVFaces meshMAX 
			)
			
			--Aplicar color
			meshColor = substring meshColor 2 (meshColor.count - 2)
			local meshColorArray = filterString meshColor ","
			meshMax.wireColor = color (meshColorArray[1] as float) (meshColorArray[2] as float) (meshColorArray[3] as float)
			
			--Crear layer si es necesario y aplicar a mesh
			if meshLayerName != undefined then (
				local layer = LayerManager.getLayerFromName meshLayerName
				if layer == undefined then (
					layer = LayerManager.newLayer()
					layer.setName meshLayerName
				)
				layer.addnode meshMax
			)

			/*
			--Aplicar normales PENDIENTE!!!
			local meshNormals = convertToPoint3Array (meshNode.getChildContent "normals") false true
			for i = 1 to meshNormals.count do (
				setNormal meshMAX i meshNormals[i]
			)
			*/		
			
			
			--Actualizar estado de mesh
			update meshMAX
			append createdMeshes meshMAX
		)
		
		--Seleccionar meshes creados
		select createdMeshes
		

		messagebox "Scene imported OK"
	)
	
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	
--############################################################
--####################### USER INTERFACE  #########################
--############################################################
	
	
	--######### GENERAL SETTINGS ###########
	--Widgets
	button w_importScene "Import Scene" width:150 height:20
	
	
	--Event: button Import
	on w_importScene pressed do
	(
		--importar
		importScene()
	)

)