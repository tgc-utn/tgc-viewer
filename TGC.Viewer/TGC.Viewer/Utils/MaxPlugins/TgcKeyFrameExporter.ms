/*
#####################################################
			TgcKeyFrameExporter			
---------------------------------------------------
Exportador de mesh animado por KeyFrame Animation 
#####################################################


General e Instalación
---------------------
Autor: Matías Leone
Plataforma: 3ds Max 2009
Instalación: Copiar archivo TgcKeyFrameExporter.ms en "..\Autodesk\3ds Max 2009\Scripts\Startup\"
También necesita el archivo MaxScriptUtils.ms. Copiarlo a la carpeta anterior.
Ejecución: 
	Abrir 3ds Max
	Ir a la sección de Scripts de la barra de herramientas de la derecha (Utilities: ícono del Martillo)
	Elegir "MAXScript"
	De la lista inferior de "Utilities" elegir "TGC-KeyFrame Exporter"
	Aparecerá abajo todo el conjunto de herramientas del Plugin.


Uso del Plugin
--------------
El plugin genera dos tipos de archivos:
	
1) TgcKeyFrameMesh.xml
Este archivo tiene toda la información de la malla: triángulos, coordendas de textura, materials, etc.
Se exporta la siguiente información de la malla: triángulos y vértices, coordenadas de textura, colores por vértice, Material de la malla.
No se exportan normales.
Solo se puede exportar una malla. Si el modelo se compone por mas de un objeto, utilizar Attach para juntar todos en un único mesh.
Solo soporta Materials del tipo StandardMaterial o MultiMaterial.
Para que un Mesh tenga mas de una textura, utilizar un solo Material del tipo MultiMaterial.
De ambas solo se exporta el DiffuseMap y el Bitmap.
Las texturas pueden ser copiadas de su posición original del FileSystem a una carpeta relativa a la ubicación
del archivo XML generado.
Actualmente solo soporta exportar la información en formato DirectX
La malla tiene que estar seleccionada antes de exportar.
Es recomendable que la malla se ubique en el origen de coordenadas.


2) TgcKeyFrameAnim.xml
Archivo que contiene una animación particular del modelo.
Normalmente existirá un archivo TgcKeyFrameMesh.xml y N archivos TgcKeyFrameAnim.xml, uno por cada animación.
El nombre de la animación que pide el plugin debe coincidir con el nombre del archivo guardado (Sin la extensión "-TgcKeyFrameAnim.xml")
Se exportan las coordenadas de todos los vértices de la malla para cada KeyFrame importante.
La información de la animación se puede obtener de dos lugares: Del propio Mesh o de un esqueleto de huesos creado con la herramienta Biped.
En caso de utilizar un Biped se debe habilitar esta opción y elegir el hueso root del esqueleto (Normalmente el Bip 01)
Actualmente solo soporta exportar la información en formato DirectX.
La malla tiene que estar seleccionada antes de exportar.
La línea de tiempo (TrackView) tiene que acotarse y que solo quede el rango de tiempo utilizado.

*/


--Incluir utilidades generales
include "MaxScriptUtils.ms"


