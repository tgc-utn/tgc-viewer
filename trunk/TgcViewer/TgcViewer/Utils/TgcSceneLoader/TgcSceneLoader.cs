using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.IO;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using Ionic.Zip;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.PortalRendering;

namespace TgcViewer.Utils.TgcSceneLoader
{
    /// <summary>
    /// Herramienta para cargar un archivo de escena XML con formato de TGC (tgcScene)
    /// </summary>
    public class TgcSceneLoader
    {
        Device device;

        IMeshFactory meshFactory;
        /// <summary>
        /// Factory utilizado para crear una instancia de TgcMesh.
        /// Por default se utiliza la clase DefaultMeshFactory.
        /// </summary>
        public IMeshFactory MeshFactory
        {
            get { return meshFactory; }
            set { meshFactory = value; }
        }

        /// <summary>
        /// Crear un nuevo Loader
        /// </summary>
        public TgcSceneLoader()
        {
            this.device = GuiController.Instance.D3dDevice;
            this.meshFactory = new DefaultMeshFactory();
        }

        /// <summary>
        /// Carga una escena a partir de un archivo
        /// </summary>
        /// <param name="filePath">Ubicacion del archivo XML</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar los recursos de escena (Texturas, LightMaps, etc.)</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromFile(string filePath, string mediaPath)
        {
            string xmlString = File.ReadAllText(filePath);
            return loadSceneFromString(xmlString, mediaPath);
        }

        /// <summary>
        /// Carga una escena a partir de un archivo. 
        /// Como carpeta de Media utiliza la misma carpeta en la que se encuentra el archivo XML de la malla
        /// </summary>
        /// <param name="filePath">Ubicacion del archivo XML</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromFile(string filePath)
        {
            string mediaPath = filePath.Substring(0, filePath.LastIndexOf('\\') + 1);
            return loadSceneFromFile(filePath, mediaPath);
        }

