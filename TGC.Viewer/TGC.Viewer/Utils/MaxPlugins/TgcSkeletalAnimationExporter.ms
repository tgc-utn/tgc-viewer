/*
#####################################################
			TgcSkeletalAnimationExporter			
---------------------------------------------------
Exportador de mesh animado con Skeletal Animation 
#####################################################


General e Instalación
---------------------
Autor: Matías Leone
Plataforma: 3ds Max 2009
Instalación: Copiar archivo TgcSkeletalAnimationExporter.ms en "..\Autodesk\3ds Max 2009\Scripts\Startup\"
También necesita el archivo MaxScriptUtils.ms. Copiarlo a la carpeta anterior.
Ejecución: 
	Abrir 3ds Max
	Ir a la sección de Scripts de la barra de herramientas de la derecha (Utilities: ícono del Martillo)
	Elegir "MAXScript"
	De la lista inferior de "Utilities" elegir "TGC-SkeletalAnimation Exporter"
	Aparecerá abajo todo el conjunto de herramientas del Plugin.


Uso del Plugin
--------------
El plugin genera dos tipos de archivos:
	
1) TgcSkeletalMesh.xml
Este archivo tiene toda la información de la malla: triángulos, coordendas de textura, materials, etc.
Se exporta la siguiente información de la malla: triángulos y vértices, coordenadas de textura, colores por vértice, Material de la malla.
No se exportan normales.
Solo se puede exportar una malla. Si el modelo se compone por mas de un objeto, utilizar Attach para juntar todos en un único mesh.
Además tiene la información del esqueleto: huesos, postura inicial, weights de cada vértice.
El modelo tiene que estar adjuntado a un conjunto de huesos mediante el modificador Skinning.
Soporta que un vértice tenga más de un hueso que lo influya.
El esqueleto puede ser un sistema de huesos propio o un Biped.
El esqueleto tiene que estar en la postura Inicial/Default.
Solo soporta Materials del tipo StandardMaterial o MultiMaterial.
Para que un Mesh tenga mas de una textura, utilizar un solo Material del tipo MultiMaterial.
De ambas solo se exporta el DiffuseMap y el Bitmap.
Las texturas pueden ser copiadas de su posición original del FileSystem a una carpeta relativa a la ubicación
del archivo XML generado.
Actualmente solo soporta exportar la información en formato DirectX
La malla tiene que estar seleccionada antes de exportar.
Es recomendable que la malla se ubique en el origen de coordenadas.


2) TgcSkeletalAnim.xml
Archivo que contiene una animación particular del modelo.
Normalmente existirá un archivo TgcSkeletalMesh.xml y N archivos TgcSkeletalAnim.xml, uno por cada animación.
El nombre de la animación que pide el plugin debe coincidir con el nombre del archivo guardado (Sin la extensión "-TgcSkeletalAnim.xml")
Se exporta la posición y rotación de los huesos en sus KeyFrames.
La animación tiene que haberse construído en base a la postura Inicial/Default del archivo TgcSkeletalMesh.xml
Las variaciones de posición y rotación de cada hueso en cada KeyFrame se toman en base a la postura inicial del esqueleto en el archivo 
TgcSkeletalMesh.xml. Por lo tanto el esqueleto tiene que arrancar orientado de la misma forma (los huesos pueden estar en una posición
inicial diferente de la inicial, pero el cuerpo en general tiene que estar sobre la misma posicion y orientación en los ejes cartesianos).
Actualmente solo soporta exportar la información en formato DirectX.
Dado que un archivo TgcSkeletalAnim.xml solo exporta la animación del esqueleto, un archivo TgcSkeletalAnim.xml puede utilizarse para varios
modelos distintos. Las animaciones pueden reutilizarse entre varios archivos TgcSkeletalMesh.xml.
Para poder reutilizar, todos los modelos del archivo TgcSkeletalMesh.xml deben utilizar el mismo esqueleto, con la misma cantidad de huesos.
La malla tiene que estar seleccionada antes de exportar.
La línea de tiempo (TrackView) tiene que acotarse y que solo quede el rango de tiempo utilizado.

*/


--Incluir utilidades generales
include "MaxScriptUtils.ms"


