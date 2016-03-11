/*
#####################################################
			MaxScriptUtils			
---------------------------------------------------
Utilidades generales de MaxScript
#####################################################


*/


---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Validar que un objeto sea del tipo Geometry
*/
function Utils_isGeometry o = (
	Superclassof o == Geometryclass and classof o != TargetObject
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Invierte el orden de los elementos de un array.
* Devuelve un nuevo array con el orden invertido.
*/
function Utils_reverseArray arrayObj = (
	local invertedArray = #()
	for i = arrayObj.count to 1 by -1 do (
		append invertedArray arrayObj[i]
	)
	invertedArray
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Truncar precision innecesaria en valores cerca de cero
*/
function Utils_cleanupFloat f = (
	if ((f > -0.0001) and (f < 0.0001)) do f = 0
	f
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Truncar precision innecesaria en valores cerca de cero
*/
function Utils_cleanupPoint3 pnt3 = (
	if ((pnt3.x > -0.0001) and (pnt3.x < 0.0001)) do pnt3.x = 0
	if ((pnt3.y > -0.0001) and (pnt3.y < 0.0001)) do pnt3.y = 0
	if ((pnt3.z > -0.0001) and (pnt3.z < 0.0001)) do pnt3.z = 0
	pnt3
)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Convierte un valor de posicion segun el formato de exportacion elegido
*/
function Utils_formatPositionValue p exportFormat = (
	--DirectX format
	if(exportFormat == 1) then (
		newP = Utils_cleanupPoint3 (point3 p.x p.z p.y)
		
	--OpenGL format
	) else if(exportFormat == 2) then (
		newP = Utils_cleanupPoint3 (point3 p.x p.z (-p.y))
		
	--RAW format
	) else (
		newP = Utils_cleanupPoint3 (point3 p.x p.y p.z)
	)
	
	newP
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Convierte un valor de escala segun el formato de exportacion elegido
*/
function Utils_formatScaleValue p exportFormat = (
	--DirectX format
	if(exportFormat == 1) then (
		newP = Utils_cleanupPoint3 (point3 p.x p.z p.y)
		
	--OpenGL format
	) else if(exportFormat == 2) then (
		newP = Utils_cleanupPoint3 (point3 p.x p.z (-p.y))
		
	--RAW format
	) else (
		newP = Utils_cleanupPoint3 (point3 p.x p.y p.z)
	)
	
	newP
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Convierte un valor de rotacion en Quaternion segun el formato de exportacion elegido
*/
function Utils_formatQuaternion q exportFormat = (
	local x = Utils_cleanupFloat q.x
	local y = Utils_cleanupFloat q.y
	local z = Utils_cleanupFloat q.z
	local w = Utils_cleanupFloat q.w
	newQ = quat x z y w
	newQ
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Convierte un valor de coordenada de textura segun el formato de exportacion elegido
*/
function Utils_formatTextureCoordinates p exportFormat = (
	--DirectX format
	if(exportFormat == 1) then (
		newP = Utils_cleanupPoint3 (point3 p.x (1-p.y) p.z)
		
	--OpenGL format and RAW format
	) else (
		newP = Utils_cleanupPoint3 (point3 p.x p.y p.z)
	)
	
	newP
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Invierte los ejex Y-Z en una matriz de transformacion
*/
function Utils_flipYZTransform Tform = (
	local axis1,axis2,axis3,t,m

	-- computes the matrix
	axis1 = point3 1 0 0 ;
	axis2 = point3 0 0 1 ;
	axis3 = point3 0 -1 0 ;
	t = point3 0 0 0 ;
	m=matrix3 axis1 axis2 axis3 t ;

	-- multiplies by the inverse
	Tform = Tform*inverse(m) ;

	Tform
)	

---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Convierte un KeyFrame en un Integer
*/
function Utils_formatKeyFrame t = (
	local frameNumber = substring (t as string) 1 ((t as string).count - 1)
	frameNumber = (floor (frameNumber as integer) as integer)
	frameNumber
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
/*
* Acomodar objetos que tienen scale negativo.
* Si el objeto pose scale negativo significa que se le aplico Mirror.
* En ese caso se resetea su posicion, escla y rotacion, se aplica XForm y luego NormalModifier
*/ 
function Utils_resetXFormAndFlip obj = (
	--Ver si tiene Scale negativo
	if ((obj.scale.x < 0) OR (obj.scale.y < 0) OR (obj.scale.z < 0)) then (
		
		--Reset rotation
		local rotvalue = obj.rotation
		obj.rotation= (quat 0 0 0 1)
		
		--Obtener inverse de matriz
		local M = obj.transform
		obj.transform = transMatrix obj.pos
		local p = obj.objecttransform * (inverse obj.transform)
		
		--Cargar valores reset
		obj.objectoffsetPos = [0,0,0]
		obj.objectoffsetRot= (quat 0 0 0 1)
		obj.objectoffsetScale = [1,1,1]
		M.translation = [0,0,0]
		
		--Multiplicar por inversa
		M= p * M
		
		--Aplicar modifier XForm con matriz calculada
		local xformMod = XForm()
		addmodifier obj xformMod
		xformMod.gizmo.transform = M
		
		--Aplicar modifier Normal
		obj.rotation=rotvalue
		addmodifier obj (Normalmodifier flip:true)
	)
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Copia un bitmap a un archivo destino
*/
function Utils_copyTexture targetBitmap destPath = (
	deleteFile destPath
	local newBitmap = Bitmap targetBitmap.width targetBitmap.height
	newBitmap.filename = destPath
	copy targetBitmap newBitmap
	save newBitmap
	
	--copyFile targetBitmap.filename destPath
)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve una lista con todos los layers existentes. Solo devuelve aquellos layers que poseen al menos un objeto adentro.
*/
function Utils_getExistingLayersWithNodes = (
	local layersList = #()
	
	for i = 0 to layerManager.count-1 do (
		ilayer = layerManager.getLayer i
		ilayer.nodes &layerNodes
		if layerNodes.count > 0 then (
			append layersList ilayer
		)
	)
	layersList
)
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Estructura para almacenar SubMaterials
*/
struct TgcSubMaterial
(
	parentMaterial,
	materialNode,
	materialClass,
	withBitmap,
	alphaBlendEnable
)

/*
* Estructura para almacenar Materials a exportar
*/
struct TgcMaterial
(
	materialNode,
	materialClass,
	sceneMaterialIndex,
	matId,
	withBitmap,
	alphaBlendEnable,
	subMaterials
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve true si un modelo tiene Material asignado.
* Solo contempla StandardMaterial y MultiMaterial.
* El Material se busca dentro de SceneMaterials para ver si existe.
*/
function Utils_meshHasMaterial selectedMesh = (

	if selectedMesh.material == undefined then (
		return false
	)
	
	--Buscar en sceneMaterials
	local matId = findItem sceneMaterials selectedMesh.material
	if (matId == 0) or (matId > sceneMaterials.count) then (
		return false
	)

	--Ver si el tipo de Material es correcto
	local matClass = classof selectedMesh.material
	if (matClass != StandardMaterial) and (matClass != MultiMaterial) then (
		return false
	)
	
	return true
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve true si un Material tiene un DiffuseMap con Bitmap
*/ 
function Utils_materialHasBitmap matNode = (
	local flag = matNode.diffuseMap != undefined and (hasProperty matNode.diffuseMap "bitmap") and matNode.diffuseMap.bitmap != undefined
	flag
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve true si un Material tiene un OpacityMap con Bitmap
*/ 
function Utils_materialHasOpacityMapBitmap matNode = (
	local flag = matNode.opacityMap  != undefined and (hasProperty matNode.opacityMap  "bitmap") and matNode.opacityMap .bitmap != undefined
	flag
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Crea un array con todos los Materials que van a ser exportados.
* Devuelve una lista de TgcMaterial
* Solo tienen en cuenta los Materials que son del tipo StandardMaterial o MultiMaterial.
* Solo se tienen en cuenta aquellos Materials que son realmente utilizados por algun modelo a exportar
*/
function Utils_createMaterialList objectList = (
	local tgcMaterials = #()
	
	for i = 1 to sceneMaterials.count do (
		local matNode = sceneMaterials[i]
		local matClass = classof matNode
		
		--Validar que tipo de Material es
		if (matClass == StandardMaterial) or (matClass == MultiMaterial) then (
		
			--Ver si algun modelo utiliza este material
			local materialIsUsed = false
			for meshNode in objectList do (
				local hasMaterial = Utils_meshHasMaterial meshNode
				if hasMaterial == true then (
					if meshNode.material == matNode then (
						materialIsUsed = true
						break
					)
				
				)
			)
			
			if materialIsUsed == false then (
				continue
			)
		
			local withBitmap = false
			local alphaBlendEnable = false
			local subMaterials = #()
			
			--Analizar si tiene Bitmap
			if matClass == StandardMaterial then (
				withBitmap = Utils_materialHasBitmap matNode
				alphaBlendEnable = Utils_materialHasOpacityMapBitmap matNode

			--Crear SubMaterials
			) else if matClass == MultiMaterial then (
				for subMat in matNode.materialList do (
					
					--Solo tener en cuenta SubMaterials que sean StandardMaterial
					local subMatClass = classof subMat
					if subMatClass == StandardMaterial then (
						local subMatWithBitmap = Utils_materialHasBitmap subMat
						local subMapAlphaBlendEnable = Utils_materialHasOpacityMapBitmap subMat
						if subMapAlphaBlendEnable == true then (
							alphaBlendEnable = true
						)
						local tgcSubMat = TgcSubMaterial materialNode:subMat materialClass:subMatClass withBitmap:subMatWithBitmap alphaBlendEnable:subMapAlphaBlendEnable
						append subMaterials tgcSubMat
					)
				)
			)
		
			--agregar a la lista
			local tgcMat = TgcMaterial materialNode:matNode materialClass:matClass sceneMaterialIndex:i matId:-1 withBitmap:withBitmap subMaterials:subMaterials alphaBlendEnable:alphaBlendEnable
			append tgcMaterials tgcMat
			tgcMat.matId = tgcMaterials.count
		)
	)
	
	--Cargar parentMaterial en SubMaterials
		for tgcMat in tgcMaterials do (
			if tgcMat.subMaterials.count > 0 then (
				for tgcSubMat in tgcMat.subMaterials do (
					tgcSubMat.parentMaterial = tgcMat
				)
			)
		)
	
		
		tgcMaterials
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve el TgcMaterial de un modelo, en base a la lista de Materials a exportar.
* Se debe llamar a antes a la funcion meshHasMaterial para saber si el modelo tiene Material
*/
function Utils_getMeshMaterial selectedMesh tgcMaterials = (
	for tgcMat in tgcMaterials do (
		local meshMat = selectedMesh.material
		if meshMat == tgcMat.materialNode then (
			return tgcMat
		)
	)
	local flag = undefined
	flag
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Estructura para almacenar un BoundingBox
*/
struct TgcBoundingBox
(
	pMin,
	pMax
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Crea el TgcBoundingBox de un EditableMesh, recorriendo los vertices del mismo.
*/
function Utils_getMeshBoundingFromVertices tmesh exportFormat = (
	local minX = undefined
	local minY = undefined
	local minZ = undefined
	local maxX = undefined
	local maxY = undefined
	local maxZ = undefined
	
	--Buscar puntos extremos
	for i = 1 to tmesh.numverts do (
		local vert = getVert tmesh i
		vert = Utils_formatPositionValue vert exportFormat
		
		if (minX == undefined) or (vert.x < minX) then (
			minX = vert.x
		)
		if (minY == undefined) or (vert.y < minY) then (
			minY = vert.Y
		)
		if (minZ == undefined) or (vert.z < minZ) then (
			minZ = vert.z
		)
		
		if (maxX == undefined) or (vert.x > maxX) then (
			maxX = vert.x
		)
		if (maxY == undefined) or (vert.y > maxY) then (
			maxY = vert.y
		)
		if (maxZ == undefined) or (vert.z > maxZ) then (
			maxZ = vert.z
		)
	)
	
	local minP = point3 minX minY minZ
	local maxP = point3 maxX maxY maxZ
	local tgcbb = TgcBoundingBox pMin:minP pMax:maxP
	tgcbb
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Crea el TgcBoundingBox de un modelo
*/
function Utils_getMeshBoundingBox selectedMesh exportFormat = (
	/*Forma tradicional, no devuelve bien los valores cuando el objeto esta rotado
	boundingBox = nodeLocalBoundingBox selectedMesh
	bbMin = Utils_formatPositionValue boundingBox[1] exportFormat
	bbMax = Utils_formatPositionValue boundingBox[2] exportFormat
	tgcbb = TgcBoundingBox pMin:bbMin pMax:bbMax
	*/
	
	local tmesh = snapshotAsMesh selectedMesh
	local bb = Utils_getMeshBoundingFromVertices tmesh exportFormat
	bb
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Crea el TgcBoundingBox de un modelo en base a su posición en un Frame determinado
*/
function Utils_getMeshBoundingBoxAtTime selectedMesh exportFormat t = (
	local tmesh = at time t snapshotAsMesh selectedMesh
	local bb = Utils_getMeshBoundingFromVertices tmesh exportFormat
	bb
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Crea un TgcBoundingBox que contenga a todos los TgcBoundingBox pasados en la lista
*/
function Utils_createBoundingBoxForAll tgcBoundingBoxes = (
	local minX = undefined
	local minY = undefined
	local minZ = undefined
	local maxX = undefined
	local maxY = undefined
	local maxZ = undefined
	
	--Buscar puntos extremos
	for bb in tgcBoundingBoxes do (
	
		if (minX == undefined) or (bb.pMin.x < minX) then (
			minX = bb.pMin.x
		)
		if (minY == undefined) or (bb.pMin.y < minY) then (
			minY = bb.pMin.y
		)
		if (minZ == undefined) or (bb.pMin.z < minZ) then (
			minZ = bb.pMin.z
		)
		
		if (maxX == undefined) or (bb.pMax.x > maxX) then (
			maxX = bb.pMax.x
		)
		if (maxY == undefined) or (bb.pMax.y > maxY) then (
			maxY = bb.pMax.y
		)
		if (maxZ == undefined) or (bb.pMax.z > maxZ) then (
			maxZ = bb.pMax.z
		)
		
	)
	
	local minP = point3 minX minY minZ
	local maxP = point3 maxX maxY maxZ
	local tgcbb = TgcBoundingBox pMin:minP pMax:maxP
	tgcbb
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve un array con los 8 vértices extremos de un BoundingBox
*/
function Utils_computeBoundingBoxCorners tgcBB = (
	local corners = #()
	
	append corners tgcBB.pMin
	append corners (point3 tgcBB.pMin.x tgcBB.pMin.y tgcBB.pMax.z)
	
	append corners (point3 tgcBB.pMin.x tgcBB.pMax.y tgcBB.pMin.z)
	append corners (point3 tgcBB.pMin.x tgcBB.pMax.y tgcBB.pMax.z)
	
	append corners (point3 tgcBB.pMax.x tgcBB.pMin.y tgcBB.pMin.z)
	append corners (point3 tgcBB.pMax.x tgcBB.pMin.y tgcBB.pMax.z)
	
	append corners (point3 tgcBB.pMax.x tgcBB.pMax.y tgcBB.pMin.z)
	append corners tgcBB.pMax
	
	corners
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve el centro de un BoundingBox
*/
function Utils_computeBoundingBoxCenter tgcBB = (
	local size = tgcBB.pMax - tgcBB.pMin
	local p = point3 (tgcBB.pMin.x + size.x / 2) (tgcBB.pMin.y + size.y / 2) (tgcBB.pMin.z + size.z / 2)
	p
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Estructura para almacenar un Plano
*/
struct TgcPlane
(
	normal,
	d
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Crea un plano a partir de tres puntos de un polígono
* Hay que especificar los puntos en clockwise-order para que tome la dirección hacia lado requerido
*/
function Utils_createPlaneFromPoints p1 p2 p3 exportFormat = (
	local vectorA = p2 - p1
	local vectorB = p3 - p1
	local planeNormal = normalize (cross vectorA vectorB)
	--planeNormal = normalize (cross vectorB vectorA) --Normal hacia afuera
	local d = -(planeNormal.x * p1.x + planeNormal.y * p1.y + planeNormal.z * p1.z)
	
	local tgcPlaneObj = TgcPlane normal:(Utils_formatPositionValue planeNormal exportFormat) d:(Utils_cleanupFloat d)
	tgcPlaneObj
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve un array de TgcPlane con un plano por cada cara del mesh.
* Trata el mesh como un EditablePoly. El mesh debe ser convexo.
* Las normales de los planos se devuelven apuntando hacia afuera.
*/
function Utils_getPlanesFromMesh meshNode exportFormat = (
	--Tratar mesh como un cuerpo Convexo
	local tPoly = convertToPoly meshNode
	local numFaces = polyop.getNumFaces tPoly
	
	--Obtener planos de las caras del poligono
	local facePlanes = #()
	for f = 1 to numFaces do (
		local faceVerts = polyop.getFaceVerts tPoly f
		local vert1 = polyop.getVert tPoly faceVerts[1]
		local vert2 = polyop.getVert tPoly faceVerts[2]
		local vert3 = polyop.getVert tPoly faceVerts[3]
		
		--facePlane = Utils_createPlaneFromPoints vert1 vert2 vert3 exportFormat
		local facePlane = Utils_createPlaneFromPoints vert3 vert2 vert1 exportFormat
		append facePlanes facePlane
	)

	facePlanes
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Dado un polígono plano en 3D de una sola cara, devuelve un array de vertices con los vertices extremos 
* que definen la cara.
* Trata el mesh como un EditablePoly y obtiene los vértices de la primera cara.
* Los vértices se devuelven en el orden original
* Devuelve undefined si el mesh tiene más de una cara.
*/
function Utils_getPolygonVertices meshNode exportFormat = (
	local tPoly = convertToPoly meshNode
	local numFaces = polyop.getNumFaces tPoly
	local polyVertices = #()
	
	--Obtener vertices de la primera cara del poligono
	if numFaces == 1 then (
		local faceVerts = polyop.getFaceVerts tPoly 1
		for i = 1 to faceVerts.count do (
			local v = polyop.getVert tPoly faceVerts[i]
			append polyVertices (Utils_formatPositionValue v exportFormat)
		)
		return polyVertices
		
	--Tiene mas de una cara
	) else (
		return undefined
	)
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Estructura almacenar user properties
*/
struct TgcUserProperty
(
	key,
	value
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Devuelve un array de TgcUserProperty con las user properties del nodo
*/
function Utils_getUserPropertiesArray theNode = (
	local propBuffer = getUserPropBuffer theNode
	local tokenArray = filterString propBuffer "=\n"
	
	if tokenArray.count < 1 then (
		return undefined
	)
	
	if (mod tokenArray.count 2) != 0 then (
		return undefined
	)
	
	local propArray = #()
	local count = tokenArray.count / 2
	for i = 1 to count do (
		local key = trimRight (trimLeft tokenArray[i*2-1])
		local value = trimRight (trimLeft tokenArray[i*2])
		local tgcProp = TgcUserProperty key:key value:value
		append propArray tgcProp
	)
	propArray
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	

/*
* Calcula el vector tangente y binormal de un triangulo.
* Devuelve un array con dos valores (tangent, binormal)
* Ya estan normalizados.
* Basado en: http://www.dhpoware.com/demos/d3d9NormalMapping.html
*/
function Utils_computeTangentBinormal pos1 pos2 pos3 texCoord1 texCoord2 texCoord3 normal1 = (
	local tangent = point3 0 0 0
	local binormal = point3 0 0 0

	/* Given the 3 vertices (position and texture coordinates) of a triangle
	 * calculate and return the triangle's tangent vector. The handedness of
	 * the local coordinate system is stored in tangent.w. The bitangent is
	 * then: float3 bitangent = cross(normal, tangent.xyz) * tangent.w.
	 */

	/* Create 2 vectors in object space.
	 *
	 * edge1 is the vector from vertex positions v1 to v2.
	 * edge2 is the vector from vertex positions v1 to v3.
	 */
	local edge1 = normalize (pos2 - pos1)
	local edge2 = normalize (pos3 - pos1)

	/* Create 2 vectors in tangent (texture) space that point in the same
	 * direction as edge1 and edge2 (in object space).
	 *
	 * texEdge1 is the vector from texture coordinates texCoord1 to texCoord2.
	 * texEdge2 is the vector from texture coordinates texCoord1 to texCoord3.
	 */
	local texEdge1 = normalize (point2 (texCoord2.x - texCoord1.x) (texCoord2.y - texCoord1.y))
	local texEdge2 = normalize (point2 (texCoord3.x - texCoord1.x) (texCoord3.y - texCoord1.y))

	/* These 2 sets of vectors form the following system of equations:
	 *
	 *  edge1 = (texEdge1.x * tangent) + (texEdge1.y * bitangent)
	 *  edge2 = (texEdge2.x * tangent) + (texEdge2.y * bitangent)
	 *
	 * Using matrix notation this system looks like:
	 *
	 *  [ edge1 ]     [ texEdge1.x  texEdge1.y ]  [ tangent   ]
	 *  [       ]  =  [                        ]  [           ]
	 *  [ edge2 ]     [ texEdge2.x  texEdge2.y ]  [ bitangent ]
	 *
	 * The solution is:
	 *
	 * [ tangent   ]        1     [ texEdge2.y  -texEdge1.y ]  [ edge1 ]
	 * [           ]  =  -------  [                         ]  [       ]
	 * [ bitangent ]      det A   [-texEdge2.x   texEdge1.x ]  [ edge2 ]
	 *
	 * where:
	 *       [ texEdge1.x  texEdge1.y ]
	 *   A = [                        ]
	 *       [ texEdge2.x  texEdge2.y ]
	 *
	 *   det A = (texEdge1.x * texEdge2.y) - (texEdge1.y * texEdge2.x)
	 *
	 * From this solution the tangent space basis vectors are:
	 *
	 * tangent = (1 / det A) * ( texEdge2.y * edge1 - texEdge1.y * edge2)
	 * bitangent = (1 / det A) * (-texEdge2.x * edge1 + texEdge1.x * edge2)
	 * normal = cross(tangent, bitangent)
	*/

	local det = (texEdge1.x * texEdge2.y) - (texEdge1.y * texEdge2.x)

	if abs(det) < 0.0001 then (    -- almost equal to zero
		tangent.x = 1.0
		tangent.y = 0.0
		tangent.z = 0.0

		binormal.x = 0.0
		binormal.y = 1.0
		binormal.z = 0.0
	) else (
		det = 1.0 / det;

		tangent.x = (texEdge2.y * edge1.x - texEdge1.y * edge2.x) * det
		tangent.y = (texEdge2.y * edge1.y - texEdge1.y * edge2.y) * det
		tangent.z = (texEdge2.y * edge1.z - texEdge1.y * edge2.z) * det
		--tangent.w = 0.0;
		
		binormal.x = (-texEdge2.x * edge1.x + texEdge1.x * edge2.x) * det
		binormal.y = (-texEdge2.x * edge1.y + texEdge1.x * edge2.y) * det
		binormal.z = (-texEdge2.x * edge1.z + texEdge1.x * edge2.z) * det

		tangent = normalize tangent
		binormal = normalize binormal
	)

	/* Calculate the handedness of the local tangent space.
	 * The bitangent vector is the cross product between the triangle face
	 * normal vector and the calculated tangent vector. The resulting bitangent
	 * vector should be the same as the bitangent vector calculated from the
	 * set of linear equations above. If they point in different directions
	 * then we need to invert the cross product calculated bitangent vector.
	 */
	local b = cross normal1 tangent
	local w = dot b binormal
	if w < 0.0 then (
		w = -1.0
	) else (
		w = 1.0
	)
	binormal = b * w;
	
	results = #(tangent, binormal)
	results
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------	


--##############################################################################
--################################### XML PARSER ###################################
--##############################################################################
    

--Devuelve la posicion de la ultima aparicion de la cadena buscada
function Utils_lastIndexOf string search_string  =
(
	local lastIndex = 0
	local text = string
	while (index = (findstring text search_string)) != undefined do
	(
		lastIndex += index
		text = substring text (index + 1) text.count
	)
	
	if lastIndex == 0 then lastIndex = undefined
	
	lastIndex
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
* Representa un atributo de un TAG de XML
*/
struct XmlAttribute
(
	name,
	value
)

---------------------------------------------------------------------------------------------------------------------------------------------------------------
 
/*
* Representa un TAG de XML
*/
struct XmlElement
(
	name,
	listAttributes = #(),
	content = undefined,
	children = #(),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Devuelve el contenido del TAG con TRIM
	*/
	function getContentTrim =
	(
		local aux = trimleft content
		aux = trimright aux
		return aux
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Devuelve el valor del atributo especificado, o undefined si no existe
	*/
	function getAttributeValue attName =
	(
		for att in listAttributes do (
			if att.name == attName then
			(
				return att.value
			)	
		)
		return undefined
	),
		
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Devuelve una lista de hijos con el nombre especificado, o una lista
	* vacia en caso de no haber ninguno
	*/
	function getChildren childName =
	(
		local auxChildren = #()
		for child in children do (
			if child.name == childName then
			(
				append auxChildren child
			)
		)
		return auxChildren
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Devuelve el primer hijo con el nombre especificado, o undefined en caso
	* de no encontrarlo
	*/
	function getChild childName =
	(
		for child in children do (
			if child.name == childName then
			(
				return child
			)
		)
		return undefined
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Devuelve el contenido del hijo especificado
	*/
	function getChildContent childName =
	(
		local child = getChild childName
		if child != undefined then 
			return child.content
			
		return undefined
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Devuelve el contenido del hijo especificado, haciendo Trim
	*/
	function getChildContentTrim childName =
	(
		local childContent = getChildContent childName
		if childContent != undefined then
			return childContent.getContentTrim()
		
		return undefined
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PUBLIC
	* Crea un nuevo atributo con el nombre y el valor especificado
	*/
	function addAttribute attName attValue =
	(
		local att = XmlAttribute()
		att.name = attName
		att.value = attValue
		append listAttributes att
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------

	/*
	* PUBLIC
	* Agrega un XmlElement como hijo de este nodo
	*/
	function addChild xmlElementItem =
	(
		append children xmlElementItem
	)

)
 
---------------------------------------------------------------------------------------------------------------------------------------------------------------
    
/*
* Estructura en objetos de un archivo XML
*/
struct XmlDocument
(
	root,
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* PRIVATE
	* Crear un nuevo elemento en base a otro de DotNet.
	* Carga nombre y atributos.
	* Devuelve el elemento creado.
	*/
	function createNewElement dotNetElement = 
	(
		--Nombre
		local element = XmlElement()
		element.name = dotNetElement.Name
		
		--Atributos
		if dotNetElement.HasAttributes == true then (
			local attCount = dotNetElement.Attributes.Count - 1
			for i = 0 to attCount do (
				
				local dotNetAtt = dotNetElement.Attributes.ItemOf i
				local attribute = XmlAttribute name:dotNetAtt.Name value:dotNetAtt.Value
				append element.listAttributes attribute
			)
		)
		
		--Hijos
		if dotNetElement.HasChildNodes == true then (
			
			--Ver si no es un nodo del tipo "Text" que es contenido interno del tag => <a>xxxxx</a>
			local firstChild = dotNetElement.FirstChild
			if firstChild.NodeType.value__ == 3 then (
				element.content = firstChild.InnerText
			)
			else (
				--Recursividad
				local childCount = dotNetElement.ChildNodes.Count - 1
				for i = 0 to childCount do (
					
					--Solo crear hijo si es del tipo "Element" (enum value = 1. Ver System.Xml.XmlNodeType)
					--XmlNodeType = dotNetClass "System.Xml.XmlNodeType"
					local dotNetChild = dotNetElement.ChildNodes.Item i
					if dotNetChild.NodeType.value__ == 1 then (
						
						--Crear hijo recursivamente
						local childElement = createNewElement dotNetChild
						append element.children childElement
					)
				)
			)
		--Contenido
		) else (
			element.content = dotNetElement.InnerText
		)
		
		
		element
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* PUBLIC
	* Crear estructura de objetos del XML desde un archivo.
	* Parsea el archivo y levanta toda la estructura en objetos.
	*/
	function loadFromFile filePath = 
	(
		--Utilizar XmlDocument DotNet para parsear XML
		dotNetXml = dotNetObject "System.Xml.XmlDocument"
		dotNetXml.Load filePath 
		
		--Crear Root
		dotNetRoot = dotNetXml.DocumentElement
		root = createNewElement dotNetRoot
		
		dotNetXml = undefined
	),

---------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/*
	* PRIVATE
	* Genera el contenido a imprimir de un XmlElement.
	* Tiene en cuenta la identacion
	*/
	function outputXmlElement streamXml element recursionIndex =
	(
		local elementStream = stringStream ""
		
		--Calcular cantidad de identacion necesaria
		for i = 1 to recursionIndex do (
			print "\t" to:elementStream
		)


		--Cargar atributos
		format "<% " element.name to:elementStream
		for i = 1 to element.listAttributes.count do (
			local att = element.listAttributes[i]
			format "%=\"%\"" att.name (att.value as string) to:elementStream
			
			if i == element.listAttributes.count then (
				print ">" to:elementStream
			) else (
				print " " to:elementStream
			)
		)

		--Ver si solo tiene contenido, sin hijos
		if element.children.count == 0 then (
			format "%</%>\n" (element.content as string) element.name to:elementStream

			--Enviar a stream final
			print (elementStream as string) to:streamXml
		)

		--Imprimir recursivamente los hijos
		else 
		(	
			--Imprimir encabezado del padre al stream final
			format "%\n" (elementStream as string) to:streamXml
			
			--Imprimir los hijos
			local nextIndex = recursionIndex + 1
			for child in element.children do (
				outputXmlElement streamXml child nextIndex
			)
			
			--Cerrar encabezado del padre
			format (identation + "</" + element.name + ">" + "\n") to:streamXml
		)
	),
	
---------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
	/*
	* PUBLIC
	* Genera un string de XML en base a los objetos en memoria
	*/
	function generateXmlString = (
		local streamXml = stringstream ""
		
		--Imprimir recursivamente el documento
		outputXmlElement streamXml root 0
		
		(streamXml as string)
	),
	
	/*
	* PUBLIC
	* Genera un archivo XML en la ubicacion especificada, con los datos de los
	* objetos en memoria
	*/
	function saveToFile filePath = (
		--Abrir archivo 
		local file = openFile filePath mode:"wt"
		
		--Generar string
		local stringXml = generateXmlString()
		
		--Grabar en archivo
		format "%" stringXml to:file
		
		--Cerrar archivo
		close file
	)

)

---------------------------------------------------------------------------------------------------------------------------------------------------------------


--###### TESTING ######

/* XML DE PRUEBA
<root att1="value1" att2="value2">
	<hijo0 att="value">
		<superHijo>valor</superHijo>
		<superHijo>  valor con espacio   </superHijo>
		<superHijo att="value">valor</superHijo>
	</hijo0>
	<hijo1 att="value">contenido</hijo1>
	<hijo2 att="value">contenido</hijo2>
	<hijo3 att="value">contenido</hijo3>
	<hijo4 att="value">contenido</hijo4>
	
	<hijosIguales att="primero">contenido</hijosIguales>
	<hijosIguales att="value">contenido</hijosIguales>
	<hijosIguales att="value">contenido</hijosIguales>

</root>


--Forma de Uso

--Cargar XML
doc = XmlDocument()
doc.loadFromFile "C:\\borrar.xml"

--Acceder a valores de un nodo
doc.root.name
doc.root.listAttributes
doc.root.children

--Acceder a hijos de diversa forma
doc.root.children[1].name
doc.root.children[2].name
doc.root.children[3].name
doc.root.children[4].name
doc.root.children[5].name

doc.root.children[1].children[1].name
doc.root.children[1].children[2].content
doc.root.children[5].content
doc.root.children[5].listAttributes[1].value

doc.root.children[1].children[2].getContentTrim()
doc.root.getChildren "hijosIguales"
(doc.root.getChild "hijosIguales").getAttributeValue "att"

--Crear nuevo elemento
unNodo = XmlElement name:"nuevo"
unNodo.addAttribute "attNuevo" 56558
unNodo.content = "Contenido nuevo"
doc.root.addChild unNodo

--Grabar XML
doc.saveToFile "C:\\borrar2.xml"
*/



---------------------------------------------------------------------------------------------------------------------------------------------------------------	




--##############################################################################
--################################## COLLISIONS ###################################
--##############################################################################

CollisionUtils_EPSILON = 0.000001

/*
* Indica la distancia de un punto al plano
*/
function CollisionUtils_distPointPlane q tgcPlaneObj = (
	return (dot tgcPlaneObj.normal q) + tgcPlaneObj.d
)

/*
* Clasifica un Punto respecto de un Plano.
* Resultados: 0: Behind, 1: In front of, 2: Coincident
*/
function CollisionUtils_classifyPointPlane q tgcPlaneObj = (
	local dist = CollisionUtils_distPointPlane q tgcPlaneObj
	
	if dist < (-CollisionUtils_EPSILON) then (
		return 0
	)
	if dist >CollisionUtils_EPSILON then (
		return 1
	)
	return 2
)



/*
* Clasifica un BoundingBox respecto de un polígono convexo
* Resultados: 0: Outside, 1: Inside, 2: Intersect
*/
function CollisionUtils_classifyConvexHullAABB convexHullPlanes tgcBB = (
	local totalIn = 0
	local aabbCorners = Utils_computeBoundingBoxCorners tgcBB
	
	--test all 8 corners against all the sides of the convexHull
	--if all points are behind 1 specific plane, we are out
	--if we are in with all points, then we are fully in
	for p = 1 to convexHullPlanes.count do (
		local inCount = 8
		local ptIn = 1
		
		for i = 1 to 8 do (
			--test this point against the planes
			if (CollisionUtils_classifyPointPlane aabbCorners[i] convexHullPlanes[p]) == 0 then (
				ptIn = 0
				inCount = inCount - 1
			)
		)
		
		--were all the points outside of plane p?
		if inCount == 0 then (
			return 0 --OUTSIDE
		)
		
		--check if they were all on the right side of the plane
		totalIn = totalIn + ptIn;
	)
	
	--so if iTotalIn is 6, then all are inside the view
	if totalIn == 6 then (
		return 1 --INSIDE
	)
		
	--we must be partly in then otherwise
	return 2 --INTERSECT
)