        /// <summary>
        /// Carga una escena a partir de un archivo en formato .ZIP.
        /// Se asume que dentro del ZIP se encuentra el archivo XML de la escena y todas las texturas necesarias.
        /// </summary>
        /// <param name="sceneFileName">Nombre del archivo XML que tiene la información de la escena</param>
        /// <param name="zipFilePath">Path del archivo ZIP que contiene la escena.</param>
        /// <param name="extractDir">Path del directorio en donde se va a extraer el ZIP</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromZipFile(string sceneFileName, string zipFilePath, string extractDir)
        {
            //extraer archivo pisando archivos existentes
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                foreach (ZipEntry e in zip)
                {
                    e.Extract(extractDir, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            return loadSceneFromFile(extractDir + sceneFileName, extractDir);
        }

        /// <summary>
        /// Carga la escena a partir del string del XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar los recursos de escena (Texturas, LightMaps, etc.)</param>
        /// <returns>Escena cargada</returns>
        public TgcScene loadSceneFromString(string xmlString, string mediaPath)
        {
            TgcSceneParser parser = new TgcSceneParser();
            TgcSceneData sceneData = parser.parseSceneFromString(xmlString);
            return loadScene(sceneData, mediaPath);
        }

        /// <summary>
        /// Carga la escena a partir de un objeto TgcSceneData ya parseado
        /// </summary>
        /// <param name="sceneData">Objeto con datos de la escena ya parseados</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar los recursos de escena (Texturas, LightMaps, etc.)</param>
        /// <returns></returns>
        public TgcScene loadScene(TgcSceneData sceneData, string mediaPath)
        {
            TgcScene tgcScene = new TgcScene(sceneData.name, null);

            //Cargar Texturas
            TgcSceneLoaderMaterialAux[] materialsArray = new TgcSceneLoaderMaterialAux[sceneData.materialsData.Length];
            for (int i = 0; i < sceneData.materialsData.Length; i++)
            {
                TgcMaterialData materialData = sceneData.materialsData[i];
                string texturesPath = mediaPath + sceneData.texturesDir + "\\";

                //Crear StandardMaterial
                if (materialData.type.Equals(TgcMaterialData.StandardMaterial))
                {
                    materialsArray[i] = createTextureAndMaterial(materialData, texturesPath);
                }

                //Crear MultiMaterial
                else if (materialData.type.Equals(TgcMaterialData.MultiMaterial))
                {
                    TgcSceneLoaderMaterialAux matAux = new TgcSceneLoaderMaterialAux();
                    materialsArray[i] = matAux;
                    matAux.subMaterials = new TgcSceneLoaderMaterialAux[materialData.subMaterials.Length];
                    for (int j = 0; j < materialData.subMaterials.Length; j++)
                    {
                        matAux.subMaterials[j] = createTextureAndMaterial(materialData.subMaterials[j], texturesPath);
                    }
                }
            }


            //Cargar Meshes
            for (int i = 0; i < sceneData.meshesData.Length; i++)
            {
                TgcMeshData meshData = sceneData.meshesData[i];
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
                }

                //Crear malla instancia
                else if (meshData.instanceType.Equals(TgcMeshData.INSTANCE))
                {
                    tgcMesh = crearMeshInstance(meshData, tgcScene.Meshes);
                }




                //Crear BoundingBox, aprovechar lo que viene del XML o crear uno por nuestra cuenta
                if (meshData.pMin != null && meshData.pMax != null)
                {
                    tgcMesh.BoundingBox = new TgcBoundingBox(
                        TgcParserUtils.float3ArrayToVector3(meshData.pMin),
                        TgcParserUtils.float3ArrayToVector3(meshData.pMax),
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
                tgcScene.BoundingBox = new TgcBoundingBox(
                    new Vector3(sceneData.pMin[0], sceneData.pMin[1], sceneData.pMin[2]),
                    new Vector3(sceneData.pMax[0], sceneData.pMax[1], sceneData.pMax[2])
                    );
            }
            else
            {
                List<TgcBoundingBox> boundingBoxes = new List<TgcBoundingBox>();
                foreach (TgcMesh mesh in tgcScene.Meshes)
                {
                    boundingBoxes.Add(mesh.BoundingBox);
                }
                tgcScene.BoundingBox = TgcBoundingBox.computeFromBoundingBoxes(boundingBoxes);
            }
            

            //Cargar parte de PortalRendering, solo hay información
            if (sceneData.portalData != null)
            {
                TgcPortalRenderingLoader portalLoader = new TgcPortalRenderingLoader();
                tgcScene.PortalRendering = portalLoader.loadFromData(tgcScene, sceneData.portalData);
            }


            return tgcScene;
        }

        

        /// <summary>
        /// Crea un mesh con uno o varios DiffuseMap y un Lightmap
        /// </summary>
        /// <returns></returns>
        private TgcMesh crearMeshDiffuseMapLightmap(TgcSceneData sceneData, string mediaPath, TgcSceneLoaderMaterialAux[] materialsArray, TgcMeshData meshData)
        {
            //Crear Mesh
            Mesh mesh = new Mesh(meshData.coordinatesIndices.Length / 3, meshData.coordinatesIndices.Length, MeshFlags.Managed, DiffuseMapAndLightmapVertexElements, device);

            //Cargar vertexBuffer
            using (VertexBuffer vb = mesh.VertexBuffer)
            {
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                for (int j = 0; j < meshData.coordinatesIndices.Length; j++)
                {
                    DiffuseMapAndLightmapVertex v = new DiffuseMapAndLightmapVertex();

                    //vertices
                    int coordIdx = meshData.coordinatesIndices[j] * 3;
                    v.Position = new Vector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

                    //normals
                    //puede haber una normal compartida para cada vertice del mesh
                    if (meshData.verticesNormals.Length == meshData.verticesCoordinates.Length)
                    {
                        v.Normal = new Vector3(
                            meshData.verticesNormals[coordIdx],
                            meshData.verticesNormals[coordIdx + 1],
                            meshData.verticesNormals[coordIdx + 2]
                            );
                    }
                    //o una normal propia por cada vertice de cada triangulo (version mejorada del exporter)
                    else
                    {
                        int normalIdx = j * 3;
                        v.Normal = new Vector3(
                            meshData.verticesNormals[normalIdx],
                            meshData.verticesNormals[normalIdx + 1],
                            meshData.verticesNormals[normalIdx + 2]
                            );
                    }

                    //texture coordinates diffuseMap
                    int texCoordIdx = meshData.texCoordinatesIndices[j] * 2;
                    v.Tu0 = meshData.textureCoordinates[texCoordIdx];
                    v.Tv0 = meshData.textureCoordinates[texCoordIdx + 1];

                    //texture coordinates LightMap
                    int texCoordIdxLM = meshData.texCoordinatesIndicesLightMap[j] * 2;
                    v.Tu1 = meshData.textureCoordinatesLightMap[texCoordIdxLM];
                    v.Tv1 = meshData.textureCoordinatesLightMap[texCoordIdxLM + 1];

                    //color
                    int colorIdx = meshData.colorIndices[j];
                    v.Color = meshData.verticesColors[colorIdx];

                    data.Write(v);
                }
                vb.Unlock();

            }

            //Cargar IndexBuffer
            using (IndexBuffer ib = mesh.IndexBuffer)
            {
                short[] indices = new short[meshData.coordinatesIndices.Length];
                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Configurar Material y Textura para un solo SubSet
            TgcSceneLoaderMaterialAux matAux = materialsArray[meshData.materialId];
            Material[] meshMaterials;
            TgcTexture[] meshTextures;
            if (matAux.subMaterials == null)
            {
                meshMaterials = new Material[] { matAux.materialId };
                meshTextures = new TgcTexture[] { TgcTexture.createTexture(device, matAux.textureFileName, matAux.texturePath) };
            }

            //Configurar Material y Textura para varios SubSet
            else
            {
                //Cargar attributeBuffer con los id de las texturas de cada triángulo
                int[] attributeBuffer = mesh.LockAttributeBufferArray(LockFlags.None);
                Array.Copy(meshData.materialsIds, attributeBuffer, attributeBuffer.Length);
                mesh.UnlockAttributeBuffer(attributeBuffer);

                //Cargar array de Materials y Texturas
                meshMaterials = new Material[matAux.subMaterials.Length];
                meshTextures = new TgcTexture[matAux.subMaterials.Length];
                for (int m = 0; m < matAux.subMaterials.Length; m++)
                {
                    meshMaterials[m] = matAux.subMaterials[m].materialId;
                    meshTextures[m] = TgcTexture.createTexture(device, matAux.subMaterials[m].textureFileName, matAux.subMaterials[m].texturePath);
                }
            }

            //Cargar lightMap
            TgcTexture lightMap = TgcTexture.createTexture(device, meshData.lightmap, mediaPath + sceneData.lightmapsDir + "\\" + meshData.lightmap);

            //Crear mesh de TGC
            TgcMesh tgcMesh = meshFactory.createNewMesh(mesh, meshData.name, TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP);
            tgcMesh.Materials = meshMaterials;
            tgcMesh.DiffuseMaps = meshTextures;
            tgcMesh.LightMap = lightMap;
            return tgcMesh;
        }


        /// <summary>
        /// Crea un mesh con uno o varios DiffuseMap
        /// </summary>
        /// <returns></returns>
        private TgcMesh crearMeshDiffuseMap(TgcSceneLoaderMaterialAux[] materialsArray, TgcMeshData meshData)
        {
            //Crear Mesh
            Mesh mesh = new Mesh(meshData.coordinatesIndices.Length / 3, meshData.coordinatesIndices.Length, MeshFlags.Managed, DiffuseMapVertexElements, device);
            
            //Cargar VertexBuffer
            using (VertexBuffer vb = mesh.VertexBuffer)
            {
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                for (int j = 0; j < meshData.coordinatesIndices.Length; j++)
                {
                    DiffuseMapVertex v = new DiffuseMapVertex();

                    //vertices
                    int coordIdx = meshData.coordinatesIndices[j] * 3;
                    v.Position = new Vector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

                    //normals
                    //puede haber una normal compartida para cada vertice del mesh
                    if (meshData.verticesNormals.Length == meshData.verticesCoordinates.Length)
                    {
                        v.Normal = new Vector3(
                            meshData.verticesNormals[coordIdx],
                            meshData.verticesNormals[coordIdx + 1],
                            meshData.verticesNormals[coordIdx + 2]
                            );
                    }
                    //o una normal propia por cada vertice de cada triangulo (version mejorada del exporter)
                    else
                    {
                        int normalIdx = j * 3;
                        v.Normal = new Vector3(
                            meshData.verticesNormals[normalIdx],
                            meshData.verticesNormals[normalIdx + 1],
                            meshData.verticesNormals[normalIdx + 2]
                            );
                    }
                    

                    //texture coordinates diffuseMap
                    int texCoordIdx = meshData.texCoordinatesIndices[j] * 2;
                    v.Tu = meshData.textureCoordinates[texCoordIdx];
                    v.Tv = meshData.textureCoordinates[texCoordIdx + 1];

                    //color
                    int colorIdx = meshData.colorIndices[j];
                    v.Color = meshData.verticesColors[colorIdx];

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (IndexBuffer ib = mesh.IndexBuffer)
            {
                short[] indices = new short[meshData.coordinatesIndices.Length];
                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Configurar Material y Textura para un solo SubSet
            TgcSceneLoaderMaterialAux matAux = materialsArray[meshData.materialId];
            Material[] meshMaterials;
            TgcTexture[] meshTextures;
            if (matAux.subMaterials == null)
            {
                meshMaterials = new Material[] { matAux.materialId };
                meshTextures = new TgcTexture[] { TgcTexture.createTexture(device, matAux.textureFileName, matAux.texturePath) };
            }

            //Configurar Material y Textura para varios SubSet
            else
            {
                //Cargar attributeBuffer con los id de las texturas de cada triángulo
                int[] attributeBuffer = mesh.LockAttributeBufferArray(LockFlags.None);
                Array.Copy(meshData.materialsIds, attributeBuffer, attributeBuffer.Length);
                mesh.UnlockAttributeBuffer(attributeBuffer);

                //Cargar array de Materials y Texturas
                meshMaterials = new Material[matAux.subMaterials.Length];
                meshTextures = new TgcTexture[matAux.subMaterials.Length];
                for (int m = 0; m < matAux.subMaterials.Length; m++)
                {
                    meshMaterials[m] = matAux.subMaterials[m].materialId;
                    meshTextures[m] = TgcTexture.createTexture(device, matAux.subMaterials[m].textureFileName, matAux.subMaterials[m].texturePath);
                }
            }

            //Crear mesh de TGC
            TgcMesh tgcMesh = meshFactory.createNewMesh(mesh, meshData.name, TgcMesh.MeshRenderType.DIFFUSE_MAP);
            tgcMesh.Materials = meshMaterials;
            tgcMesh.DiffuseMaps = meshTextures;
            return tgcMesh;
        }


        /// <summary>
        /// Crea un mesh sin texturas, solo con VertexColors
        /// </summary>
        /// <param name="meshData"></param>
        private TgcMesh crearMeshSoloColor(TgcMeshData meshData)
        {
            //Crear Mesh
            Mesh mesh = new Mesh(meshData.coordinatesIndices.Length / 3, meshData.coordinatesIndices.Length, MeshFlags.Managed, VertexColorVertexElements, device);

            //Cargar VertexBuffer
            using (VertexBuffer vb = mesh.VertexBuffer)
            {
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                for (int j = 0; j < meshData.coordinatesIndices.Length; j++)
                {
                    VertexColorVertex v = new VertexColorVertex();

                    //vertices
                    int coordIdx = meshData.coordinatesIndices[j] * 3;
                    v.Position = new Vector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

                    //normals
                    //puede haber una normal compartida para cada vertice del mesh
                    if (meshData.verticesNormals.Length == meshData.verticesCoordinates.Length)
                    {
                        v.Normal = new Vector3(
                            meshData.verticesNormals[coordIdx],
                            meshData.verticesNormals[coordIdx + 1],
                            meshData.verticesNormals[coordIdx + 2]
                            );
                    }
                    //o una normal propia por cada vertice de cada triangulo (version mejorada del exporter)
                    else
                    {
                        int normalIdx = j * 3;
                        v.Normal = new Vector3(
                            meshData.verticesNormals[normalIdx],
                            meshData.verticesNormals[normalIdx + 1],
                            meshData.verticesNormals[normalIdx + 2]
                            );
                    }

                    //color
                    int colorIdx = meshData.colorIndices[j];
                    v.Color = meshData.verticesColors[colorIdx];

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar indexBuffer en forma plana
            using (IndexBuffer ib = mesh.IndexBuffer)
            {
                short[] indices = new short[meshData.coordinatesIndices.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = (short)i;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }


            //Crear mesh de TGC
            TgcMesh tgcMesh = meshFactory.createNewMesh(mesh, meshData.name, TgcMesh.MeshRenderType.VERTEX_COLOR);
            return tgcMesh;
        }


        /// <summary>
        /// Crear una malla instancia de una original
        /// </summary>
        private TgcMesh crearMeshInstance(TgcMeshData meshData, List<TgcMesh> meshes)
        {
            TgcMesh originalMesh = meshes[meshData.originalMesh];
            Vector3 translation = new Vector3(meshData.position[0], meshData.position[1], meshData.position[2]);
            Quaternion rotationQuat = new Quaternion(meshData.rotation[0], meshData.rotation[1], meshData.rotation[2], meshData.rotation[3]);
            Vector3 rotation = quaternionToEuler(rotationQuat);
            Vector3 scale = new Vector3(meshData.scale[0], meshData.scale[1], meshData.scale[2]);

            TgcMesh tgcMesh = new TgcMesh(meshData.name, originalMesh, translation, rotation, scale);
            return tgcMesh;
        }

        /// <summary>
        /// Convierte un Quaternion a rotación de Euler
        /// </summary>
        private Vector3 quaternionToEuler(Quaternion quat)
        {
            //TODO revisar que esta conversion a Euler ande bien

            Matrix mat = Matrix.RotationQuaternion(quat);
            Vector3 matrixGetRotation = new Vector3();

            //gets the x axis rotation from the matrix
            matrixGetRotation.X = (float)Math.Asin(mat.M32);
            float cosX = (float)Math.Cos(matrixGetRotation.X);

            //checks for gimbal lock
            if (cosX < 0.005)
            {
                matrixGetRotation.Z = 0;
                matrixGetRotation.Y = (float)Math.Sign(-mat.M21) * (float)Math.Acos(mat.M11);
            }
            //normal calculation
            else
            {
                matrixGetRotation.Z = (float)Math.Sign(mat.M12) * (float)Math.Acos(mat.M22 / cosX);
                matrixGetRotation.Y = (float)Math.Sign(mat.M31) * (float)Math.Acos(mat.M33 / cosX);
                //converts the rotations because the x axis rotation can't be bigger than 90 and -90
                if (Math.Sign(mat.M22) == -1 && matrixGetRotation.Z == 0)
                {
                    float pi = (float)Math.PI;
                    matrixGetRotation.Z += pi;
                    matrixGetRotation.Y += pi;
                }
            }

            return matrixGetRotation;
        }

        /// <summary>
        /// Crea Material y Textura
        /// </summary>
        private TgcSceneLoaderMaterialAux createTextureAndMaterial(TgcMaterialData materialData, string texturesPath)
        {
            TgcSceneLoaderMaterialAux matAux = new TgcSceneLoaderMaterialAux();

            //Crear material
            Material material = new Material();
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


        #region Mesh FVF

        /// <summary>
        /// FVF para formato de malla VERTEX_COLOR
        /// </summary>
        public static readonly VertexElement[] VertexColorVertexElements = new VertexElement[]
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
        /// Estructura de Vertice para formato de malla VERTEX_COLOR
        /// </summary>
        public struct VertexColorVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;
        }


        /// <summary>
        /// FVF para formato de malla DIFFUSE_MAP
        /// </summary>
        public static readonly VertexElement[] DiffuseMapVertexElements = new VertexElement[]
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
        /// Estructura de Vertice para formato de malla DIFFUSE_MAP
        /// </summary>
        public struct DiffuseMapVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;
            public float Tu;
            public float Tv;
        }

        /// <summary>
        /// FVF para formato de malla DIFFUSE_MAP_AND_LIGHTMAP
        /// </summary>
        public static readonly VertexElement[] DiffuseMapAndLightmapVertexElements = new VertexElement[]
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
        /// Estructura de Vertice para formato de malla DIFFUSE_MAP_AND_LIGHTMAP
        /// </summary>
        public struct DiffuseMapAndLightmapVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;
            public float Tu0;
            public float Tv0;
            public float Tu1;
            public float Tv1;
        }


        #endregion


        /// <summary>
        /// Estructura auxiliar para cargar SubMaterials y Texturas
        /// </summary>
        class TgcSceneLoaderMaterialAux
        {
            public Material materialId;
            public string texturePath;
            public string textureFileName;
            public TgcSceneLoaderMaterialAux[] subMaterials;
        }


        #region MeshFactory

        /// <summary>
        /// Factory para permitir crear una instancia especifica de la clase TgcMesh
        /// </summary>
        public interface IMeshFactory
        {
            /// <summary>
            /// Crear una nueva instancia de la clase TgcMesh o derivados
            /// </summary>
            /// <param name="d3dMesh">Mesh de Direct3D</param>
            /// <param name="meshName">Nombre de la malla</param>
            /// <param name="renderType">Tipo de renderizado de la malla</param>
            /// <param name="meshData">Datos de la malla</param>
            /// <returns>Instancia de TgcMesh creada</returns>
            TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType);

            /// <summary>
            /// Crear una nueva malla que es una instancia de otra malla original.
            /// Crear una instancia de la clase TgcMesh o derivados
            /// </summary>
            /// <param name="name">Nombre de la malla</param>
            /// <param name="parentInstance">Malla original desde la cual basarse</param>
            /// <param name="translation">Traslación respecto de la malla original</param>
            /// <param name="rotation">Rotación respecto de la malla original</param>
            /// <param name="scale">Escala respecto de la malla original</param>
            /// <returns>Instancia de TgcMesh creada</returns>
            TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale);
        }

        /// <summary>
        /// Factory default que crea una instancia de la clase TgcMesh
        /// </summary>
        public class DefaultMeshFactory : IMeshFactory
        {
            public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
            {
                return new TgcMesh(d3dMesh, meshName, renderType);
            }

            public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
            {
                return new TgcMesh(meshName, originalMesh, translation, rotation, scale);
            }
        }

        #endregion

    }
}
