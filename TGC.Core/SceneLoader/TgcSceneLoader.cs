using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.MeshFactory;
using TGC.Core.PortalRendering;
using TGC.Core.Textures;

namespace TGC.Core.SceneLoader
{
    /// <summary>
    ///     Herramienta para cargar un archivo de escena XML con formato de TGC (tgcScene)
    /// </summary>
    public class TgcSceneLoader
    {
        /// <summary>
        ///     Crear un nuevo Loader
        /// </summary>
        public TgcSceneLoader()
        {
            MeshFactory = new DefaultMeshFactory();
        }

        /// <summary>
        ///     Factory utilizado para crear una instancia de TgcMesh.
        ///     Por default se utiliza la clase DefaultMeshFactory.
        /// </summary>
        public IMeshFactory MeshFactory { get; set; }

        /// <summary>
        ///     Carga una escena a partir de un archivo
        /// </summary>
        /// <param name="filePath">Ubicacion del archivo XML</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar los recursos de escena (Texturas, LightMaps, etc.)</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromFile(string filePath, string mediaPath)
        {
            var xmlString = File.ReadAllText(filePath);
            return loadSceneFromString(xmlString, mediaPath);
        }

        /// <summary>
        ///     Carga una escena a partir de un archivo.
        ///     Como carpeta de Media utiliza la misma carpeta en la que se encuentra el archivo XML de la malla
        /// </summary>
        /// <param name="filePath">Ubicacion del archivo XML</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromFile(string filePath)
        {
            var mediaPath = filePath.Substring(0, filePath.LastIndexOf('\\') + 1);
            return loadSceneFromFile(filePath, mediaPath);
        }