utility TgcSkeletalAnimationExporter "TGC-SkeletalAnimation Exporter"
(

	--######### GLOBALS ###########
	local MESH_FILE_EXT = "-TgcSkeletalMesh.xml"
	local ANIM_FILE_EXT = "-TgcSkeletalAnim.xml"
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------

	/*
	* Estructura con datos de un triángulo de una malla
	*/
	struct TgcMeshFace
	(
		index,
		face,
		tvFace,
		vcFace,
		faceMatId
	)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

	/*
	* Funcion para comparar dos triángulos TgcMeshFace y devolver el que menor faceMatId tiene.
	* Utilizada para ordernar con qsort los triángulos de una malla
	*/
	function compareTgcMeshFace t1 t2 = (
		case of (
			(t1.faceMatId < t2.faceMatId): -1
			(t2.faceMatId > t1.faceMatId): 1
			default: 0
		)
	)	
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------		
	
	
	--Estructura auxiliar con huesos del esqueleto
	struct TgcBone
	(
		boneNode,
		parentNode,
		childNodes,
		parentId,
		localTransform,
		skinModId
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	--Recorre recursivamente los nodos del esqueleto y devuelve los hijos en orden Deep-Fist
	function skeletonTreeLoop currentTgcNode = (
		childs = #()
		append childs currentTgcNode
		
		if(currentTgcNode.childNodes.count > 0) then (
			for childTgc in currentTgcNode.childNodes do (
				childChilds = skeletonTreeLoop childTgc
				join childs childChilds
			)
		)
		
		return childs
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	--Crea un array de todos los huesos del esqueleto, ordenados desde el Root hacia las hojas
	function createBoneList skinModifier = (
		totalBones = skinOps.GetNumberBones skinModifier
		
		--Guardar todos los Bone en una lista
		boneList = #()
		for i = 1 to totalBones do (	
			boneName = skinOps.GetBoneName skinModifier i 0
			boneNode = getNodeByName boneName
			append boneList boneNode
		)
				
		--Cargar el padre y los hijos de cada nodo
		bonesTree = #()
		rootBone = undefined
		for i = 1 to totalBones do (
			boneNode = boneList[i]
			
			--Ver si el parent del nodo es un hueso mas o es el root
			parentNode = undefined
			if (boneNode.parent != undefined) then (
				--Buscar que el nodo padre exista en la lista de nodos, sino es el Root
				for j = 1 to totalBones do (
					boneNode2 = boneList[j]
					if(boneNode.parent.name == boneNode2.name) then (
						parentNode = boneNode2
						exit
					)
				)
			)

			--Crear nodo del arbol
			tgcBoneItem = TgcBone boneNode:boneNode parentNode:parentNode childNodes:#() parentId:0 skinModId:i
			append bonesTree tgcBoneItem
			
			--guardar bone Root y acumular
			if(parentNode == undefined) then (
				--si ya habia otro nodo root, algo esta mal en el esqueleto
				if(rootBone != undefined) then (
					messagebox "There are more than one Root Bone"
					return 0
				)
				rootBone = tgcBoneItem
			)
		)
		
		--Cargar hijos de cada nodo
		for tgcBoneItem in bonesTree do (
			childNodes = #()
			
			for tgcBoneChild in bonesTree do (
				if (tgcBoneChild.parentNode != undefined) then (
					if(tgcBoneChild.parentNode.name == tgcBoneItem.boneNode.name) then (
						append childNodes tgcBoneChild
					)
				)
			)
			tgcBoneItem.childNodes = childNodes
		)
		
		

		--Armar arbol en orden partiendo del Root
		bonesTreeOrdered = skeletonTreeLoop rootBone
		
		--Armar IDs de parent de cada nodo y localTransform
		for tgcBoneItem in bonesTreeOrdered do (
			if(tgcBoneItem.parentNode == undefined) then (
				--Es el root
				tgcBoneItem.parentId = 0
				tgcBoneItem.localTransform = tgcBoneItem.boneNode.transform
			) else (
				--Buscar id del padre
				for j = 1 to bonesTreeOrdered.count do (
					tgcBoneItem2 = bonesTreeOrdered[j]
					boneNode2 = tgcBoneItem2.boneNode
					if(boneNode2.name == tgcBoneItem.parentNode.name) then (
						tgcBoneItem.parentId = j
						
						--Calcular localTransform relativa al padre
						tgcBoneItem.localTransform = tgcBoneItem.boneNode.transform * inverse tgcBoneItem.boneNode.parent.transform
						
						exit
					)
				)
				
			)
		)
		
		return bonesTreeOrdered
	)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* Crea un BoundingBox que contemple el tamaño de la malla en todos los cuadros de animacion
	* de sus huesos
	*/
	function getAnimationBoundingBox selectedMesh exportFormat = (
	
		--Obtener el BoundingBox de la malla en cada frame de animación
		boundingBoxList = #()
		for t = animationrange.start to animationrange.end do (
			--BoundingBox
			tmesh = at time t snapshotAsMesh selectedMesh
			tgcBB = Utils_getMeshBoundingFromVertices tmesh exportFormat
			append boundingBoxList tgcBB
		)
		
		--Calcular un BoundingBox que englobe a todos
		tgcFinalBB = Utils_createBoundingBoxForAll boundingBoxList
		return tgcFinalBB
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* Obtiene la lista KeyFrames de un hueso
	*/
	function getBoneKeys tgcBoneNode = (
		keyFrames = #()
		boneNode = tgcBoneNode.boneNode
		
		--Caso comun
		if(tgcBoneNode.parentNode != undefined) then (
			for k in boneNode.transform.controller.keys do (
				append keyFrames (Utils_formatKeyFrame k.time)
			)
			
		--Si es el hueso root la cantidad de KeyFrames se obtiene de forma mas complicada
		) else (
			for k in boneNode.transform.controller.vertical.controller.keys do (
				appendIfUnique keyFrames (Utils_formatKeyFrame k.time)
			)
			for k in boneNode.transform.controller.horizontal.controller.keys do (
				appendIfUnique keyFrames (Utils_formatKeyFrame k.time)
			)
			for k in boneNode.transform.controller.turning.controller.keys do (
				appendIfUnique keyFrames (Utils_formatKeyFrame k.time)
			)
		)
		
		return keyFrames
	)

---------------------------------------------------------------------------------------------------------------------------------------------------------------

	--Exporta toda la informacion del Mesh estatico
	function exportMesh 
		objectList:selection exportFormat:1 copyTextures:true textureDirName:"Textures" texturePreffix:"" = (
			
		if objectList.count == 0 then (
			messagebox "You must select at least one mesh."
			return 0
		)
		
		if objectList.count != 1 then (
			messagebox "You must select only one mesh."
			return 0
		)
		selectedMesh = selection[1]

		--Check Skin Modifier
		max modify mode
		meshModifiers = selectedMesh.modifiers
		skinModifier = undefined
		for i=1 to meshModifiers.count do (
			if (classof meshModifiers[i] as string == "Skin") do (
				skinModifier = meshModifiers[i]
				modPanel.setCurrentObject skinModifier
				exit
			)
		)
		if (skinModifier == undefined) do (
			messagebox "There is no SKIN modifier for the selected object"
			return 0
		)
		
		
		--GET OUTPUT FILE
		outName = GetSaveFileName()
		if outName == undefined then (
			return 0
		)

		--check file extension
		if ( findString outName MESH_FILE_EXT ) == undefined then (
			outName = outName + MESH_FILE_EXT
		)
		outFile = createfile outName
		sceneDir = getFilenamePath outName
		
		--Utilizar datos segun frame inicial
		set time 0
		
		--Obtener lista de materials
		tgcMaterials = Utils_createMaterialList objectList

		--Start exporting
		format "<tgcSkeletalMesh>\n" to:outFile

		--Crear directorios de texturas y lightmaps
		if copyTextures then (
			texturesDir = sceneDir + "\\" + textureDirName + "\\"
			deleteFile texturesDir
			makeDir texturesDir
		)
		
		--Exportar info de directorios de texturas y lightmaps
		format "\t<texturesExport enabled='%' dir='%'/>\n" (copyTextures as string) textureDirName to:outFile
		
		
		--######### MATERIALS ###########
		
		--Exportar sceneMaterials utilizadas
		format "\t<materials count='%'>\n" tgcMaterials.count to:outFile
		for i = 1 to tgcMaterials.count do (
			tgcMat = tgcMaterials[i]
			m = tgcMat.materialNode
			
			--Exportar Standard Material
			if tgcMat.subMaterials.count == 0 then (
			
				format "\t\t<m name='%' type='%'>\n" m.name tgcMat.materialClass to:outFile
				
				--Ambient
				ambColor = m.ambient
				format "\t\t\t<ambient>[%,%,%,%]</ambient>\n" ambColor.red ambColor.green ambColor.blue ambColor.alpha to:outFile

				--Diffuse
				diffuseColor = m.diffuse
				format "\t\t\t<diffuse>[%,%,%,%]</diffuse>\n" diffuseColor.red diffuseColor.green diffuseColor.blue diffuseColor.alpha to:outFile

				--Specular
				specularColor = m.specular
				format "\t\t\t<specular>[%,%,%,%]</specular>\n" specularColor.red specularColor.green specularColor.blue specularColor.alpha to:outFile
				
				--Opacity
				format "\t\t\t<opacity>%</opacity>\n" m.opacity to:outFile
				
				--Comprobar si el material tiene un Bitmap en DiffuseMap
				if tgcMat.withBitmap == true then (
					
					--fileName
					filename = m.diffuseMap.bitmap.filename
					textureName = filenameFromPath filename
					
					--UV Tiling
					uTiling = m.diffuseMap.coords.U_Tiling
					vTiling = m.diffuseMap.coords.V_Tiling
					
					--UV Offset
					uOffset = m.diffuseMap.coords.U_Offset
					vOffset = m.diffuseMap.coords.V_Offset
					
					--Exportar Bitmap
					format "\t\t\t<bitmap uvTiling='[%,%]' uvOffset='[%,%]'>%</bitmap>\n" uTiling vTiling uOffset vOffset textureName to:outFile
					
					--Copiar Bitmap a carpeta de texturas
					if copyTextures then (
						Utils_copyTexture m.diffuseMap.bitmap (texturesDir + "\\" + texturePreffix + textureName)
					)
				)
				
				format "\t\t</m>\n" to:outFile
				

			--Exportar Multi/Sub Object Material 
			) else (

				format "\t\t<m name='%' type='%'>\n" m.name tgcMat.materialClass to:outFile
				
				for tgcSubMat in tgcMat.subMaterials do (
					
					subMat = tgcSubMat.materialNode
					
					--Export sub-material
					format "\t\t\t<subM name='%' type='%'>\n" subMat.name tgcSubMat.materialClass to:outFile
					
					--Ambient
					ambColor = subMat.ambient
					format "\t\t\t\t<ambient>[%,%,%,%]</ambient>\n" ambColor.red ambColor.green ambColor.blue ambColor.alpha to:outFile

					--Diffuse
					diffuseColor = subMat.diffuse
					format "\t\t\t\t<diffuse>[%,%,%,%]</diffuse>\n" diffuseColor.red diffuseColor.green diffuseColor.blue diffuseColor.alpha to:outFile

					--Specular
					specularColor = subMat.specular
					format "\t\t\t\t<specular>[%,%,%,%]</specular>\n" specularColor.red specularColor.green specularColor.blue specularColor.alpha to:outFile
					
					--Opacity
					format "\t\t\t\t<opacity>%</opacity>\n" subMat.opacity to:outFile
					
					--Comprobar si el material tiene un Bitmap en DiffuseMap
					if tgcSubMat.withBitmap == true then (
						
						--fileName
						filename = subMat.diffuseMap.bitmap.filename
						textureName = filenameFromPath filename
						
						--UV Tiling
						uTiling = subMat.diffuseMap.coords.U_Tiling
						vTiling =subMat.diffuseMap.coords.V_Tiling
						
						--UV Offset
						uOffset = subMat.diffuseMap.coords.U_Offset
						vOffset = subMat.diffuseMap.coords.V_Offset
						
						--Exportar Bitmap
						format "\t\t\t\t<bitmap uvTiling='[%,%]' uvOffset='[%,%]'>%</bitmap>\n" uTiling vTiling uOffset vOffset textureName to:outFile
						
						--Copiar Bitmap a carpeta de texturas
						if copyTextures then (
							Utils_copyTexture subMat.diffuseMap.bitmap (texturesDir + "\\" + texturePreffix + textureName)
						)
					)
					
					format "\t\t\t</subM>\n" to:outFile
				)
				
				format "\t\t</m>\n" to:outFile

			)
		)
		format "\t</materials>\n" to:outFile
		
		
		
		
		--######### MESH ###########

		--Ver si tiene material
		tgcMaterialItem = undefined
		if (Utils_meshHasMaterial selectedMesh) == true then (
		
			--Obtener TgcMaterial correspondiente
			tgcMaterialItem = Utils_getMeshMaterial selectedMesh tgcMaterials
		)
		
		--Material id y maxMatId
		matId = -1
		maxMatId = -1
		if tgcMaterialItem != undefined then (
			matId = 0
			if tgcMaterialItem.subMaterials.count > 0 then (
				maxMatId = tgcMaterialItem.subMaterials.count
			) else (
				maxMatId = 1
			)
		)
		
		--BoundingBox
		tgcBB = Utils_getMeshBoundingBox selectedMesh exportFormat
		
		--Check visibility flag
		visibility = 1.0
		if (selectedMesh[1] != undefined and selectedMesh[1].controller != undefined) then (
			visibility = selectedMesh.visibility.controller.value
		)
		
		--WireColor 
		wireColor = selectedMesh.wireColor
		
		--take snapshot
		tmesh = snapshotAsMesh selectedMesh
		
		--Start mesh
		format ("\t<mesh name='%' matId='%' color='[%,%,%]' visibility='%'>\n") selectedMesh.name matId wireColor.red wireColor.green wireColor.blue visibility to:outFile
		
		--BoundingBox
		format "\t\t<boundingBox min='%' max='%'/>\n"  tgcBB.pMin tgcBB.pMax to:outFile
		
		
		--Obtener toda la informacion de: Coordinates Index, Texture Coordinates Index, Colors Index and Material Ids
		tgcMeshFaces = #()
		withColorPerVert = getNumCPVVerts tmesh
		for t = 1 to tmesh.numfaces do (
			--Coordinates Index
			face = getFace tmesh t
			
			--Texture Coordinates Index
			texFace = getTVFace tmesh t
			
			--Colors Index
			if withColorPerVert > 0 then (
				colorFace = getVCFace tmesh t
			) else (
				colorFace = point3 1 1 1
			)
			
			--Material ID
			faceMatId = getFaceMatID tmesh t
			if faceMatId > maxMatId then (
				faceMatId = maxMatId
			)
			
			tgcMeshFaceObj = TgcMeshFace index:t face:face tvFace:texFace vcFace:colorFace faceMatId:faceMatId
			append tgcMeshFaces tgcMeshFaceObj
		)
		
		--Ordenar triángulos de la malla según su material ID, para que vayan todos en batches de textura
		qsort tgcMeshFaces compareTgcMeshFace
		
		
		--Armar streams de strings con los valores de cada triángulo
		streamCoordinatesIndex = stringstream ""
		streamTexCoordsIndex = stringstream ""
		streamColorsIndex = stringstream ""
		streamMaterialIds = stringstream ""
		for tgcMeshFaceObj in tgcMeshFaces do (
			face = tgcMeshFaceObj.face
			format "% % % " (face.x as integer - 1) (face.y as integer - 1) (face.z as integer - 1) to:streamCoordinatesIndex 
			
			texFace = tgcMeshFaceObj.tvFace
			format "% % % " (texFace.x as integer - 1) (texFace.y as integer -1) (texFace.z as integer -1) to:streamTexCoordsIndex 
			
			colorFace = tgcMeshFaceObj.vcFace
			format "% % % " (colorFace.x as integer -1) (colorFace.y as integer -1) (colorFace.z as integer -1) to:streamColorsIndex 
			
			faceMatId = tgcMeshFaceObj.faceMatId
			format "% " (faceMatId - 1) to:streamMaterialIds 
		)
		format "\t\t<coordinatesIdx count='%'>%</coordinatesIdx>\n" (tmesh.numfaces * 3) (streamCoordinatesIndex as string) to:outFile
		format "\t\t<textCoordsIdx count='%'>%</textCoordsIdx>\n" (tmesh.numfaces * 3) (streamTexCoordsIndex as string) to:outFile
		format "\t\t<colorsIdx count='%'>%</colorsIdx>\n" (tmesh.numfaces * 3) (streamColorsIndex as string) to:outFile
		--Solo imprimir indices de MaterialId si tiene mas de una
		if maxMatId > 1 then (
			format "\t\t<matIds count='%'>%</matIds>\n" tmesh.numfaces (streamMaterialIds as string) to:outFile
		) else (
			format "\t\t<matIds count='%'>%</matIds>\n" maxMatId 0 to:outFile
		)
		

		--Vertices
		streamVertices = stringstream ""
		for j = 1 to tmesh.numverts do (
			vert = getVert tmesh j
			
			newVert = Utils_formatPositionValue vert exportFormat
			format "% % % " newVert.x newVert.y newVert.z to:streamVertices
		)
		format "\t\t<vertices count='%'>%</vertices>\n" (tmesh.numverts * 3) (streamVertices as string) to:outFile
		
		
		--Texture coordinates
		streamTexCoordinates = stringstream ""
		for k = 1 to tmesh.numtverts do (
			texCoord = getTVert tmesh k
			
			newTexCoord = Utils_formatTextureCoordinates texCoord exportFormat
			format "% % " newTexCoord.x newTexCoord.y to:streamTexCoordinates
		)
		format "\t\t<texCoords count='%'>%</texCoords>\n" (tmesh.numtverts * 2) (streamTexCoordinates as string) to:outFile

		
		--Colors
		streamColors = stringstream ""
		numCPVerts = getNumCPVVerts tmesh
		if numCPVerts > 0 then (
			for c = 1 to numCPVerts do (
				vertColor = getVertColor tmesh c
				format "% % % " vertColor.red vertColor.green vertColor.blue to:streamColors
			)
		) else (
			numCPVerts = 1
			--Si no hay colores por vertice ni texturas, utilizamos el WireColor
			if matId == -1 then (
				format "% % % " wireColor.red wireColor.green wireColor.blue to:streamColors
			--Si no hay colores por vertice pero si hay texturas, exportamos Blanco para no alterar los colores de la textura
			) else (
				format "% % % " 255 255 255 to:streamColors
			)
		)
		format "\t\t<colors count='%'>%</colors>\n" (numCPVerts * 3) (streamColors as string) to:outFile
		
		--Normals
		local streamNormals = stringstream ""
		local meshNormals = #()
		for tgcMeshFaceObj in tgcMeshFaces do (
			local t = tgcMeshFaceObj.index
			--Sacar normales del Smooth group
			local sg = getFaceSmoothGroup tmesh t
			if sg != 0 then (
				local tnorm = meshop.getFaceRNormals tmesh t
			
				--Formatear normales
				local normal1 = Utils_formatPositionValue (normalize tnorm[1]) exportFormat
				local normal2 = Utils_formatPositionValue (normalize tnorm[2]) exportFormat
				local normal3 = Utils_formatPositionValue (normalize tnorm[3]) exportFormat
			
				format "% % % " normal1.x normal1.y normal1.z to:streamNormals
				format "% % % " normal2.x normal2.y normal2.z to:streamNormals
				format "% % % " normal3.x normal3.y normal3.z to:streamNormals
				
				--Guardarlas en array para calcular luego tangent y binormal
				append meshNormals normal1
				append meshNormals normal2
				append meshNormals normal3
			)
			--Si no tiene Smooth group, sacar normal de la cara
			else
			(
				local tnorm = getFaceNormal tmesh t
				--Formatear normales
				local normal1 = Utils_formatPositionValue (normalize tnorm) exportFormat
			
				--Imprimir 3 veces la misma
				format "% % % " normal1.x normal1.y normal1.z to:streamNormals
				format "% % % " normal1.x normal1.y normal1.z to:streamNormals
				format "% % % " normal1.x normal1.y normal1.z to:streamNormals
				
				--Guardarlas en array para calcular luego tangent y binormal
				append meshNormals normal1
				append meshNormals normal1
				append meshNormals normal1
			)
		)
		format "\t\t<normals count='%'>%</normals>\n" (meshNormals.count * 3) (streamNormals as string) to:outFile
		
		--Tangents y Binormals
		local streamTangents = stringstream ""
		local streamBinormals = stringstream ""
		for f = 1 to tgcMeshFaces.count do (
			--Normal del primer vertice del triangulo
			local normal1 = meshNormals[f * 3]
			
			--Posiciones de los 3 vertices del triangulo
			local face = tgcMeshFaces[f].face
			local pos1 = getVert tmesh face.x
			local pos2 = getVert tmesh face.y
			local pos3 = getVert tmesh face.z
			
			--Coordenadas de textura de los 3 vertices del triangulo
			local texFace = tgcMeshFaces[f].tvFace
			local texCoord1 = getTVert tmesh texFace.x
			local texCoord2 = getTVert tmesh texFace.y
			local texCoord3 = getTVert tmesh texFace.z
			
			--Calcular tangent y binormal
			local results = Utils_computeTangentBinormal pos1 pos2 pos3 texCoord1 texCoord2 texCoord3 normal1
			local tangent = Utils_formatPositionValue results[1] exportFormat
			local binormal = Utils_formatPositionValue results[2] exportFormat
			
			--Exportar la misma tangente para los tres vertices de este triangulo
			format "% % % " tangent.x tangent.y tangent.z to:streamTangents
			format "% % % " tangent.x tangent.y tangent.z to:streamTangents
			format "% % % " tangent.x tangent.y tangent.z to:streamTangents
			
			--Exportar la misma binormal para los tres vertices de este triangulo
			format "% % % " binormal.x binormal.y binormal.z to:streamBinormals
			format "% % % " binormal.x binormal.y binormal.z to:streamBinormals
			format "% % % " binormal.x binormal.y binormal.z to:streamBinormals
		)
		format "\t\t<tangents count='%'>%</tangents>\n" (meshNormals.count * 3) (streamTangents as string) to:outFile
		format "\t\t<binormals count='%'>%</binormals>\n" (meshNormals.count * 3) (streamBinormals as string) to:outFile
		
		
		
		--Skeleton
		boneList = createBoneList skinModifier
		totalBones = boneList.count

		format "\t\t<skeleton bonesCount='%'>\n" totalBones to:outFile
		for i = 1 to totalBones do (
			tgcBoneNode = boneList[i]
			boneNode = tgcBoneNode.boneNode
			parentId = tgcBoneNode.parentId
			
			--Obtener transformacion del hueso
			boneTransform = tgcBoneNode.localTransform
			
			--Translation
			bonePos = Utils_formatPositionValue boneTransform.pos exportFormat
			
			--Rotation			
			boneRotQuat = Utils_formatQuaternion boneTransform.rotation exportFormat
			boneRotX = boneRotQuat.x
			boneRotY = boneRotQuat.y
			boneRotZ = boneRotQuat.z
			boneRotW = boneRotQuat.w
			
			format "\t\t\t<bone id='%' name='%' parentId='%' pos='%' rotQuat='[%,%,%,%]'/>\n" (i-1) boneNode.name (parentId-1) bonePos boneRotX boneRotY boneRotZ boneRotW to:outFile

		)
		format "\t\t</skeleton>\n" to:outFile
		
		
		--Weights
		streamWeights = stringstream ""
		totalWeights = 0
		--Iterar sobre todos los vertices de la malla
		for i = 1 to tmesh.numverts do (
			numweights = skinops.getvertexweightcount skinModifier i
			
			--Iterar sobre las influencias de este vertice
			for w = 1 to numweights do (
				weightVal = skinops.getvertexweight skinModifier i w
				weightBoneId = skinops.getvertexweightboneid skinModifier i w
				totalWeights = totalWeights + 1
				
				--El weightBoneId es segun el skinModifier, buscar nuestro ID interno correspondiente
				for j = 1 to totalBones do (
					tgcBoneNode = boneList[j]
					if(tgcBoneNode.skinModId == weightBoneId) then (
						weightBoneId = j
						exit
					)
				)
				
				
				--Imprimir: VertexId, BoneId, WeightValue
				format "% % % " (i-1) (weightBoneId-1) weightVal to:streamWeights
			)
		)
		format "\t\t<weights count='%'>%</weights>\n" (totalWeights * 3) (streamWeights as string) to:outFile
		

		
		--Finish mesh
		format "\t</mesh>\n" to:outFile

		--End exporting
		format "</tgcSkeletalMesh>\n" to:outFile

		close outFile

		messagebox "Skeletal Mesh exported OK"
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	--Exporta toda la informacion de animacion del esqueleto
	function exportAnimation 
		objectList:selection animName:"Animation" exportFormat:1 = (
			
		if objectList.count == 0 then (
			messagebox "You must select at least one mesh."
			return 0
		)
		
		if objectList.count != 1 then (
			messagebox "You must select only one mesh."
			return 0
		)
		selectedMesh = selection[1]
		
		if Superclassof selectedMesh != Geometryclass then (
			messagebox "The selected object is not a Mesh"
			return 0
		)
		
		--Check Skin Modifier
		max modify mode
		meshModifiers = selectedMesh.modifiers
		skinModifier = undefined
		for i=1 to meshModifiers.count do (
			if (classof meshModifiers[i] as string == "Skin") do (
				skinModifier = meshModifiers[i]
				modPanel.setCurrentObject skinModifier
				exit
			)
		)
		if (skinModifier == undefined) do (
			messagebox "There is no SKIN modifier for the selected object"
			return 0
		)


		--GET OUTPUT FILE
		outName = GetSaveFileName caption:"Save Animation" filename:(animName + ANIM_FILE_EXT)
		if outName == undefined then (
			return 0
		)

		--check file extension
		if ( findString outName ANIM_FILE_EXT ) == undefined then (
			outName = outName + ANIM_FILE_EXT
		)
		outFile = createfile outName
		sceneDir = getFilenamePath outName
		
		

		--Start exporting
		format "<tgcSkeletalAnim>\n" to:outFile

		--Check keyFrames
		totalMaxFrames = animationrange.end - animationrange.start + 1
		--check there is at least one frame
		if totalMaxFrames < 1 then (
			messagebox "There are no animation key frames"
			return 0
		)
		--check 0 as start frame
		if(animationrange.start != 0) then (
			messagebox "The animation must start in frame 0"
			return 0
		)
		
		totalFrames = substring (totalMaxFrames as string) 1 ((totalMaxFrames as string).count - 1)
		startFrame = substring (animationrange.start as string) 1 ((animationrange.start as string).count - 1)
		endFrame = substring (animationrange.end as string) 1 ((animationrange.end as string).count - 1)
		
		--Armar lista de huesos
		boneList = createBoneList skinModifier
		totalBones = boneList.count
		
		format "\t<animation name='%' bonesCount='%' framesCount='%' frameRate='%' startFrame='%' endFrame='%'>\n" animName totalBones totalFrames FrameRate startFrame endFrame to:outFile
		
		--Obtener un BoundingBox que englobe toda la animación
		tgcBB = getAnimationBoundingBox selectedMesh exportFormat
		format "\t\t<boundingBox min='%' max='%'/>\n"  tgcBB.pMin tgcBB.pMax to:outFile
		
		--Iterar sobre cada Bone y analizar sus keyFrames
		for i = 1 to totalBones do (
			tgcBoneNode = boneList[i]
			boneNode = tgcBoneNode.boneNode
			
			--Obtener KeyFrames del hueso
			boneKeys = getBoneKeys tgcBoneNode

			--Si no tiene KeyFrames, al menos generar uno para cada hueso con la posicion del frame cero
			if (boneKeys.count < 1) then (
				format "\t\t<bone id='%' name='%' keyFramesCount='%'>\n" (i-1) boneNode.name 1 to:outFile
				
				--Ir al frame 0
				t = 0
				set time t
				
				--Obtener transformacion del hueso
				if(tgcBoneNode.parentNode == undefined) then (
					boneTransform = boneNode.transform
				) else (
					boneTransform = boneNode.transform * (inverse boneNode.parent.transform)
				)
				
				
				--Translation
				bonePos = Utils_formatPositionValue boneTransform.pos exportFormat
				
				--Rotation			
				boneRotQuat = Utils_formatQuaternion boneTransform.rotation exportFormat
				boneRotX = boneRotQuat.x
				boneRotY = boneRotQuat.y
				boneRotZ = boneRotQuat.z
				boneRotW = boneRotQuat.w
				
				
				format "\t\t\t<frame n='%' pos='%' rotQuat='[%,%,%,%]' />\n" t bonePos boneRotX boneRotY boneRotZ boneRotW to:outFile
				
			) else (
				format "\t\t<bone id='%' name='%' keyFramesCount='%'>\n" (i-1) boneNode.name boneKeys.count to:outFile

				--Iterar sobre los keyframes del Bone
				for j =1 to boneKeys.count do (
					t = boneKeys[j]
					
					--Si el primer KeyFrame no es el 0, tomar el primero que haya y guardarlo en el frame cero
					if (j == 1) and (t != 0) then (
						t = 0
					)
					
					--Ir al frame requerido
					set time t
					
					--Obtener transformacion del hueso
					if(tgcBoneNode.parentNode == undefined) then (
						boneTransform = boneNode.transform
					) else (
						boneTransform = boneNode.transform * (inverse boneNode.parent.transform)
					)
					
					--Translation
					bonePos = Utils_formatPositionValue boneTransform.pos exportFormat
					
					--Rotation			
					boneRotQuat = Utils_formatQuaternion boneTransform.rotation exportFormat
					boneRotX = boneRotQuat.x
					boneRotY = boneRotQuat.y
					boneRotZ = boneRotQuat.z
					boneRotW = boneRotQuat.w
					
					format "\t\t\t<frame n='%' pos='%' rotQuat='[%,%,%,%]' />\n" t bonePos boneRotX boneRotY boneRotZ boneRotW to:outFile
				)
			)
			
			
			format "\t\t</bone>\n" to:outFile
			
		)
		
		--Finish animation
		format "\t</animation>\n" to:outFile

		--End exporting
		format "</tgcSkeletalAnim>\n" to:outFile

		close outFile

		messagebox "Skeletal Animation exported OK"
	)

---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
--############################################################
--##################### USER INTERFACE  ###########################
--############################################################

		
	--######### MESH EXPORTER ###########
	--Widgets
	checkbox w_textureEnable "Copy textures to folder" checked:true
	edittext w_textureDir "Textures dir:" text:"Textures"
	edittext w_texturePreffix "Textures preffix:" text:""
	radiobuttons w_exportFormat labels:#("DirectX", "OpenGL", "Raw")
	button w_exportMesh "Export Mesh" width:150 height:20
	
	
	on w_textureEnable changed theState  do (
		w_textureDir.enabled = theState
		w_texturePreffix.enabled = theState
	)
	
	--Event: button Export Mesh
	on w_exportMesh pressed do
	(
		
		--Crear copia de todos los objetos seleccionados
		selectionCopy = #()
		for s in selection do (
			append selectionCopy s
		)
		
		--exportar Mesh
		exportMesh \
			objectList:selectionCopy exportFormat:w_exportFormat.state \
			copyTextures:w_textureEnable.checked textureDirName:w_textureDir.text \
			texturePreffix:w_texturePreffix.text \
		
	)
	
	--######### ANIMATION EXPORTER ###########
	rollout animationExporter "Animation Exporter" (
		
		
		--Widgets
		edittext w_animName "Animation name:" text:"Animation"
		button w_exportAnim "Export Animation" width:170 height:20
		
		
		--Event: button Export Animation
		on w_exportAnim pressed do
		(
			
			--Crear copia de todos los objetos seleccionados
			selectionCopy = #()
			for s in selection do (
				append selectionCopy s
			)
			
			--exportar Animacion
			exportAnimation \
				objectList:selectionCopy animName:w_animName.text exportFormat:w_exportFormat.state\
			
		)
		
	)

	
	--######### MAIN MENU ###########
	on TgcSkeletalAnimationExporter open do (
		addRollout animationExporter rolledUp:false
	)
	
	on TgcSkeletalAnimationExporter close do (
		removeRollout animationExporter
	)
	
)
	
	
		













