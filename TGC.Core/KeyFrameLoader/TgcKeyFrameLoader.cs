using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using TGC.Core.Direct3D;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;

namespace TGC.Core.KeyFrameLoader
{
    /// <summary>
    ///     Herramienta para cargar una Malla con animacion por KeyFrame, segun formato TGC
    /// </summary>
    public class TgcKeyFrameLoader
    {
        private readonly Dictionary<string, TgcTexture> texturesDict;

        public TgcKeyFrameLoader()
        {
            texturesDict = new Dictionary<string, TgcTexture>();
        }

        /// <summary>
        ///     Carga un modelo a partir de un archivo
        /// </summary>
        /// <param name="filePath">Ubicacion del archivo XML</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar las Texturas</param>
        /// <returns>Modelo cargado</returns>
        public TgcKeyFrameMesh loadMeshFromFile(string filePath, string mediaPath)
        {
            try
            {
                var xmlString = File.ReadAllText(filePath);
                return loadMeshFromString(xmlString, mediaPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar mesh desde archivo: " + filePath, ex);
            }
        }

        /// <summary>
        ///     Carga un modelo a partir de un archivo.
        ///     Se elige el directorio de texturas y recursos en base al directorio en el cual se encuntra el archivo del modelo.
        /// </summary>
        /// <param name="filePath">Ubicacion del archivo XML</param>
        /// <returns>Modelo cargado</returns>
        public TgcKeyFrameMesh loadMeshFromFile(string filePath)
        {
            var mediaPath = new FileInfo(filePath).DirectoryName + "\\";
            return loadMeshFromFile(filePath, mediaPath);
        }

        /// <summary>
        ///     Carga un modelo y un conjunto de animaciones a partir de varios archivos
        /// </summary>
        /// <param name="meshFilePath">Ubicacion del archivo XML del modelo</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar las Texturas</param>
        /// <param name="animationsFilePath">Array con ubicaciones de los archivos XML de cada animación</param>
        /// <returns>Modelo cargado con sus animaciones</returns>
        public TgcKeyFrameMesh loadMeshAndAnimationsFromFile(string meshFilePath, string mediaPath,
            string[] animationsFilePath)
        {
            var mesh = loadMeshFromFile(meshFilePath, mediaPath);
            foreach (var animPath in animationsFilePath)
            {
                loadAnimationFromFile(mesh, animPath);
            }
            return mesh;
        }

        /// <summary>
        ///     Carga un modelo y un conjunto de animaciones a partir de varios archivos.
        ///     Se elige el directorio de texturas y recursos en base al directorio en el cual se encuntra el archivo del modelo.
        /// </summary>
        /// <param name="meshFilePath">Ubicacion del archivo XML del modelo</param>
        /// <param name="animationsFilePath">Array con ubicaciones de los archivos XML de cada animación</param>
        /// <returns>Modelo cargado con sus animaciones</returns>
        public TgcKeyFrameMesh loadMeshAndAnimationsFromFile(string meshFilePath, string[] animationsFilePath)
        {
            var mediaPath = new FileInfo(meshFilePath).DirectoryName + "\\";
            return loadMeshAndAnimationsFromFile(meshFilePath, mediaPath, animationsFilePath);
        }

        /// <summary>
        ///     Carga un modelo a partir del string del XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar las Texturas</param>
        /// <returns>Modelo cargado</returns>
        public TgcKeyFrameMesh loadMeshFromString(string xmlString, string mediaPath)
        {
            var parser = new TgcKeyFrameParser();
            var meshData = parser.parseMeshFromString(xmlString);
            return loadMesh(meshData, mediaPath);
        }

        /// <summary>
        ///     Carga una animación a un modelo ya cargado, en base a un archivo
        ///     La animación se agrega al modelo.
        /// </summary>
        /// <param name="mesh">Modelo ya cargado</param>
        /// <param name="filePath">Ubicacion del archivo XML de la animación</param>
        public TgcKeyFrameAnimation loadAnimationFromFile(TgcKeyFrameMesh mesh, string filePath)
        {
            try
            {
                var xmlString = File.ReadAllText(filePath);
                return loadAnimationFromString(mesh, xmlString);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar animacion desde archivo: " + filePath, ex);
            }
        }

        /// <summary>
        ///     Carga una animación a un modelo ya cargado, a partir del string del XML.
        ///     La animación se agrega al modelo.
        /// </summary>
        /// <param name="mesh">Modelo ya cargado</param>
        /// <param name="xmlString">contenido del XML</param>
        public TgcKeyFrameAnimation loadAnimationFromString(TgcKeyFrameMesh mesh, string xmlString)
        {
            var parser = new TgcKeyFrameParser();
            var animationData = parser.parseAnimationFromString(xmlString);
            return loadAnimation(mesh, animationData);
        }

        /// <summary>
        ///     Carga una animación a un modelo ya cargado, a partir de un objeto TgcKeyFrameAnimationData ya parseado
        ///     La animación se agrega al modelo.
        /// </summary>
        /// <param name="mesh">Modelo ya cargado</param>
        /// <param name="animationData">Objeto de animacion con datos ya cargados</param>
        public TgcKeyFrameAnimation loadAnimation(TgcKeyFrameMesh mesh, TgcKeyFrameAnimationData animationData)
        {
            //BoundingBox de la animación, aprovechar lo que viene en el XML o utilizar el de la malla estática
            TgcBoundingBox boundingBox = null;
            if (animationData.pMin != null && animationData.pMax != null)
            {
                boundingBox = new TgcBoundingBox(
                    TgcParserUtils.float3ArrayToVector3(animationData.pMin),
                    TgcParserUtils.float3ArrayToVector3(animationData.pMax));
            }
            else
            {
                boundingBox = mesh.BoundingBox;
            }

            var animation = new TgcKeyFrameAnimation(animationData, boundingBox);
            mesh.Animations.Add(animationData.name, animation);
            return animation;
        }

        /// <summary>
        ///     Carga un Modelo a partir de un objeto TgcKeyFrameMeshData ya parseado
        /// </summary>
        /// <param name="meshData">Objeto con datos ya parseados</param>
        /// <param name="mediaPath">Path a partir del cual hay que buscar las Texturas</param>
        /// <returns>Modelo cargado</returns>
        public TgcKeyFrameMesh loadMesh(TgcKeyFrameMeshData meshData, string mediaPath)
        {
            //Cargar Texturas
            var materialsArray = new TgcKeyFrameLoaderMaterialAux[meshData.materialsData.Length];
            for (var i = 0; i < meshData.materialsData.Length; i++)
            {
                var materialData = meshData.materialsData[i];
                var texturesPath = mediaPath + meshData.texturesDir + "\\";

                //Crear StandardMaterial
                if (materialData.type.Equals(TgcMaterialData.StandardMaterial))
                {
                    materialsArray[i] = createTextureAndMaterial(materialData, texturesPath);
                }

                //Crear MultiMaterial
                else if (materialData.type.Equals(TgcMaterialData.MultiMaterial))
                {
                    var matAux = new TgcKeyFrameLoaderMaterialAux();
                    materialsArray[i] = matAux;
                    matAux.subMaterials = new TgcKeyFrameLoaderMaterialAux[materialData.subMaterials.Length];
                    for (var j = 0; j < materialData.subMaterials.Length; j++)
                    {
                        matAux.subMaterials[j] = createTextureAndMaterial(materialData.subMaterials[j], texturesPath);
                    }
                }
            }

            //Crear Mesh
            TgcKeyFrameMesh tgcMesh = null;

            //Crear mesh que no tiene Material, con un color simple
            if (meshData.materialId == -1)
            {
                tgcMesh = crearMeshSoloColor(meshData);
            }

            //Crear mesh con DiffuseMap
            else
            {
                tgcMesh = crearMeshDiffuseMap(materialsArray, meshData);
            }

            //Crear BoundingBox, aprovechar lo que viene del XML o crear uno por nuestra cuenta
            if (meshData.pMin != null && meshData.pMax != null)
            {
                tgcMesh.BoundingBox = new TgcBoundingBox(
                    TgcParserUtils.float3ArrayToVector3(meshData.pMin),
                    TgcParserUtils.float3ArrayToVector3(meshData.pMax)
                    );
            }
            else
            {
                tgcMesh.createBoundingBox();
            }

            tgcMesh.Enabled = true;
            return tgcMesh;
        }

        /// <summary>
        ///     Crea un mesh con uno o varios DiffuseMap
        /// </summary>
        /// <returns></returns>
        private TgcKeyFrameMesh crearMeshDiffuseMap(TgcKeyFrameLoaderMaterialAux[] materialsArray,
            TgcKeyFrameMeshData meshData)
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
                    v.Position = new Vector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

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
                meshTextures = new[] { matAux.texture };
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
                    meshTextures[m] = matAux.subMaterials[m].texture;
                }
            }

