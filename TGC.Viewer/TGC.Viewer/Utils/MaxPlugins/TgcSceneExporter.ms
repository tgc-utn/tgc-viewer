/*
##############################################################
			TgcSceneExporter			
--------------------------------------------------------------
Exportador de escenario estático con Iluminación por Lightmaps
##############################################################


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
	De la lista inferior de "Utilities" elegir "TGC-Scene Exporter"
	Aparecerá abajo todo el conjunto de herramientas del Plugin.


Uso del Plugin
--------------
El plugin genera un archivo XML "-TgcScene.xml".
Permite exportar varios modelos estáticos con colores por vértice, textura y lightmaps.
Se exporta la siguiente información de la malla: triángulos y vértices, coordenadas de textura, colores por vértice, Material de la malla y normales.
Solo se exportan los modelos que están seleccionados.
Se debe especificar el nombre de la escena (es recomendable que coincida con el nombre del archivo a guardar)
Cada modelo puede tener solo un Material.
Solo se exportan los Materials del tipo Standard Material y Multi-SubObject Material.
Si un modelo necesita tener mas de una textura, debe utilizar Multi-SubObject Material.
De cada Material se exporta el Bitmap del DiffuseMap.
El resto de los atributos del Material no son tomados en cuenta.
Si se habilita la opción correspondiente, el plugin toma todas las texturas utilizadas por los modelos y las copia a una carpeta
destino en el mismo lugar en el que se guarda el archivo. De esta forma todas las texturas quedan con path relativos a esa carpeta.
Lo mismo ocurre con los Lightmaps.
La iluminación de los Lightmaps se genera con Radiosity y puede tardar bastante en caso de haber gran cantidad de luces y modelos.
Si un objeto está compuesto por dos modelos separados, y dentro de la aplicación se quiere tratar como un único Mesh entonces hay que
utilizar la herramienta Attach de 3D Max para unir ambos modelos en un solo Mesh.
El origen de coordendas para cada modelo será el lugar en el cual se encuentra ubicado dentro del escenario de 3D Max.

*/


--Incluir utilidades generales
include "MaxScriptUtils.ms"