        /// <summary>
        ///     Carga una escena a partir de un archivo en formato .ZIP.
        ///     Se asume que dentro del ZIP se encuentra el archivo XML de la escena y todas las texturas necesarias.
        /// </summary>
        /// <param name="sceneFileName">Nombre del archivo XML que tiene la información de la escena</param>
        /// <param name="zipFilePath">Path del archivo ZIP que contiene la escena.</param>
        /// <param name="extractDir">Path del directorio en donde se va a extraer el ZIP</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromZipFile(string sceneFileName, string zipFilePath, string extractDir)
        {
            //extraer archivo pisando archivos existentes
            var zip = ZipFile.OpenRead(zipFilePath);
            try
            {
                zip.ExtractToDirectory(extractDir);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return loadSceneFromFile(extractDir + sceneFileName, extractDir);
        }

        /// <summary>
        ///     Carga la escena a partir del string del XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar los recursos de escena (Texturas, LightMaps, etc.)</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromString(string xmlString, string mediaPath)
        {
            var parser = new TgcSceneParser();
            var sceneData = parser.parseSceneFromString(xmlString);
            return loadScene(sceneData, mediaPath);
        }

        /// <summary>
        ///     Carga la escena a partir de un objeto TgcSceneData ya parseado
        /// </summary>
        /// <param name="sceneData">Objeto con datos de la escena ya parseados</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar los recursos de escena (Texturas, LightMaps, etc.)</param>
        /// <returns></returns>
        public TgcScene loadScene(TgcSceneData sceneData, string mediaPath)
        {
            var tgcScene = new TgcScene(sceneData.name, null);

            //Cargar Texturas
            var materialsArray = new TgcSceneLoaderMaterialAux[sceneData.materialsData.Length];
            for (var i = 0; i < sceneData.materialsData.Length; i++)
            {
                var materialData = sceneData.materialsData[i];
                var texturesPath = mediaPath + sceneData.texturesDir + "\\";

                //Crear StandardMaterial
                if (materialData.type.Equals(TgcMaterialData.StandardMaterial))
                {
                    materialsArray[i] = createTextureAndMaterial(materialData, texturesPath);
                }

                //Crear MultiMaterial
                else if (materialData.type.Equals(TgcMaterialData.MultiMaterial))
                {
                    var matAux = new TgcSceneLoaderMaterialAux();
                    materialsArray[i] = matAux;
                    matAux.subMaterials = new TgcSceneLoaderMaterialAux[materialData.subMaterials.Length];
                    for (var j = 0; j < materialData.subMaterials.Length; j++)
                    {
                        matAux.subMaterials[j] = createTextureAndMaterial(materialData.subMaterials[j], texturesPath);
                    }
                }
            }

            //Cargar Meshes
            for (var i = 0; i < sceneData.meshesData.Length; i++)
            {
                var meshData = sceneData.meshesData[i];
                TgcMesh tgcMesh = null;

                //Crear malla original
                if (meshData.instanceType.Equals(TgcMeshData.ORIGINAL))
                {
                    //Crear mesh que no tiene Material, con un color simple
                    if (meshData.materialId == -1)
                    {
                        tgcMesh = crearMeshSoloColor(meshData);
                    }

                    //Para los que si tienen Material
                    else
                    {
                        //Crear MeshFormat que soporte LightMaps
                        if (meshData.lightmapEnabled)
                        {
                            tgcMesh = crearMeshDiffuseMapLightmap(sceneData, mediaPath, materialsArray, meshData);
                        }

                        //Formato de Mesh con Textura pero sin Lightmap
                        else
                        {
                            tgcMesh = crearMeshDiffuseMap(materialsArray, meshData);
                        }
                    }
                    //Fixloader
                    tgcMesh.AutoTransformEnable = true;
                }

                //Crear malla instancia
                else if (meshData.instanceType.Equals(TgcMeshData.INSTANCE))
                {
                    tgcMesh = crearMeshInstance(meshData, tgcScene.Meshes);
                    tgcMesh.AutoTransformEnable = true;
                }

                //Crear BoundingBox, aprovechar lo que viene del XML o crear uno por nuestra cuenta
                if (meshData.pMin != null && meshData.pMax != null)
                {
                    tgcMesh.BoundingBox = new TgcBoundingAxisAlignBox(
                        TGCVector3.Float3ArrayToTGCVector3(meshData.pMin),
                        TGCVector3.Float3ArrayToTGCVector3(meshData.pMax),
                        tgcMesh.Position,
                        tgcMesh.Scale
                        );
                }
                else
                {
                    tgcMesh.createBoundingBox();
                }

                //Cargar layer
                tgcMesh.Layer = meshData.layerName;

                //Cargar AlphaBlending
                tgcMesh.AlphaBlendEnable = meshData.alphaBlending;

                //agregar mesh a escena
                tgcMesh.Enabled = true;
                tgcScene.Meshes.Add(tgcMesh);

                //Cargar userProperties, si hay
                tgcMesh.UserProperties = meshData.userProperties;
            }

            //BoundingBox del escenario, utilizar el que viene del XML o crearlo nosotros
            if (sceneData.pMin != null && sceneData.pMax != null)
            {
                tgcScene.BoundingBox = new TgcBoundingAxisAlignBox(
                    new TGCVector3(sceneData.pMin[0], sceneData.pMin[1], sceneData.pMin[2]),
                    new TGCVector3(sceneData.pMax[0], sceneData.pMax[1], sceneData.pMax[2])
                    );
            }
            else
            {
                var boundingBoxes = new List<TgcBoundingAxisAlignBox>();
                foreach (var mesh in tgcScene.Meshes)
                {
                    boundingBoxes.Add(mesh.BoundingBox);
                }
                tgcScene.BoundingBox = TgcBoundingAxisAlignBox.computeFromBoundingBoxes(boundingBoxes);
            }

            //Cargar parte de PortalRendering, solo hay información
            if (sceneData.portalData != null)
            {
                var portalLoader = new TgcPortalRenderingLoader();
                tgcScene.PortalRendering = portalLoader.loadFromData(tgcScene, sceneData.portalData);
            }

            //Fixloader
            //Deshabilitamos el manejo automatico de Transformaciones de TgcMesh, para poder manipularlas en forma personalizada
            foreach (TgcMesh mesh in tgcScene.Meshes)
            {
                mesh.AutoTransformEnable = false;
            }

            return tgcScene;
        }

        /// <summary>
        ///     Crea un mesh con uno o varios DiffuseMap y un Lightmap
        /// </summary>
        /// <returns></returns>
        private TgcMesh crearMeshDiffuseMapLightmap(TgcSceneData sceneData, string mediaPath,
            TgcSceneLoaderMaterialAux[] materialsArray, TgcMeshData meshData)
        {
            //Crear Mesh
            var mesh = new Mesh(meshData.coordinatesIndices.Length / 3, meshData.coordinatesIndices.Length,
                MeshFlags.Managed, DiffuseMapAndLightmapVertexElements, D3DDevice.Instance.Device);

            //Cargar vertexBuffer
            using (var vb = mesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                for (var j = 0; j < meshData.coordinatesIndices.Length; j++)
                {
                    var v = new DiffuseMapAndLightmapVertex();

                    //vertices
                    var coordIdx = meshData.coordinatesIndices[j] * 3;
                    v.Position = new TGCVector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

                    //normals
                    //puede haber una normal compartida para cada vertice del mesh
                    if (meshData.verticesNormals.Length == meshData.verticesCoordinates.Length)
                    {
                        v.Normal = new TGCVector3(
                            meshData.verticesNormals[coordIdx],
                            meshData.verticesNormals[coordIdx + 1],
                            meshData.verticesNormals[coordIdx + 2]
                            );
                    }
                    //o una normal propia por cada vertice de cada triangulo (version mejorada del exporter)
                    else
                    {
                        var normalIdx = j * 3;
                        v.Normal = new TGCVector3(
                            meshData.verticesNormals[normalIdx],
                            meshData.verticesNormals[normalIdx + 1],
                            meshData.verticesNormals[normalIdx + 2]
                            );
                    }

                    //texture coordinates diffuseMap
                    var texCoordIdx = meshData.texCoordinatesIndices[j] * 2;
                    v.Tu0 = meshData.textureCoordinates[texCoordIdx];
                    v.Tv0 = meshData.textureCoordinates[texCoordIdx + 1];

                    //texture coordinates LightMap
                    var texCoordIdxLM = meshData.texCoordinatesIndicesLightMap[j] * 2;
                    v.Tu1 = meshData.textureCoordinatesLightMap[texCoordIdxLM];
                    v.Tv1 = meshData.textureCoordinatesLightMap[texCoordIdxLM + 1];

                    //color
                    var colorIdx = meshData.colorIndices[j];
                    v.Color = meshData.verticesColors[colorIdx];

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar IndexBuffer
            using (var ib = mesh.IndexBuffer)
            {
                var indices = new short[meshData.coordinatesIndices.Length];
                for (var j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Configurar Material y Textura para un solo SubSet
            var matAux = materialsArray[meshData.materialId];
            Material[] meshMaterials;
            TgcTexture[] meshTextures;
            if (matAux.subMaterials == null)
            {
                meshMaterials = new[] { matAux.materialId };
                meshTextures = new[]
                {TgcTexture.createTexture(D3DDevice.Instance.Device, matAux.textureFileName, matAux.texturePath)};
            }

            //Configurar Material y Textura para varios SubSet
            else
            {
                //Cargar attributeBuffer con los id de las texturas de cada triángulo
                var attributeBuffer = mesh.LockAttributeBufferArray(LockFlags.None);
                Array.Copy(meshData.materialsIds, attributeBuffer, attributeBuffer.Length);
                mesh.UnlockAttributeBuffer(attributeBuffer);

                //Cargar array de Materials y Texturas
                meshMaterials = new Material[matAux.subMaterials.Length];
                meshTextures = new TgcTexture[matAux.subMaterials.Length];
                for (var m = 0; m < matAux.subMaterials.Length; m++)
                {
                    meshMaterials[m] = matAux.subMaterials[m].materialId;
                    meshTextures[m] = TgcTexture.createTexture(D3DDevice.Instance.Device,
                        matAux.subMaterials[m].textureFileName,
                        matAux.subMaterials[m].texturePath);
                }
            }

            //Cargar lightMap
            var lightMap = TgcTexture.createTexture(D3DDevice.Instance.Device, meshData.lightmap,
                mediaPath + sceneData.lightmapsDir + "\\" + meshData.lightmap);

            //Crear mesh de TGC
            var tgcMesh = MeshFactory.createNewMesh(mesh, meshData.name, TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP);
            tgcMesh.Materials = meshMaterials;
            tgcMesh.DiffuseMaps = meshTextures;
            tgcMesh.LightMap = lightMap;
            return tgcMesh;
        }

        /// <summary>
        ///     Crea un mesh con uno o varios DiffuseMap
        /// </summary>
        /// <returns></returns>
        private TgcMesh crearMeshDiffuseMap(TgcSceneLoaderMaterialAux[] materialsArray, TgcMeshData meshData)
        {
            //Crear Mesh
            var mesh = new Mesh(meshData.coordinatesIndices.Length / 3, meshData.coordinatesIndices.Length,
                MeshFlags.Managed, DiffuseMapVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = mesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                for (var j = 0; j < meshData.coordinatesIndices.Length; j++)
                {
                    var v = new DiffuseMapVertex();

                    //vertices
                    var coordIdx = meshData.coordinatesIndices[j] * 3;
                    v.Position = new TGCVector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

                    //normals
                    //puede haber una normal compartida para cada vertice del mesh
                    if (meshData.verticesNormals.Length == meshData.verticesCoordinates.Length)
                    {
                        v.Normal = new TGCVector3(
                            meshData.verticesNormals[coordIdx],
                            meshData.verticesNormals[coordIdx + 1],
                            meshData.verticesNormals[coordIdx + 2]
                            );
                    }
                    //o una normal propia por cada vertice de cada triangulo (version mejorada del exporter)
                    else
                    {
                        var normalIdx = j * 3;
                        v.Normal = new TGCVector3(
                            meshData.verticesNormals[normalIdx],
                            meshData.verticesNormals[normalIdx + 1],
                            meshData.verticesNormals[normalIdx + 2]
                            );
                    }

                    //texture coordinates diffuseMap
                    var texCoordIdx = meshData.texCoordinatesIndices[j] * 2;
                    v.Tu = meshData.textureCoordinates[texCoordIdx];
                    v.Tv = meshData.textureCoordinates[texCoordIdx + 1];

                    //color
                    var colorIdx = meshData.colorIndices[j];
                    v.Color = meshData.verticesColors[colorIdx];

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (var ib = mesh.IndexBuffer)
            {
                var indices = new short[meshData.coordinatesIndices.Length];
                for (var j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Configurar Material y Textura para un solo SubSet
            var matAux = materialsArray[meshData.materialId];
            Material[] meshMaterials;
            TgcTexture[] meshTextures;
            if (matAux.subMaterials == null)
            {
                meshMaterials = new[] { matAux.materialId };
                meshTextures = new[]
                {TgcTexture.createTexture(D3DDevice.Instance.Device, matAux.textureFileName, matAux.texturePath)};
            }

            //Configurar Material y Textura para varios SubSet
            else
            {
                //Cargar attributeBuffer con los id de las texturas de cada triángulo
                var attributeBuffer = mesh.LockAttributeBufferArray(LockFlags.None);
                Array.Copy(meshData.materialsIds, attributeBuffer, attributeBuffer.Length);
                mesh.UnlockAttributeBuffer(attributeBuffer);

                //Cargar array de Materials y Texturas
                meshMaterials = new Material[matAux.subMaterials.Length];
                meshTextures = new TgcTexture[matAux.subMaterials.Length];
                for (var m = 0; m < matAux.subMaterials.Length; m++)
                {
                    meshMaterials[m] = matAux.subMaterials[m].materialId;
                    meshTextures[m] = TgcTexture.createTexture(D3DDevice.Instance.Device,
                        matAux.subMaterials[m].textureFileName,
                        matAux.subMaterials[m].texturePath);
                }
            }

            //Crear mesh de TGC
            var tgcMesh = MeshFactory.createNewMesh(mesh, meshData.name, TgcMesh.MeshRenderType.DIFFUSE_MAP);
            tgcMesh.Materials = meshMaterials;
            tgcMesh.DiffuseMaps = meshTextures;
            return tgcMesh;
        }

        /// <summary>
        ///     Crea un mesh sin texturas, solo con VertexColors
        /// </summary>
        /// <param name="meshData"></param>
        private TgcMesh crearMeshSoloColor(TgcMeshData meshData)
        {
            //Crear Mesh
            var mesh = new Mesh(meshData.coordinatesIndices.Length / 3, meshData.coordinatesIndices.Length,
                MeshFlags.Managed, VertexColorVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = mesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                for (var j = 0; j < meshData.coordinatesIndices.Length; j++)
                {
                    var v = new VertexColorVertex();

                    //vertices
                    var coordIdx = meshData.coordinatesIndices[j] * 3;
                    v.Position = new TGCVector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

                    //normals
                    //puede haber una normal compartida para cada vertice del mesh
                    if (meshData.verticesNormals.Length == meshData.verticesCoordinates.Length)
                    {
                        v.Normal = new TGCVector3(
                            meshData.verticesNormals[coordIdx],
                            meshData.verticesNormals[coordIdx + 1],
                            meshData.verticesNormals[coordIdx + 2]
                            );
                    }
                    //o una normal propia por cada vertice de cada triangulo (version mejorada del exporter)
                    else
                    {
                        var normalIdx = j * 3;
                        v.Normal = new TGCVector3(
                            meshData.verticesNormals[normalIdx],
                            meshData.verticesNormals[normalIdx + 1],
                            meshData.verticesNormals[normalIdx + 2]
                            );
                    }

                    //color
                    var colorIdx = meshData.colorIndices[j];
                    v.Color = meshData.verticesColors[colorIdx];

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar indexBuffer en forma plana
            using (var ib = mesh.IndexBuffer)
            {
                var indices = new short[meshData.coordinatesIndices.Length];
                for (var i = 0; i < indices.Length; i++)
                {
                    indices[i] = (short)i;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Crear mesh de TGC
            var tgcMesh = MeshFactory.createNewMesh(mesh, meshData.name, TgcMesh.MeshRenderType.VERTEX_COLOR);
            return tgcMesh;
        }

        /// <summary>
        ///     Crear una malla instancia de una original
        /// </summary>
        private TgcMesh crearMeshInstance(TgcMeshData meshData, List<TgcMesh> meshes)
        {
            var originalMesh = meshes[meshData.originalMesh];
            var translation = new TGCVector3(meshData.position[0], meshData.position[1], meshData.position[2]);
            var rotationQuat = new TGCQuaternion(meshData.rotation[0], meshData.rotation[1], meshData.rotation[2],
                meshData.rotation[3]);
            var rotation = quaternionToEuler(rotationQuat);
            var scale = new TGCVector3(meshData.scale[0], meshData.scale[1], meshData.scale[2]);

            var tgcMesh = new TgcMesh(meshData.name, originalMesh, translation, rotation, scale);
            return tgcMesh;
        }

        /// <summary>
        ///     Convierte un TGCQuaternion a rotación de Euler
        /// </summary>
        private TGCVector3 quaternionToEuler(TGCQuaternion quat)
        {
            //TODO revisar que esta conversion a Euler ande bien

            var mat = TGCMatrix.RotationTGCQuaternion(quat);
            var matrixGetRotation = TGCVector3.Empty;

            //gets the x axis rotation from the matrix
            matrixGetRotation.X = (float)Math.Asin(mat.M32);
            var cosX = (float)Math.Cos(matrixGetRotation.X);

            //checks for gimbal lock
            if (cosX < 0.005)
            {
                matrixGetRotation.Z = 0;
                matrixGetRotation.Y = Math.Sign(-mat.M21) * (float)Math.Acos(mat.M11);
            }
            //normal calculation
            else
            {
                matrixGetRotation.Z = Math.Sign(mat.M12) * (float)Math.Acos(mat.M22 / cosX);
                matrixGetRotation.Y = Math.Sign(mat.M31) * (float)Math.Acos(mat.M33 / cosX);
                //converts the rotations because the x axis rotation can't be bigger than 90 and -90
                if (Math.Sign(mat.M22) == -1 && matrixGetRotation.Z == 0)
                {
                    var pi = (float)Math.PI;
                    matrixGetRotation.Z += pi;
                    matrixGetRotation.Y += pi;
                }
            }

            return matrixGetRotation;
        }

        /// <summary>
        ///     Crea Material y Textura
        /// </summary>
        private TgcSceneLoaderMaterialAux createTextureAndMaterial(TgcMaterialData materialData, string texturesPath)
        {
            var matAux = new TgcSceneLoaderMaterialAux();

            //Crear material
            var material = new Material();
            matAux.materialId = material;
            material.AmbientColor = new ColorValue(
                materialData.ambientColor[0],
                materialData.ambientColor[1],
                materialData.ambientColor[2],
                materialData.ambientColor[3]);
            material.DiffuseColor = new ColorValue(
                materialData.diffuseColor[0],
                materialData.diffuseColor[1],
                materialData.diffuseColor[2],
                materialData.diffuseColor[3]);
            material.SpecularColor = new ColorValue(
                materialData.specularColor[0],
                materialData.specularColor[1],
                materialData.specularColor[2],
                materialData.specularColor[3]);

            //TODO ver que hacer con la opacity

            //guardar datos de textura
            if (materialData.fileName != null)
            {
                matAux.texturePath = texturesPath + materialData.fileName;
                matAux.textureFileName = materialData.fileName;
            }
            else
            {
                matAux.texturePath = null;
                matAux.textureFileName = null;
            }

            return matAux;
        }

        /// <summary>
        ///     Estructura auxiliar para cargar SubMaterials y Texturas
        /// </summary>
        private class TgcSceneLoaderMaterialAux
        {
            public Material materialId;
            public TgcSceneLoaderMaterialAux[] subMaterials;
            public string textureFileName;
            public string texturePath;
        }

        #region Mesh FVF

        /// <summary>
        ///     FVF para formato de malla VERTEX_COLOR
        /// </summary>
        public static readonly VertexElement[] VertexColorVertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0),
            new VertexElement(0, 24, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     Estructura de Vertice para formato de malla VERTEX_COLOR
        /// </summary>
        public struct VertexColorVertex
        {
            public TGCVector3 Position;
            public TGCVector3 Normal;
            public int Color;
        }

        /// <summary>
        ///     FVF para formato de malla DIFFUSE_MAP
        /// </summary>
        public static readonly VertexElement[] DiffuseMapVertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0),
            new VertexElement(0, 24, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            new VertexElement(0, 28, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     Estructura de Vertice para formato de malla DIFFUSE_MAP
        /// </summary>
        public struct DiffuseMapVertex
        {
            public TGCVector3 Position;
            public TGCVector3 Normal;
            public int Color;
            public float Tu;
            public float Tv;
        }

        /// <summary>
        ///     FVF para formato de malla DIFFUSE_MAP_AND_LIGHTMAP
        /// </summary>
        public static readonly VertexElement[] DiffuseMapAndLightmapVertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0),
            new VertexElement(0, 24, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            new VertexElement(0, 28, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            new VertexElement(0, 36, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 1),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     Estructura de Vertice para formato de malla DIFFUSE_MAP_AND_LIGHTMAP
        /// </summary>
        public struct DiffuseMapAndLightmapVertex
        {
            public TGCVector3 Position;
            public TGCVector3 Normal;
            public int Color;
            public float Tu0;
            public float Tv0;
            public float Tu1;
            public float Tv1;
        }

        #endregion Mesh FVF
    }
}