utility TgcKeyFrameExpoter "TGC-KeyFrame Exporter"
(

	--######### GLOBALS ###########
	local MESH_FILE_EXT = "-TgcKeyFrameMesh.xml"
	local ANIM_FILE_EXT = "-TgcKeyFrameAnim.xml"

---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* Crea un BoundingBox que contemple el tamaño de la malla en todos los cuadros de animacion
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
	* Estructura con datos de un triángulo de una malla
	*/
	struct TgcMeshFace
	(
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
		format "<tgcKeyFrameMesh>\n" to:outFile

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
			
			tgcMeshFaceObj = TgcMeshFace face:face tvFace:texFace vcFace:colorFace faceMatId:faceMatId
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
		
		--Finish mesh
		format "\t</mesh>\n" to:outFile

		--End exporting
		format "</tgcKeyFrameMesh>\n" to:outFile

		close outFile

		messagebox "KeyFrame Mesh exported OK"
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	--Obtiene los KeyFrames del Mesh o Biped a exportar
	function getKeyFrames bipObj = (
		keyFramesArray = #()
		
		--Obtener KeyFrames  de la malla, cuando no hay esqueleto
		if bipObj == undefined then (
			t = 0f
			lastT = -1f
			while (t != undefined) and (t > lastT) do (
				append keyFramesArray t
				
				--Obtener proximo keyFrame
				lastT = t
				t = at time lastT trackbar.getNextKeyTime()
			)
			
		--Obtener KeyFrames del esqueleto
		) else (
			nn = biped.maxNumNodes bipObj
			nl = biped.maxNumLinks bipObj
			
			for i = 1 to nn do ( 

				anode = biped.getNode bipObj i
				if anode != undefined then ( 
					
					if anode.controller != undefined then (
						nodeKeys = anode.controller.keys
						for nk = 1 to nodeKeys.count do (
							appendIfUnique keyFramesArray nodeKeys[nk].time
						) 
					)
					
					for j = 1 to nl do ( 
						alink = biped.getNode bipObj i link:j
						if alink != undefined then (
							if alink.controller != undefined then (
								linkKeys = alink.controller.keys
								for lk = 1 to linkKeys.count do (
									appendIfUnique keyFramesArray linkKeys[lk].time
								) 
							)
						)
					)
				)
			)
			
			sort keyFramesArray
		)
		
		return keyFramesArray
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	--Exporta toda la informacion de una animacion compatible con un Mesh ya exportado
	function exportAnimation 
		objectList:selection animName:"KeyFrameAnim" exportFormat:1 bipedObj:undefined = (
			
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


		--GET OUTPUT FILE
		outName = GetSaveFileName()
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
		format "<tgcKeyFrameAnim>\n" to:outFile

		--Check keyFrames
		totalMaxFrames = animationrange.end - animationrange.start + 1
		minKeyFrame = at time -1 trackbar.getNextKeyTime()
		--check there is at least one frame
		if (totalMaxFrames < 1) or (minKeyFrame == undefined) then (
			messagebox "There are no animation key frames"
			return 0
		)
		totalFrames = substring (totalMaxFrames as string) 1 ((totalMaxFrames as string).count - 1)
		startFrame = substring (animationrange.start as string) 1 ((animationrange.start as string).count - 1)
		endFrame = substring (animationrange.end as string) 1 ((animationrange.end as string).count - 1)
		
		--Obtener KeyFrames
		keyFramesArray = getKeyFrames bipedObj
		if keyFramesArray.count < 2 then (
			messagebox "There must be at least two key frames"
			return 0
		)
		
		--Start mesh
		tmesh = snapshotAsMesh selectedMesh;
		format ("\t<animation name='%' verticesCount='%' framesCount='%' keyFramesCount='%' frameRate='%' startFrame='%' endFrame='%'>\n") animName (tmesh.numverts*3) totalFrames keyFramesArray.count FrameRate startFrame endFrame to:outFile
		
		--Obtener un BoundingBox que englobe toda la animación
		tgcBB = getAnimationBoundingBox selectedMesh exportFormat
		format "\t\t<boundingBox min='%' max='%'/>\n"  tgcBB.pMin tgcBB.pMax to:outFile
		
		--Exportar cada KeyFrame
		for keyFrame in keyFramesArray do (
			t = Utils_formatKeyFrame keyFrame

			--Get the the mesh at the current time
			at time t tmesh = snapshotAsMesh selectedMesh

			--Vertices
			streamVertices = stringstream ""
			for j = 1 to tmesh.numverts do (
				
				vert = getVert tmesh j
				newVert = Utils_formatPositionValue vert exportFormat
				format "% % % " newVert.x newVert.y newVert.z to:streamVertices
			)
			
			format "\t\t<frame time='%'>%</frame>\n" t (streamVertices as string) to:outFile
		)
		

		--Finish animation
		format "\t</animation>\n" to:outFile

		--End exporting
		format "</tgcKeyFrameAnim>\n" to:outFile

		close outFile

		messagebox "KeyFrame Animation exported OK"
	)

	
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
		
		--Validar que sea un Biped
		function getBiped o = (
			classof o == Biped_Object
			--Superclassof o == Geometryclass and classof o != TargetObject
		)
		
		--Widgets
		edittext w_animName "Animation name:" text:"Animation"
		checkbox w_useBiped "Use Biped" checked:false
		pickbutton w_selectBiped "Select Biped" width:100 filter:getBiped enabled:false autoDisplay:true
		button w_exportAnim "Export Animation" width:170 height:20
		
		on w_useBiped changed theState  do (
			w_selectBiped.enabled = theState
		)
		
		--Event: button Export Animation
		on w_exportAnim pressed do
		(
			
			--Crear copia de todos los objetos seleccionados
			selectionCopy = #()
			for s in selection do (
				append selectionCopy s
			)
			
			bipedObj = undefined
			if w_useBiped.checked then (
				if w_selectBiped.object == undefined then (
					messagebox "No Biped has been selected."
					return 0
				)
				bipedObj = w_selectBiped.object
			)
			
			
			
			--exportar Mesh
			exportAnimation \
				objectList:selectionCopy animName:w_animName.text exportFormat:w_exportFormat.state bipedObj: bipedObj\
			
		)
		
	)

	
	--######### MAIN MENU ###########
	on TgcKeyFrameExpoter open do (
		addRollout animationExporter rolledUp:false
	)
	
	on TgcKeyFrameExpoter close do (
		removeRollout animationExporter
	)
	
)
	
	
		