utility TgcSceneExpoter "TGC-Scene Exporter"
(

	--######### GLOBALS ###########
	local FILE_EXT = "-TgcScene.xml"
	local LIGHTMAP_MODIFIER_NAME = "LightMap UVW"
	local LIGHTMAP_MATERIAL_NAME = "LightMap Material"
	local LIGHTMAP_CHANNEL_NUM = 2

	
---------------------------------------------------------------------------------------------------------------------------------------------------------------		
	
	/*
	* Estructura con informacion de cada malla a exportar
	*/
	struct TgcMesh
	(
		meshNode,
		meshId,
		parentInstance,
		tgcMaterialItem,
		meshInstances,
		tgcBB,
		meshTransform,
		meshPosition,
		meshRotQuat,
		meshScale,
		tgcUserPropArray,
		alphaBlendEnable
	) 

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
		faceMatId,
		texFaceLM
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
	
	/*
	* Crea un array de TgcMesh con todos los modelos a exportar.
	* Se toman todos los objetos y se clonan. Los originales se ocultan. Se trabaja sobre los clonados para no modificar los originales.
	* Todos los clonados se colapsan a EditableMesh.
	* Cargar previamente la lista de TgcMaterial con los Materials a exportar
	* La lista se devuelve ordenada poniendo primero al modelo original y a continuacion
	* todas sus instancias
	*/
	function createMeshList objectList tgcMaterials exportFormat portalExportEnable portalPortalsLayerName portalCellsLayerName = (
	
		--Elegir los mesh a clonar
		local meshesToClone = #()
		for i = 1 to objectList.count do (
			local selectedMesh = objectList[i]

			--Ver si realmente es un Geometry
			if (Utils_isGeometry selectedMesh) == false then (
				continue
			) 
			
			--Si está habilitado PortalRendering, evitar los modelos de los layers de PortalRendering
			if (portalExportEnable == true) and ((selectedMesh.layer.name == portalPortalsLayerName) or (selectedMesh.layer.name == portalCellsLayerName)) then (
				continue
			)
			
			append meshesToClone selectedMesh
		)
		
		--Clonar todo
		local meshesClones
		maxops.cloneNodes meshesToClone cloneType:#copy newNodes:&meshesClones
		
		--Esconder todos los meshes originales y renombrar clonados
		for i = 1 to meshesToClone.count do (
			local selectedMesh = meshesToClone[i]
			
			--Esconder
			hide selectedMesh
			
			--Poner en el mesh clonado el nombre del mesh original (porque MAX los auto-renombra)
			meshesClones[i].name = selectedMesh.name
		)
		
		--Desagrupar todo lo que este agrupado y luego descartar todos los nodos de grupos desagrupados que quedaron invalidos
		ungroupMeshes = #()
		for i = 1 to meshesClones.count do (
			local selectedMesh = meshesClones[i]
			if (isGroupHead selectedMesh) then (
				ungroup selectedMesh
			)
			else (
				append ungroupMeshes selectedMesh
			)
		)
		meshesClones = ungroupMeshes
		
		
		--Iterar sobre clonados y armar lista de mesh
		local meshList = #()
		for i = 1 to meshesClones.count do (
			local selectedMesh = meshesClones[i]
			
			--Colapsar todo y convertir a Mesh
			convertToMesh selectedMesh
			
			--Ver si tiene material
			local tgcMaterialItem = undefined
			local alphaBlendEnable = false
			if (Utils_meshHasMaterial selectedMesh) == true then (
			
				--Obtener TgcMaterial correspondiente
				tgcMaterialItem = Utils_getMeshMaterial selectedMesh tgcMaterials
				alphaBlendEnable = tgcMaterialItem.alphaBlendEnable
			)
			
			--BoundingBox
			local tgcBB = Utils_getMeshBoundingBox selectedMesh exportFormat
			
			--Position, rotation y scale
			local meshTransform = selectedMesh.transform 
			local meshPosition = Utils_formatPositionValue meshTransform.pos exportFormat
			local meshRotQuat = Utils_formatQuaternion meshTransform.rotation exportFormat
			local meshScale = Utils_formatScaleValue meshTransform.scale exportFormat
			
			--User properties
			local tgcUserPropArray = Utils_getUserPropertiesArray selectedMesh
			
			--Crear mesh
			local tgcMeshItem = TgcMesh meshNode:selectedMesh tgcMaterialItem:tgcMaterialItem meshInstances:#() parentInstance:undefined tgcBB:tgcBB \
				meshTransform:meshTransform meshPosition:meshPosition meshRotQuat:meshRotQuat meshScale:meshScale tgcUserPropArray:tgcUserPropArray \
				alphaBlendEnable:alphaBlendEnable
		
			append meshList tgcMeshItem
			tgcMeshItem.meshId = meshList.count
		)
		
		--Configurar mallas instancias en padres e hijos
		for tgcMeshItem in meshList do (
			if tgcMeshItem.parentInstance != undefined or tgcMeshItem.meshInstances.count > 0 then (
				continue
			)
			
			--Ver si tiene instancias
			local instances
			InstanceMgr.GetInstances tgcMeshItem.meshNode &instances
			if instances.count > 0 then (
				--Recorrer instancias
				for meshInstance in instances do (

					--Buscar TgcMesh de cada instancia y guardar en el padre
					for i = 1 to meshList.count do (
						
						--Ver que no sea el original
						if meshList[i] != tgcMeshItem then (
							if meshList[i].meshNode == meshInstance then (
								--Agregar instancia a padre
								append tgcMeshItem.meshInstances meshList[i]
								--Cargar padre en instancia
								meshList[i].parentInstance = tgcMeshItem
								break
							)
						)
					)
				)
			)
		)
				
		--Ordenar lista de modelos de manera tal que quede primero el modelo original y detrás sus instancias
		local sortedMeshList = #()
		for tgcMeshItem in meshList do (
		
			--Ver si ya lo pusimos como instancia
			if tgcMeshItem.parentInstance != undefined then (
				continue
			)
			
			--Agregar el padre
			append sortedMeshList tgcMeshItem
			
			--Agregar instancias
			for tgcMeshInstance in tgcMeshItem.meshInstances do (
				append sortedMeshList tgcMeshInstance
			)
		)
		
		
		--Devolver sortedMeshList
		sortedMeshList
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* Generar un Lightmap general para todo el escenario, y mapeos de UV para cada modelo individual
	*/
	function generateSceneLightMap tgcMeshList fullFileName:"Lightmap\\lightmap.jpg" 
		imageFormat:"jpg" xSize:1024 ySize:1024 lightmapLayers:"0" lightmapDebugMeshEnabled:false = (

		--Crear lista con todos los nodos de mesh que estan en un layer contemplado para generar lightmap
		local meshesForLightmap = #()
		local hiddenMeshes = #()
		for i = 1 to tgcMeshList.count do (
			local originalMesh =  tgcMeshList[i].meshNode
			--Ver si pertence a un layer contemplado para lightmaps
			if (findItem lightmapLayers originalMesh.layer.name) > 0 then (
				--Es un layer valido, agregar
				append meshesForLightmap originalMesh
				
			--No se usa para Lightmaps
			) else (
				--Lo ocultamos asi no interfiere con el calculo de iluminacion
				hide originalMesh
				append hiddenMeshes originalMesh
			)
		)
		
		--Ver si quedo algun objeto para iluminar, sino salir
		if meshesForLightmap.count == 0 then (
			for i = 1 to hiddenMeshes.count do (
				unhide hiddenMeshes[i]
			)
			return undefined
		)
		
		--Aplicamos algunas modificaciones a cada modelo para trabajar mas facil con el
		local facesCount = 0
		local meshesForLightmap_faceIndex = #()
		for i = 1 to meshesForLightmap.count do (
			local meshForLightmap = meshesForLightmap[i]

			--Acomodar normales del objeto, por si tiene Mirror
			Utils_resetXFormAndFlip meshForLightmap
			
			--Agregar nuevo modifier en channel 2 para el Unwrap UVW para el Lightmap
			local originalMeshLightmapUnwrap = unwrap_UVW()
			originalMeshLightmapUnwrap.name = LIGHTMAP_MODIFIER_NAME
			addModifier meshForLightmap originalMeshLightmapUnwrap
			originalMeshLightmapUnwrap.setMapChannel LIGHTMAP_CHANNEL_NUM

			--Crear un array asignandole un id correlativos a cada face del mesh respecto de todas las caras de todos los meshes del escenario, para luego poder pedir solo esas caras
			local meshNumFaces = meshForLightmap.numFaces
			local currentFaces = for f = 1 to meshNumFaces collect (facesCount + f)
			append meshesForLightmap_faceIndex currentFaces
			facesCount += meshNumFaces
		)
		
		--Clonar los modelos porque los vamos a attachear, para que no se arruinen los originales y poder luego obtener su Unwrap correspondiente
		local meshesClones
		maxops.cloneNodes meshesForLightmap cloneType:#copy newNodes:&meshesClones
		
		--Attachamos todos los modelos del escenario al primer modelo, asi creamos un gran mesh para todo el escenario
		local lightmapSceneMesh = meshesClones[1]
		if meshesClones.count > 1 then (
			for i = 2 to meshesClones.count do ( 
				meshop.attach lightmapSceneMesh meshesClones[i]
			)
		)
		lightmapSceneMesh.name="lightmapSceneMesh"
		convertToMesh lightmapSceneMesh
		
		--Al mesh de todo el escenario le agregamos un nuevo modifier UVW Unwrap en channel 2
		local lightmapUnwrap = unwrap_UVW()
		lightmapUnwrap.name = LIGHTMAP_MODIFIER_NAME
		addModifier lightmapSceneMesh lightmapUnwrap
		lightmapUnwrap = lightmapSceneMesh.modifiers[1]
		
		--Esta linea debe ser ejecutada antes de flattenMapNoParams para que funcione. Parece ser un Bug de 3Ds Max
		select lightmapSceneMesh
		classof lightmapSceneMesh
		
		--Seleccionar todas las caras del unwrap
		local lightmapUnwrapNumberPolygons = lightmapUnwrap.unwrap.numberPolygons()
		lightmapUnwrap.unwrap2.selectFaces #{1..lightmapUnwrapNumberPolygons}
		
		--Aplicar Flatten Mapping al UVW Unrwap
		lightmapUnwrap.setMapChannel LIGHTMAP_CHANNEL_NUM
		lightmapUnwrap.setFlattenSpacing 0.001
		lightmapUnwrap.flattenMapNoParams()
		lightmapUnwrap.packNoParams()
		
		--Crear LightMap BakeElement
		local alightMap=LightingMap()
		alightMap.directOn =true
		alightMap.indirectOn = true
		alightMap.shadowsOn = true
		alightMap.filterOn = true

		--Paths
		local fPath=getFilenamePath fullFileName
		local fName=getFilenameFile fullFileName
		local fType=getFilenameType fullFileName

		--Size
		local bi = lightmapSceneMesh.INodeBakeProperties
		alightMap.outputSzX=xSize
		alightMap.outputSzY=ySize

		alightMap.filename=fullFileName
		alightMap.fileType=fullFileName
		--alightMap.fileType=imageFormat

		--Agregar Bake Element
		bi.removeAllBakeElements()
		bi.addBakeElement alightMap
		bi.bakeChannel = LIGHTMAP_CHANNEL_NUM
		bi.nDilations = 1
		bi.bakeEnabled = true

		--Hacer render to texture del Lightmap con Radiosity
		vfbPopup = render rendertype:#bakeSelected outputwidth:xSize outputheight:ySize vfb:true progressbar:true /*computeRadiosity:true*/ useRadiosity:true
		
		--Quitar Bake Element
		bi.removeAllBakeElements()
		bi.bakeEnabled = false

		--TODO: Eliminar Material si ya existia de antes!!!!!

		--Crear material con la textura de Lightmap generada, en channel 2
		local lightmapSceneMaterial = standardMaterial name:LIGHTMAP_MATERIAL_NAME showInViewport:true
		lightmapSceneMaterial.diffuseMap = bitmapTexture filename:fullFileName
		lightmapSceneMaterial.diffuseMap.coords.mapChannel = LIGHTMAP_CHANNEL_NUM
		lightmapSceneMesh.material = lightmapSceneMaterial

		--Obtener del mesh de todo el escenario el UV correspondiente a cada mesh individual y aplicarselo
		max modify mode
		disableSceneRedraw()
		for i = 1 to meshesForLightmap.count do (
			local meshForLightmap = meshesForLightmap[i]
			
			--Asignarle material con Lightmap (Habria que hacer un Composite)
			--meshForLightmap.material = lightmapSceneMaterial
			
			--Seleccionar del mesh de todo el escenario solo las caras de UV que pertenecen al mesh individual, pidiendo por los IDs de face correlativos correspondientes
			local meshFaceIndex = (meshesForLightmap_faceIndex[i] as BitArray)
			lightmapUnwrap.unwrap2.setTVSubObjectMode 3
			modPanel.setCurrentObject lightmapUnwrap
			lightmapUnwrap.unwrap2.selectFaces meshFaceIndex
			--Copiar caras
			lightmapUnwrap.unwrap2.copy()
			
			--Seleccionar todas las caras de UV del mesh individual y pegar las del mesh de todo el escenario
			local originalMeshLightmapUnwrap = meshForLightmap.modifiers[1]
			originalMeshLightmapUnwrap.unwrap2.setTVElementMode false
			modPanel.setCurrentObject originalMeshLightmapUnwrap
			local numberPolygons = originalMeshLightmapUnwrap.unwrap.numberPolygons()
			local originalMesh_faceIndex = #{1..numberPolygons}
			originalMeshLightmapUnwrap.unwrap2.selectFaces originalMesh_faceIndex
			--Pegar caras
			originalMeshLightmapUnwrap.unwrap2.paste false		
			
			--Colapsar modifiers en un EditableMesh
			convertToMesh meshForLightmap
		)
		enableSceneRedraw()
		max views redraw
	
		--Volver a mostrar todos los objetos que se ocultaron para que no salgan en el render
		for i = 1 to hiddenMeshes.count do (
			unhide hiddenMeshes[i]
		)
		
		--Cerrar ventana de render to texture del Lightmap
		close vfbPopup
		
		--Ver si dejamos el mesh de todo el escenario para poder verlo o lo eliminamos
		if lightmapDebugMeshEnabled then (
			select lightmapSceneMesh
		) else (
			--Eliminar mesh de todo el escenario
			delete lightmapSceneMesh
		)
		
		
		--Devolver bitmap de Lightmap generado
		alightMap.bitmap	
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	

	/*
	* Estructura con informacion de una Celda de Portal Rendering
	*/
	struct TgcCell
	(
		id,
		meshNode,
		facePlanes,
		boundingVertices,
		tgcMeshList,
		tgcBB
	) 
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* Crear lista de TgcCell con celdas de la escena
	*/
	function createCellsList cellObjectsList tgcMeshList exportFormat = (
		--Crear celdas
		tgcCells = #()
		cellId = 0
		for meshNode in cellObjectsList do (
			--Tratar mesh de celda como un cuerpo Convexo
			tPoly = convertTo meshNode (Editable_Poly)
			numVert =  polyop.getNumVerts tPoly
			
			--Obtener vertices extremos cuerpo convexo
			boundingVertices = #()
			for v = 1 to numVert do (
				vertVal = polyop.getVert tPoly v
				append boundingVertices (Utils_formatPositionValue vertVal exportFormat)
			)
			
			--Obtener planos de las caras del cuerpo convexo, apuntando hacia afuera
			facePlanes = Utils_getPlanesFromMesh meshNode exportFormat	
			
			--Crear BoundingBox del cuerpo convexo
			tgcBB = Utils_getMeshBoundingBox meshNode exportFormat
			
			--Crear TgcCell
			tgcCellObj = TgcCell id:cellId meshNode:meshNode facePlanes:facePlanes boundingVertices:boundingVertices tgcMeshList:#() tgcBB:tgcBB
			append tgcCells tgcCellObj
			cellId = cellId + 1
		)
		
		--Obtener meshes que están dentro de la celda
		for tgcMeshItem in tgcMeshList do (
			meshBB = tgcMeshItem.tgcBB
			cellsCount = 0
			for cell in tgcCells do (
				
				--Ver si el BoundingBox del mesh colisiona con el ConvexHull de la celda. Puede estar en mas de una celda.
				if (CollisionUtils_classifyConvexHullAABB cell.facePlanes meshBB) != 0 then (
					append cell.tgcMeshList tgcMeshItem
					cellsCount = cellsCount  + 1
				)
			)
			if cellsCount == 0 then (
				messagebox ("The mesh: " + tgcMeshItem.meshNode.name + " does not belong to any cell.")
				return undefined
			)
		)
		
		return tgcCells
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	

		/*
	* Estructura con informacion de un Portal de Portal Rendering
	*/
	struct TgcPortal
	(
		meshNode,
		tgcBB,
		tgcCellA,
		tgcCellB,
		boundingVerticesA,
		boundingVerticesB,
		tgcPlaneA,
		tgcPlaneB
	) 
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	/*
	* Crear lista de TgcPortal con los portales de la escena
	*/
	function createPortalsList portalObjectsList tgcCells exportFormat = (
		--Crear portales
		tgcPortals = #()
		for meshNode in portalObjectsList do (
			
			--Obtener vertices extremos del poligono
			boundingVertices = Utils_getPolygonVertices meshNode exportFormat
			--Tiene que ser un poligono de una sola cara
			if boundingVertices == undefined then (
				messagebox ("The Portal: " + portal.meshNode.name + " has more than one face. A Portal can only be a plane.")
				return undefined
			)
			
			--Crear BoundingBox del polígono
			tgcBB = Utils_getMeshBoundingBox meshNode exportFormat

			--Crear TgcPortal
			tgcPortalObj = TgcPortal meshNode:meshNode tgcBB:tgcBB tgcCellA:undefined tgcCellB:undefined boundingVerticesA:boundingVertices boundingVerticesB:undefined tgcPlaneA:undefined tgcPlaneB:undefined 
			append tgcPortals tgcPortalObj
		)
		
		--Relacionar celdas y portales
		bbExpansion = point3 5 5 5
		for portal in tgcPortals do (
			
			--Agrandar los extremos del BoundingBox para poder detectar inclusión con las celdas (el portal es solo un plano)
			tgcBBExp = TgcBoundingBox pMin:(portal.tgcBB.pMin - bbExpansion) pMax:(portal.tgcBB.pMax + bbExpansion)
			
			--Detecar colisión del BoundingBox del portal contra el ConvexHull de cada celda del escenario
			conectedCells = #()
			for cell in tgcCells do (
				if (CollisionUtils_classifyConvexHullAABB cell.facePlanes tgcBBExp) != 0 then (
					append conectedCells cell
				)
			)
			
			--Validar que solo haya dos celdas que conectan con este portal
			if conectedCells.count != 2 then (
				connectedCellsNames = "Connected Cells: "
				for c in conectedCells do (
					connectedCellsNames = connectedCellsNames + c.meshNode.name + ", "
				)
				messagebox ("The Portal: " + portal.meshNode.name + " does not have two cells connected with it. " + connectedCellsNames)
				return undefined
			)
			
			--Hay que crear un portal que apunte hacia cada celda, con los vertices en clockwise-order
			cell1 = conectedCells[1]
			cell2 = conectedCells[2]
			
			--Ver hacia que lado apunta la normal del portal, en el orden en que vienen los vértices originalmente
			centerCellA = Utils_computeBoundingBoxCenter cell1.tgcBB
			tgcPlaneOrig = Utils_createPlaneFromPoints portal.boundingVerticesA[1] portal.boundingVerticesA[2] portal.boundingVerticesA[3] exportFormat
			
			--La normal del plano del portal apunta hacia la celda A
			if (CollisionUtils_classifyPointPlane centerCellA tgcPlaneOrig) == 1 then (
				portal.tgcCellA = cell1
				portal.tgcCellB = cell2
				portal.tgcPlaneA = tgcPlaneOrig
				
				--Invertir orden de vertices para la celda B
				portal.boundingVerticesB = Utils_reverseArray portal.boundingVerticesA
				
				--Crear plano de portal para la celda B, con los vertices invertidos
				portal.tgcPlaneB = Utils_createPlaneFromPoints portal.boundingVerticesB[1] portal.boundingVerticesB[2] portal.boundingVerticesB[3] exportFormat
					
			--La normal del plano del portal apunta hacia la celda B
			) else (
				--Invertir orden de celdas
				portal.tgcCellA = cell2
				portal.tgcCellB = cell1
				
				--Los vertices y el plano del portal están en el orden correcto para la celda B
				portal.boundingVerticesB = portal.boundingVerticesA
				portal.tgcPlaneB = tgcPlaneOrig
				
				--Invertir orden de vertices para la celda A
				portal.boundingVerticesA = Utils_reverseArray portal.boundingVerticesB
				
				--Crear plano de portal para la celda A, con los vertices invertidos
				portal.tgcPlaneA = Utils_createPlaneFromPoints portal.boundingVerticesA[1] portal.boundingVerticesA[2] portal.boundingVerticesA[3] exportFormat
			)
		)
		
		return tgcPortals
	)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* Exportar información de Portal Rendering
	*/
	function exportPortalInfo objectList tgcMeshList portalsLayerName cellsLayerName exportFormat outFile = (
		portalObjectsList = #()
		cellObjectsList = #()
		
		--Separar todos los objetos seleccionados
		for meshNode in objectList do (
			
			--Objetos del layer de Portales
			if meshNode.layer.name == portalsLayerName then (
				append portalObjectsList meshNode
				
			--Objetos del layer de Celdas
			) else if meshNode.layer.name == cellsLayerName then (
				append cellObjectsList meshNode
			)
		)
		
		--Crear lista de celdas
		tgcCells = createCellsList cellObjectsList tgcMeshList exportFormat	
		if tgcCells == undefined then
			return -1
		
		--Crear lista de portales
		tgcPortals = createPortalsList portalObjectsList tgcCells exportFormat
		if tgcPortals == undefined then
			return -1
		
		--Empezar exportación en XML
		format "\t<portalRendering>\n" to:outFile
		
		
		--Exportar celdas
		format "\t\t<cells count='%'>\n" tgcCells.count to:outFile
		for cell in tgcCells do (
			format "\t\t\t<cell id='%' name='%'>\n" cell.id cell.meshNode.name to:outFile
			
			--facePlanes
			streamFacePlanes = stringstream ""
			for p in cell.facePlanes do (
				format "% % % % " p.normal.x p.normal.y p.normal.z p.d to:streamFacePlanes
			)
			format "\t\t\t\t<facePlanes count='%'>%</facePlanes>\n" (cell.facePlanes.count * 4) (streamFacePlanes as string) to:outFile
			
			--boundingVertices
			streamVertices = stringstream ""
			for v in cell.boundingVertices do (
				format "% % % " v.x v.y v.z to:streamVertices
			)
			format "\t\t\t\t<boundingVertices count='%'>%</boundingVertices>\n" (cell.boundingVertices.count * 3) (streamVertices as string) to:outFile
			
			--meshes
			streamMeshes = stringstream ""
			for i = 1 to cell.tgcMeshList.count do (
				meshName = cell.tgcMeshList[i].meshNode.name
				separator = ","
				if i == cell.tgcMeshList.count then (
					separator = ""
				)
				format "%%" meshName separator to:streamMeshes
			)
			format "\t\t\t\t<meshes count='%'>%</meshes>\n" cell.tgcMeshList.count (streamMeshes as string) to:outFile			
			
			format "\t\t\t</cell>\n" to:outFile
		)
		format "\t\t</cells>\n" to:outFile
		
		
		--Exportar Portales
		format "\t\t<portals count='%'>\n" tgcPortals.count to:outFile
		for portal in tgcPortals do (
			format "\t\t\t<portal name='%'>\n" portal.meshNode.name to:outFile
			
			--boundingBox
			format "\t\t\t\t<boundingBox min='%' max='%'/>\n" portal.tgcBB.pMin portal.tgcBB.pMax to:outFile
			
			--cellA
			format "\t\t\t\t<cellA id='%'>\n" portal.tgcCellA.id to:outFile
			
			--planeA
			format "\t\t\t\t\t<plane>[%,%,%,%]</plane>\n" portal.tgcPlaneA.normal.x portal.tgcPlaneA.normal.y portal.tgcPlaneA.normal.z portal.tgcPlaneA.d to:outFile
			
			--boundingVerticesA
			streamVerticesA = stringstream ""
			for v in portal.boundingVerticesA do (
				format "% % % " v.x v.y v.z to:streamVerticesA
			)
			format "\t\t\t\t\t<vertices count='%'>%</vertices>\n" (portal.boundingVerticesA.count * 3) (streamVerticesA as string) to:outFile
			
			format "\t\t\t\t</cellA>\n" to:outFile
			
			
			--cellB
			format "\t\t\t\t<cellB id='%'>\n" portal.tgcCellB.id to:outFile
			
			--planeB
			format "\t\t\t\t\t<plane>[%,%,%,%]</plane>\n" portal.tgcPlaneB.normal.x portal.tgcPlaneB.normal.y portal.tgcPlaneB.normal.z portal.tgcPlaneB.d to:outFile
			
			--boundingVerticesB
			streamVerticesB = stringstream ""
			for v in portal.boundingVerticesB do (
				format "% % % " v.x v.y v.z to:streamVerticesB
			)
			format "\t\t\t\t\t<vertices count='%'>%</vertices>\n" (portal.boundingVerticesB.count * 3) (streamVerticesB as string) to:outFile
			
			format "\t\t\t\t</cellB>\n" to:outFile
			
			
			format "\t\t\t</portal>\n" to:outFile
		)
		format "\t\t</portals>\n" to:outFile
		
		format "\t</portalRendering>\n" to:outFile
		
		return 0
	)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* FUNCION PRINCIPAL
	*
	* Exporta toda la informacion de la escena: meshes, textures, lightmaps.
	* También exporta información de PortalRendering si está activado.
	*/
	function exportScene 
		objectList:selection sceneName:"Scene" exportFormat:1 
		lightmapEnabled:true lightmapDirName:"Lightmaps" lightmapWidth:128 lightmapHeight:128  
		lightmapFormat:".jpg" lightmapName:"lightmap" lightmapLayers: #() lightmapDebugMeshEnabled:false
		copyTextures:true textureDirName:"Textures" texturePreffix:"" 
		portalExportEnable:false portalPortalsLayerName:"" portalCellsLayerName:"" = (			
			
		if objectList.count == 0 then (
			messagebox "You must select at least one mesh."
			return 0
		)

		--GET OUTPUT FILE
		local outName = GetSaveFileName()
		if outName == undefined then (
			return 0
		)

		--check file extension
		if ( findString outName FILE_EXT ) == undefined then (
			outName = outName + FILE_EXT
		)
		local outFile = createfile outName
		local sceneDir = getFilenamePath outName
			

		--Start exporting
		format "<tgcScene>\n" to:outFile
		
		format "\t<name>%</name>\n" sceneName to:outFile
		
		--Utilizar datos segun frame inicial
		set time 0
		
		--Crear directorios de texturas y lightmaps
		if copyTextures then (
			local texturesDir = sceneDir + "\\" + textureDirName + "\\"
			deleteFile texturesDir
			makeDir texturesDir
		)
		if lightmapEnabled then (
			local lightmapDir = sceneDir + "\\" + lightmapDirName + "\\"
			deleteFile lightmapDir
			makeDir lightmapDir
		)
		
		--Exportar info de directorios de texturas y lightmaps
		format "\t<texturesExport enabled='%' dir='%'/>\n" (copyTextures as string) textureDirName to:outFile
		format "\t<lightmapExport enabled='%' dir='%'/>\n" (lightmapEnabled as string) lightmapDirName to:outFile
		
		
		
		--Obtener lista de materials
		local tgcMaterials = Utils_createMaterialList objectList
		--Obtener lista de meshes
		local tgcMeshList = createMeshList objectList tgcMaterials exportFormat portalExportEnable portalPortalsLayerName portalCellsLayerName

		/*
		--Volver a chequear que los Materials sean utilizados por alguno de los modelos finales que quedaron en la lista tgcMeshList
		tgcMaterialsFiltered = #()
		for tgcMeshItem in tgcMeshList do (
			appendIfUnique tgcMaterialsFiltered tgcMeshItem.tgcMaterialItem
		)
		tgcMaterials = tgcMaterialsFiltered		
		*/
		
		--BoundingBox de la escena
		local tgcBoundingBoxes = #()
		for tgcMeshItem in tgcMeshList do (
			append tgcBoundingBoxes tgcMeshItem.tgcBB
		)
		local sceneBB = Utils_createBoundingBoxForAll tgcBoundingBoxes
		format "\t<sceneBoundingBox min='%' max='%'/>\n" sceneBB.pMin sceneBB.pMax to:outFile
		
		
		--######### MATERIALS ###########
		
		--Exportar sceneMaterials utilizadas
		format "\t<materials count='%'>\n" tgcMaterials.count to:outFile
		for i = 1 to tgcMaterials.count do (
			local tgcMat = tgcMaterials[i]
			local m = tgcMat.materialNode
			
			--Exportar Standard Material
			if tgcMat.subMaterials.count == 0 then (
			
				format "\t\t<m name='%' type='%'>\n" m.name tgcMat.materialClass to:outFile
				
				--Ambient
				local ambColor = m.ambient
				format "\t\t\t<ambient>[%,%,%,%]</ambient>\n" ambColor.red ambColor.green ambColor.blue ambColor.alpha to:outFile

				--Diffuse
				local diffuseColor = m.diffuse
				format "\t\t\t<diffuse>[%,%,%,%]</diffuse>\n" diffuseColor.red diffuseColor.green diffuseColor.blue diffuseColor.alpha to:outFile

				--Specular
				local specularColor = m.specular
				format "\t\t\t<specular>[%,%,%,%]</specular>\n" specularColor.red specularColor.green specularColor.blue specularColor.alpha to:outFile
				
				--Opacity
				format "\t\t\t<opacity>%</opacity>\n" m.opacity to:outFile
				
				--alphaBlendEnable
				format "\t\t\t<alphaBlendEnable>%</alphaBlendEnable>\n" tgcMat.alphaBlendEnable to:outFile
				
				--Comprobar si el material tiene un Bitmap en DiffuseMap
				if tgcMat.withBitmap == true then (
					
					--fileName
					local filename = m.diffuseMap.bitmap.filename
					local textureName = filenameFromPath filename
					
					--UV Tiling
					local uTiling = m.diffuseMap.coords.U_Tiling
					local vTiling = m.diffuseMap.coords.V_Tiling
					
					--UV Offset
					local uOffset = m.diffuseMap.coords.U_Offset
					local vOffset = m.diffuseMap.coords.V_Offset
					
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
					
					local subMat = tgcSubMat.materialNode
					
					--Export sub-material
					format "\t\t\t<subM name='%' type='%'>\n" subMat.name tgcSubMat.materialClass to:outFile
					
					--Ambient
					local ambColor = subMat.ambient
					format "\t\t\t\t<ambient>[%,%,%,%]</ambient>\n" ambColor.red ambColor.green ambColor.blue ambColor.alpha to:outFile

					--Diffuse
					local diffuseColor = subMat.diffuse
					format "\t\t\t\t<diffuse>[%,%,%,%]</diffuse>\n" diffuseColor.red diffuseColor.green diffuseColor.blue diffuseColor.alpha to:outFile

					--Specular
					local specularColor = subMat.specular
					format "\t\t\t\t<specular>[%,%,%,%]</specular>\n" specularColor.red specularColor.green specularColor.blue specularColor.alpha to:outFile
					
					--Opacity
					format "\t\t\t\t<opacity>%</opacity>\n" subMat.opacity to:outFile
					
					--alphaBlendEnable
					format "\t\t\t\t<alphaBlendEnable>%</alphaBlendEnable>\n" tgcMat.alphaBlendEnable to:outFile
					
					--Comprobar si el material tiene un Bitmap en DiffuseMap
					if tgcSubMat.withBitmap == true then (
						
						--fileName
						local filename = subMat.diffuseMap.bitmap.filename
						local textureName = filenameFromPath filename
						
						--UV Tiling
						local uTiling = subMat.diffuseMap.coords.U_Tiling
						local vTiling =subMat.diffuseMap.coords.V_Tiling
						
						--UV Offset
						local uOffset = subMat.diffuseMap.coords.U_Offset
						local vOffset = subMat.diffuseMap.coords.V_Offset
						
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
		
		
		--######### LIGHTMAPS ###########
		
		--Generar 1 Lightmap para todo el escenario y mapeos UV para cada mesh
		if lightmapEnabled then (
			local lightmapFileName = lightmapName + lightmapFormat
			local fullLightmapFileName = lightmapDir + lightmapFileName
			generateSceneLightMap tgcMeshList fullFileName:fullLightmapFileName imageFormat:lightmapFormat xSize:lightmapWidth ySize:lightmapHeight lightmapLayers:lightmapLayers lightmapDebugMeshEnabled:lightmapDebugMeshEnabled
		)

		
		--######### MESHES ###########
		
		format "\t<meshes count='%' >\n" tgcMeshList.count to:outFile
		for i = 1 to tgcMeshList.count do (

			local tgcMeshItem = tgcMeshList[i]
			local selectedMesh = tgcMeshItem.meshNode
			
			--Check visibility flag	
			local visibility = 1.0
			if tgcMeshItem.alphaBlendEnable == true then (
				visibility = 0
			)
			/*
			if (selectedMesh[1] != undefined and selectedMesh[1].controller != undefined) then (
				visibility = selectedMesh.visibility.controller.value
			)
			*/
			
			--WireColor 
			local wireColor = selectedMesh.wireColor

			--Export lightmap
			local meshLightmapEnabled = false
			local meshLightmapName = ""
			if lightmapEnabled then (
				--Ver si pertence a un layer contemplado para lightmaps
				if (findItem lightmapLayers selectedMesh.layer.name) > 0 then (
					meshLightmapName = lightmapFileName
					meshLightmapEnabled = true
				)
			)

			--take snapshot
			--local tmesh = snapshotAsMesh selectedMesh
			local tmesh = selectedMesh
			
			--Mesh type
			local meshType = "Original"
			if tgcMeshItem.parentInstance != undefined then (
				meshType = "Instance"
			)
			
			--Material id y maxMatId
			local matId = -1
			local maxMatId = -1
			if tgcMeshItem.tgcMaterialItem != undefined then (
				matId = tgcMeshItem.tgcMaterialItem.matId - 1
				if tgcMeshItem.tgcMaterialItem.subMaterials.count > 0 then (
					maxMatId = tgcMeshItem.tgcMaterialItem.subMaterials.count
				) else (
					maxMatId = 1
				)
			)
			
			
			
			--Start mesh
			format ("\t\t<mesh name='%' layer='%' type='%' matId='%' color='[%,%,%]' visibility='%' lightmap='%'>\n") selectedMesh.name selectedMesh.layer.name meshType matId wireColor.red wireColor.green wireColor.blue visibility meshLightmapName to:outFile
			
			--BoundingBox
			format "\t\t\t<boundingBox min='%' max='%'/>\n"  tgcMeshItem.tgcBB.pMin tgcMeshItem.tgcBB.pMax to:outFile
			
			
			--Mesh Original
			local withColorPerVert = getNumCPVVerts tmesh
			if tgcMeshItem.parentInstance == undefined then (
				
				--Obtener toda la informacion de: Coordinates Index, Texture Coordinates Index, Colors Index and Material Ids
				local tgcMeshFaces = #()
				for t = 1 to tmesh.numfaces do (
					--Coordinates Index
					local face = getFace tmesh t
					
					--Texture Coordinates Index
					local texFace = getTVFace tmesh t
					
					--Colors Index
					if withColorPerVert > 0 then (
						local colorFace = getVCFace tmesh t
					) else (
						local colorFace = point3 1 1 1
					)
					
					--Material ID
					local faceMatId = getFaceMatID tmesh t
					if faceMatId > maxMatId then (
						faceMatId = maxMatId
					)
					
					--Lightmap Texture Coordinates index
					if meshLightmapEnabled then (
						local texFaceLM = meshop.getMapFace tmesh LIGHTMAP_CHANNEL_NUM t
					) else (
						local texFaceLM = undefined
					)
					
					
					local tgcMeshFaceObj = TgcMeshFace index:t face:face tvFace:texFace vcFace:colorFace faceMatId:faceMatId texFaceLM:texFaceLM
					append tgcMeshFaces tgcMeshFaceObj
				)
				
				--Ordenar triángulos de la malla según su material ID, para que vayan todos en batches de textura
				qsort tgcMeshFaces compareTgcMeshFace
			
			
				--Armar streams de strings con los valores de cada triángulo
				local streamCoordinatesIndex = stringstream ""
				local streamTexCoordsIndex = stringstream ""
				local streamColorsIndex = stringstream ""
				local streamMaterialIds = stringstream ""
				local streamTexCoordsLightMapIndex = stringstream ""
				for tgcMeshFaceObj in tgcMeshFaces do (
					local face = tgcMeshFaceObj.face
					format "% % % " (face.x as integer - 1) (face.y as integer - 1) (face.z as integer - 1) to:streamCoordinatesIndex 
					
					local texFace = tgcMeshFaceObj.tvFace
					format "% % % " (texFace.x as integer - 1) (texFace.y as integer -1) (texFace.z as integer -1) to:streamTexCoordsIndex 
					
					local colorFace = tgcMeshFaceObj.vcFace
					format "% % % " (colorFace.x as integer -1) (colorFace.y as integer -1) (colorFace.z as integer -1) to:streamColorsIndex 
					
					local faceMatId = tgcMeshFaceObj.faceMatId
					format "% " (faceMatId - 1) to:streamMaterialIds 
					
					if meshLightmapEnabled then (
						local texFaceLM = tgcMeshFaceObj.texFaceLM
						format "% % % " (texFaceLM.x as integer - 1) (texFaceLM.y as integer -1) (texFaceLM.z as integer -1) to:streamTexCoordsLightMapIndex 
					)
				)
				format "\t\t\t<coordinatesIdx count='%'>%</coordinatesIdx>\n" (tmesh.numfaces * 3) (streamCoordinatesIndex as string) to:outFile
				format "\t\t\t<textCoordsIdx count='%'>%</textCoordsIdx>\n" (tmesh.numfaces * 3) (streamTexCoordsIndex as string) to:outFile
				format "\t\t\t<colorsIdx count='%'>%</colorsIdx>\n" (tmesh.numfaces * 3) (streamColorsIndex as string) to:outFile
				--Solo imprimir indices de MaterialId si tiene mas de una
				if maxMatId > 1 then (
					format "\t\t\t<matIds count='%'>%</matIds>\n" tmesh.numfaces (streamMaterialIds as string) to:outFile
				) else (
					format "\t\t\t<matIds count='%'>%</matIds>\n" maxMatId 0 to:outFile
				)
				--Exportar texCoords de LightMaps solo si los LightMaps estan habilitados
				if meshLightmapEnabled then (
					format "\t\t\t<textCoordsLightMapIdx count='%'>%</textCoordsLightMapIdx>\n" (tmesh.numfaces * 3) (streamTexCoordsLightMapIndex as string) to:outFile
				)
				

				--Vertices
				local streamVertices = stringstream ""
				for j = 1 to tmesh.numverts do (
					local vert = getVert tmesh j
					local newVert = Utils_formatPositionValue vert exportFormat
					
					format "% % % " newVert.x newVert.y newVert.z to:streamVertices
				)
				format "\t\t\t<vertices count='%'>%</vertices>\n" (tmesh.numverts * 3) (streamVertices as string) to:outFile
				
				
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
				format "\t\t\t<normals count='%'>%</normals>\n" (meshNormals.count * 3) (streamNormals as string) to:outFile
				
				--Texture coordinates
				local streamTexCoordinates = stringstream ""
				for k = 1 to tmesh.numtverts do (
					local texCoord = getTVert tmesh k
					local newTexCoord = Utils_formatTextureCoordinates texCoord exportFormat
					
					format "% % " newTexCoord.x newTexCoord.y to:streamTexCoordinates
				)
				format "\t\t\t<texCoords count='%'>%</texCoords>\n" (tmesh.numtverts * 2) (streamTexCoordinates as string) to:outFile

				--Colors
				local streamColors = stringstream ""
				local numCPVerts = getNumCPVVerts tmesh
				if numCPVerts > 0 then (
					for c = 1 to numCPVerts do (
						local vertColor = getVertColor tmesh c
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
				format "\t\t\t<colors count='%'>%</colors>\n" (numCPVerts * 3) (streamColors as string) to:outFile

				--LightMaps Texture coordinates
				if meshLightmapEnabled then (
					local streamLightMapTexCoordinates = stringstream ""
					local texCoordsLightMapCount = meshop.getNumMapVerts tmesh LIGHTMAP_CHANNEL_NUM
					for k = 1 to texCoordsLightMapCount do (
						local texCoordLM = meshop.getMapVert tmesh LIGHTMAP_CHANNEL_NUM k
						local newTexCoord = Utils_formatTextureCoordinates texCoordLM exportFormat
						
						format "% % " newTexCoord.x newTexCoord.y to:streamLightMapTexCoordinates
					)
					format "\t\t\t<texCoordsLightMap count='%'>%</texCoordsLightMap>\n" (texCoordsLightMapCount * 2) (streamLightMapTexCoordinates as string) to:outFile
				)

				
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
				format "\t\t\t<tangents count='%'>%</tangents>\n" (meshNormals.count * 3) (streamTangents as string) to:outFile
				format "\t\t\t<binormals count='%'>%</binormals>\n" (meshNormals.count * 3) (streamBinormals as string) to:outFile
				
				
			
			
			--Mesh instancia
			) else (
				
				--malla original
				local parentIndex = tgcMeshItem.parentInstance.meshId - 1
				format "\t\t\t<originalMesh>%</originalMesh>\n" parentIndex to:outFile
				
				--solo exportar posicion, rotacion y escala, en base a la diferencia con el padre
				local meshPosition = tgcMeshItem.meshPosition - tgcMeshItem.parentInstance.meshPosition
				local meshRotQuat = tgcMeshItem.meshRotQuat - tgcMeshItem.parentInstance.meshRotQuat
				local meshScale = tgcMeshItem.meshScale / tgcMeshItem.parentInstance.meshScale
				
				format "\t\t\t<transform pos='%' rotQuat='[%,%,%,%]' scale='%'/>\n" meshPosition meshRotQuat.x meshRotQuat.y meshRotQuat.z meshRotQuat.w meshScale to:outFile
			)
			
			--User properties
			if tgcMeshItem.tgcUserPropArray != undefined then (
				format "\t\t\t<userProps count='%'>\n" tgcMeshItem.tgcUserPropArray.count to:outFile
				for prop in tgcMeshItem.tgcUserPropArray do (
					format "\t\t\t\t<%>%</%>\n" prop.key prop.value prop.key to:outFile
				)
				format "\t\t\t</userProps>\n" to:outFile
			)

			--Finish mesh
			format "\t\t</mesh>\n" to:outFile
		)
		--Finish Meshes
		format "\t</meshes>\n" to:outFile

		
		--Exportar información de PortalRendering si está activado
		if portalExportEnable == true then (
			result = exportPortalInfo objectList tgcMeshList portalPortalsLayerName portalCellsLayerName exportFormat outFile
			if result != 0 then
				return -1
		)

		
		--Eliminar todos los mesh que se clonaron para la exportacion
		for i = 1 to tgcMeshList.count do (
			delete tgcMeshList[i].meshNode
		)
		
		--Volver a mostrar todos los mesh originales que se habian ocultado para la exportacion
		for i = 1 to objectList.count do (
			unhide objectList[i]
		)
		
		

		--End exporting
		format "</tgcScene>\n" to:outFile

		close outFile

		messagebox "Scene exported OK"
	)
	


	

	
--############################################################
--##################### USER INTERFACE  ######################
--############################################################

	--######### TEXTURE SETTINGS ###########
	rollout texturesSettings "Textures Settings" (
		
		--Widgets
		checkbox w_textureEnable "Copy textures to folder" checked:true
		edittext w_textureDir "Output dir:" text:"Textures"
		edittext w_texturePreffix "Files preffix:" text:""
		
		on w_textureEnable changed theState  do (
			w_textureDir.enabled = theState
			w_texturePreffix.enabled = theState
		)
		
	)
	
	
	--######### LIGHTMAP SETTINGS ###########
	rollout lightmapsSettings "LightMaps Settings" (
		
		--Widgets
		checkbox w_lightmapEnable "Enable LightMaps" checked:false
		edittext w_lightmapDir "Output dir:" text:"LightMaps" enabled:false
		edittext w_lightmapName "File name:" text:"lightmap" enabled:false
		checkbox w_debugMeshEnabled "Enable Debug Mesh" checked:false enabled:false
		dropdownlist w_lightmapImageFormat "Image format" items:#(".jpg",".png") width:80 enabled:false
		spinner w_lightmapWidth "Width:" range:[0,2048,512] type:#integer enabled:false
		spinner w_lightmapHeight "Height:" range:[0,2048,512] type:#integer enabled:false
		MultiListBox w_lightmapSelectedLayers "Layers to export:" height:5 enabled:false
		--button w_testLightmap "Test selected LightMap" width:150 height:20
		
		on w_lightmapEnable changed theState  do (
			w_lightmapDir.enabled = theState
			w_debugMeshEnabled.enabled = theState
			w_lightmapName.enabled = theState
			w_lightmapImageFormat.enabled = theState
			w_lightmapWidth.enabled = theState
			w_lightmapHeight.enabled = theState
			w_lightmapSelectedLayers.enabled = theState
			
			--Refrescar layers, dejar solo los que tienen objetos adentro
			local layerList = Utils_getExistingLayersWithNodes()
			local layerNames = #()
			for i = 1 to layerList.count do (
				append layerNames layerList[i].name
			)
			sort layerNames
			
			--Pre-seleccionar los layers que tienen habilitado GlobalIlumination
			local layerSelection = #()
			for i = 1 to layerNames.count do (
				local l = LayerManager.getLayerFromName layerNames[i]
				if l.isGIExcluded == false then (
					append layerSelection i
				)
			)
			
			w_lightmapSelectedLayers.items = layerNames
			w_lightmapSelectedLayers.selection = layerSelection
		)
		
		/*
		on w_testLightmap pressed do (
			if selection.count > 0 then (
				selectedMesh = selection[1]
				if (getGeometry selectedMesh) == true then (
					tempFileName = "temp_lightmap" + w_lightmapImageFormat.selected
					lBitmap = generateLightMap selectedMesh fullFileName:tempFileName imageFormat:"jpg" xSize:w_lightmapWidth.value ySize:w_lightmapHeight.value
					
					--Mostrar render
					display lBitmap
					deleteFile tempFileName
				)
			) 
		)
		*/
	)
	
	
	--######### PORTAL RENDERING ###########
	rollout portalRendering "Portal Rendering" (
		
		--Widgets
		checkbox w_portalEnable "Export Portal info" checked:false
		edittext w_portalPortalsLayer "Portals layer:" text:"Portals" enabled:false
		edittext w_portalCellsLayer "Cells layer:" text:"Cells" enabled:false
		
		on w_portalEnable changed theState  do (
			w_portalPortalsLayer.enabled = theState
			w_portalCellsLayer.enabled = theState
		)
	)
	
	
	--######### GENERAL SETTINGS ###########
	--Widgets
	edittext w_sceneName "Scene name:" text:"Scene01"
	radiobuttons w_exportFormat labels:#("DirectX", "OpenGL", "Raw")
	button w_exportScene "Export Scene" width:150 height:20
	
	
	--Event: button Export
	on w_exportScene pressed do
	(
		--Obtener layers para exportar lightmaps
		lightmapLayers = #()
		for i in lightmapsSettings.w_lightmapSelectedLayers.selection do (
			layerName = lightmapsSettings.w_lightmapSelectedLayers.items[ i ]
			append lightmapLayers layerName
		)
		
		--Crear copia de todos los objetos seleccionados
		selectionCopy = #()
		for s in selection do (
			append selectionCopy s
		)
		
	
		--exportar
		exportScene \
			objectList:selectionCopy sceneName:w_sceneName.text exportFormat:w_exportFormat.state \
			lightmapEnabled:lightmapsSettings.w_lightmapEnable.checked lightmapDirName:lightmapsSettings.w_lightmapDir.text \
			lightmapWidth: lightmapsSettings.w_lightmapWidth.value lightmapHeight:lightmapsSettings.w_lightmapHeight.value \
			lightmapFormat:lightmapsSettings.w_lightmapImageFormat.selected lightmapName:lightmapsSettings.w_lightmapName.text \
			lightmapLayers:lightmapLayers lightmapDebugMeshEnabled:lightmapsSettings.w_debugMeshEnabled.checked \
			copyTextures:texturesSettings.w_textureEnable.checked \
			textureDirName:texturesSettings.w_textureDir.text texturePreffix:texturesSettings.w_texturePreffix.text \
			portalExportEnable:portalRendering.w_portalEnable.checked portalPortalsLayerName:portalRendering.w_portalPortalsLayer.text \
			portalCellsLayerName:portalRendering.w_portalCellsLayer.text \
	)

	
	--######### MAIN MENU ###########
	on TgcSceneExpoter open do (
		addRollout texturesSettings rolledUp:true
		addRollout lightmapsSettings rolledUp:true
		addRollout portalRendering rolledUp:true
	)
	
	on TgcSceneExpoter close do (
		removeRollout texturesSettings
		removeRollout lightmapsSettings
		removeRollout portalRendering
	)
	
)
	
	
		