            //Cargar datos que originales que tienen que mantenerse
            var originalData = new OriginalData();
            originalData.coordinatesIndices = meshData.coordinatesIndices;
            originalData.colorIndices = meshData.colorIndices;
            originalData.verticesColors = meshData.verticesColors;
            originalData.texCoordinatesIndices = meshData.texCoordinatesIndices;
            originalData.textureCoordinates = meshData.textureCoordinates;

            //Crear mesh de TGC
            var tgcMesh = new TgcKeyFrameMesh(mesh, meshData.name, TgcKeyFrameMesh.MeshRenderType.DIFFUSE_MAP,
                originalData);
            tgcMesh.Materials = meshMaterials;
            tgcMesh.DiffuseMaps = meshTextures;
            return tgcMesh;
        }

        /// <summary>
        ///     Crea un mesh sin texturas, solo con VertexColors
        /// </summary>
        /// <param name="meshData"></param>
        private TgcKeyFrameMesh crearMeshSoloColor(TgcKeyFrameMeshData meshData)
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
                    v.Position = new Vector3(
                        meshData.verticesCoordinates[coordIdx],
                        meshData.verticesCoordinates[coordIdx + 1],
                        meshData.verticesCoordinates[coordIdx + 2]
                        );

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

            //Cargar datos que originales que tienen que mantenerse
            var originalData = new OriginalData();
            originalData.coordinatesIndices = meshData.coordinatesIndices;
            originalData.colorIndices = meshData.colorIndices;
            originalData.verticesColors = meshData.verticesColors;
            originalData.texCoordinatesIndices = null;
            originalData.textureCoordinates = null;

            //Crear mesh de TGC
            var tgcMesh = new TgcKeyFrameMesh(mesh, meshData.name, TgcKeyFrameMesh.MeshRenderType.VERTEX_COLOR,
                originalData);
            return tgcMesh;
        }

        /// <summary>
        ///     Crea Material y Textura
        /// </summary>
        private TgcKeyFrameLoaderMaterialAux createTextureAndMaterial(TgcMaterialData materialData, string texturesPath)
        {
            var matAux = new TgcKeyFrameLoaderMaterialAux();

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

            //crear textura
            texturesDict.Clear();
            if (materialData.fileName != null)
            {
                //revisar que esa imagen no se haya cargado previamente
                TgcTexture texture;
                if (texturesDict.ContainsKey(materialData.fileName))
                {
                    texture = texturesDict[materialData.fileName];
                }
                else
                {
                    texture = TgcTexture.createTexture(D3DDevice.Instance.Device, materialData.fileName,
                        texturesPath + "\\" + materialData.fileName);
                    texturesDict[materialData.fileName] = texture;
                    //TODO usar para algo el OFFSET y el TILING
                }
                matAux.texture = texture;
            }
            else
            {
                matAux.texture = null;
            }

            return matAux;
        }

        /// <summary>
        ///     Estructura auxiliar para cargar SubMaterials y Texturas
        /// </summary>
        private class TgcKeyFrameLoaderMaterialAux
        {
            public Material materialId;
            public TgcKeyFrameLoaderMaterialAux[] subMaterials;
            public TgcTexture texture;
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
            new VertexElement(0, 12, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     Estructura de Vertice para formato de malla VERTEX_COLOR
        /// </summary>
        public struct VertexColorVertex
        {
            public Vector3 Position;
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
            new VertexElement(0, 12, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            new VertexElement(0, 16, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     Estructura de Vertice para formato de malla DIFFUSE_MAP
        /// </summary>
        public struct DiffuseMapVertex
        {
            public Vector3 Position;
            public int Color;
            public float Tu;
            public float Tv;
        }

        #endregion Mesh FVF
    }
}