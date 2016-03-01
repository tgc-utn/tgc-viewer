using System.Drawing;
using System.Xml;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TGC.Viewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     Parser de archivos XML de formato TGC para TgcSkeletalAnimation
    /// </summary>
    public class TgcSkeletalParser
    {
        /// <summary>
        ///     Levanta la informacion del mesh a partir de un XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <returns></returns>
        public TgcSkeletalMeshData parseMeshFromString(string xmlString)
        {
            var dom = new XmlDocument();
            dom.LoadXml(xmlString);
            var root = dom.DocumentElement;

            var meshData = new TgcSkeletalMeshData();

            //Ver si tiene exportacion de texturas
            var texturesExportNode = root.GetElementsByTagName("texturesExport")[0];
            var texturesExportEnabled = bool.Parse(texturesExportNode.Attributes["enabled"].InnerText);
            if (texturesExportEnabled)
            {
                meshData.texturesDir = texturesExportNode.Attributes["dir"].InnerText;
            }

            //Parsear Texturas
            var materialNodes = root.GetElementsByTagName("materials")[0].ChildNodes;
            meshData.materialsData = new TgcMaterialData[materialNodes.Count];
            var i = 0;
            foreach (XmlElement matNode in materialNodes)
            {
                //determinar tipo de Material
                var material = new TgcMaterialData();
                material.type = matNode.Attributes["type"].InnerText;

                //Standard Material
                if (material.type.Equals(TgcMaterialData.StandardMaterial))
                {
                    parseStandardMaterial(material, matNode);
                }

                //Multi Material
                else if (material.type.Equals(TgcMaterialData.MultiMaterial))
                {
                    material.name = matNode.Attributes["name"].InnerText;
                    var subMaterialsNodes = matNode.GetElementsByTagName("subM");
                    material.subMaterials = new TgcMaterialData[subMaterialsNodes.Count];
                    for (var j = 0; j < subMaterialsNodes.Count; j++)
                    {
                        var subMaterial = new TgcMaterialData();
                        parseStandardMaterial(subMaterial, (XmlElement)subMaterialsNodes[j]);
                        material.subMaterials[j] = subMaterial;
                    }
                }

                meshData.materialsData[i++] = material;
            }

            //Parsear Mesh
            var meshNode = (XmlElement)root.GetElementsByTagName("mesh")[0];

            //parser y convertir valores
            meshData.name = meshNode.Attributes["name"].InnerText;
            meshData.materialId = int.Parse(meshNode.Attributes["matId"].InnerText);
            meshData.color = TgcParserUtils.parseFloat3Array(meshNode.Attributes["color"].InnerText);

            //TODO: formatear bien visibility
            var visibilityStr = meshNode.Attributes["visibility"].InnerText;

            //boundingBox, si esta
            var boundingBoxNodes = root.GetElementsByTagName("boundingBox");
            if (boundingBoxNodes != null && boundingBoxNodes.Count == 1)
            {
                var boundingBoxNode = boundingBoxNodes[0];
                meshData.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                meshData.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);
            }

            int count;

            //parsear coordinatesIdx
            var coordinatesIdxNode = meshNode.GetElementsByTagName("coordinatesIdx")[0];
            count = int.Parse(coordinatesIdxNode.Attributes["count"].InnerText);
            meshData.coordinatesIndices = TgcParserUtils.parseIntStream(coordinatesIdxNode.InnerText, count);

            //parsear textCoordsIdx
            var textCoordsIdxNode = meshNode.GetElementsByTagName("textCoordsIdx")[0];
            count = int.Parse(textCoordsIdxNode.Attributes["count"].InnerText);
            meshData.texCoordinatesIndices = TgcParserUtils.parseIntStream(textCoordsIdxNode.InnerText, count);

            //parsear colorsIdx
            var colorsIdxNode = meshNode.GetElementsByTagName("colorsIdx")[0];
            count = int.Parse(colorsIdxNode.Attributes["count"].InnerText);
            meshData.colorIndices = TgcParserUtils.parseIntStream(colorsIdxNode.InnerText, count);

            //parsear matIds
            if (meshData.materialsData.Length > 0)
            {
                var matIdsNode = meshNode.GetElementsByTagName("matIds")[0];
                count = int.Parse(matIdsNode.Attributes["count"].InnerText);
                meshData.materialsIds = TgcParserUtils.parseIntStream(matIdsNode.InnerText, count);
            }

            //parsear vertices
            var verticesNode = meshNode.GetElementsByTagName("vertices")[0];
            count = int.Parse(verticesNode.Attributes["count"].InnerText);
            meshData.verticesCoordinates = TgcParserUtils.parseFloatStream(verticesNode.InnerText, count);

            //parsear texCoords
            var texCoordsNode = meshNode.GetElementsByTagName("texCoords")[0];
            count = int.Parse(texCoordsNode.Attributes["count"].InnerText);
            meshData.textureCoordinates = TgcParserUtils.parseFloatStream(texCoordsNode.InnerText, count);

            //parsear colors
            var colorsNode = meshNode.GetElementsByTagName("colors")[0];
            count = int.Parse(colorsNode.Attributes["count"].InnerText);
            var colorsArray = TgcParserUtils.parseFloatStream(colorsNode.InnerText, count);
            //convertir a format TV
            meshData.verticesColors = new int[count / 3];
            for (var j = 0; j < meshData.verticesColors.Length; j++)
            {
                meshData.verticesColors[j] = Color.FromArgb(
                    (int)colorsArray[j * 3],
                    (int)colorsArray[j * 3 + 1],
                    (int)colorsArray[j * 3 + 2]).ToArgb();
            }

            //parsear normals, si hay
            var normalsNodes = meshNode.GetElementsByTagName("normals");
            if (normalsNodes != null && normalsNodes.Count == 1)
            {
                var normalsNode = normalsNodes[0];
                count = int.Parse(normalsNode.Attributes["count"].InnerText);
                meshData.verticesNormals = TgcParserUtils.parseFloatStream(normalsNode.InnerText, count);
            }

            //parsear tangents, si hay
            var tangentsNodes = meshNode.GetElementsByTagName("tangents");
            if (tangentsNodes != null && tangentsNodes.Count == 1)
            {
                var tangentsNode = tangentsNodes[0];
                count = int.Parse(tangentsNode.Attributes["count"].InnerText);
                meshData.verticesTangents = TgcParserUtils.parseFloatStream(tangentsNode.InnerText, count);
            }

            //parsear binormals, si hay
            var binormalsNodes = meshNode.GetElementsByTagName("binormals");
            if (binormalsNodes != null && binormalsNodes.Count == 1)
            {
                var binormalsNode = binormalsNodes[0];
                count = int.Parse(binormalsNode.Attributes["count"].InnerText);
                meshData.verticesBinormals = TgcParserUtils.parseFloatStream(binormalsNode.InnerText, count);
            }

            //parsear esqueleto
            var skeletonNode = meshNode.GetElementsByTagName("skeleton")[0];
            var bonesData = new TgcSkeletalBoneData[skeletonNode.ChildNodes.Count];
            var boneCount = 0;
            foreach (XmlElement boneNode in skeletonNode.ChildNodes)
            {
                var boneData = new TgcSkeletalBoneData();
                boneData.id = int.Parse(boneNode.Attributes["id"].InnerText);
                boneData.name = boneNode.Attributes["name"].InnerText;
                boneData.parentId = int.Parse(boneNode.Attributes["parentId"].InnerText);
                boneData.startPosition = TgcParserUtils.parseFloat3Array(boneNode.Attributes["pos"].InnerText);
                boneData.startRotation = TgcParserUtils.parseFloat4Array(boneNode.Attributes["rotQuat"].InnerText);

                bonesData[boneCount++] = boneData;
            }
            meshData.bones = bonesData;

            //parsear Weights
            var weightsNode = meshNode.GetElementsByTagName("weights")[0];
            count = int.Parse(weightsNode.Attributes["count"].InnerText);
            meshData.verticesWeights = TgcParserUtils.parseFloatStream(weightsNode.InnerText, count);

            return meshData;
        }

        /// <summary>
        ///     Parsear Standard Material
        /// </summary>
        private void parseStandardMaterial(TgcMaterialData material, XmlElement matNode)
        {
            material.name = matNode.Attributes["name"].InnerText;
            material.type = matNode.Attributes["type"].InnerText;

            //Valores de Material
            var ambientStr = matNode.GetElementsByTagName("ambient")[0].InnerText;
            material.ambientColor = TgcParserUtils.parseFloat4Array(ambientStr);
            TgcParserUtils.divFloatArrayValues(ref material.ambientColor, 255f);

            var diffuseStr = matNode.GetElementsByTagName("diffuse")[0].InnerText;
            material.diffuseColor = TgcParserUtils.parseFloat4Array(diffuseStr);
            TgcParserUtils.divFloatArrayValues(ref material.diffuseColor, 255f);

            var specularStr = matNode.GetElementsByTagName("specular")[0].InnerText;
            material.specularColor = TgcParserUtils.parseFloat4Array(specularStr);
            TgcParserUtils.divFloatArrayValues(ref material.specularColor, 255f);

            var opacityStr = matNode.GetElementsByTagName("opacity")[0].InnerText;
            material.opacity = TgcParserUtils.parseFloat(opacityStr) / 100f;

            //Valores de Bitmap
            var bitmapNode = matNode.GetElementsByTagName("bitmap")[0];
            if (bitmapNode != null)
            {
                material.fileName = bitmapNode.InnerText;

                //TODO: formatear correctamente TILING y OFFSET
                var uvTilingStr = bitmapNode.Attributes["uvTiling"].InnerText;
                material.uvTiling = TgcParserUtils.parseFloat2Array(uvTilingStr);

                var uvOffsetStr = bitmapNode.Attributes["uvOffset"].InnerText;
                material.uvOffset = TgcParserUtils.parseFloat2Array(uvOffsetStr);
            }
            else
            {
                material.fileName = null;
                material.uvTiling = null;
                material.uvOffset = null;
            }
        }

        /// <summary>
        ///     Levanta la informacion de una animacion a partir del XML
        /// </summary>
        /// <param name="xmlString">Contenido que el XML</param>
        /// <returns>Información parseada</returns>
        public TgcSkeletalAnimationData parseAnimationFromString(string xmlString)
        {
            var dom = new XmlDocument();
            dom.LoadXml(xmlString);
            var root = dom.DocumentElement;

            var animation = new TgcSkeletalAnimationData();

            //Parsear informacion general de animation
            var animationNode = (XmlElement)root.GetElementsByTagName("animation")[0];
            animation.name = animationNode.Attributes["name"].InnerText;
            animation.bonesCount = int.Parse(animationNode.Attributes["bonesCount"].InnerText);
            animation.framesCount = int.Parse(animationNode.Attributes["framesCount"].InnerText);
            animation.frameRate = int.Parse(animationNode.Attributes["frameRate"].InnerText);
            animation.startFrame = int.Parse(animationNode.Attributes["startFrame"].InnerText);
            animation.endFrame = int.Parse(animationNode.Attributes["endFrame"].InnerText);

            //Parsear boundingBox, si esta
            var boundingBoxNodes = animationNode.GetElementsByTagName("boundingBox");
            if (boundingBoxNodes != null && boundingBoxNodes.Count == 1)
            {
                var boundingBoxNode = boundingBoxNodes[0];
                animation.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                animation.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);
            }

            //Parsear bones
            var boneNodes = animationNode.GetElementsByTagName("bone");
            animation.bonesFrames = new TgcSkeletalAnimationBoneData[boneNodes.Count];
            var boneIdx = 0;
            foreach (XmlElement boneNode in boneNodes)
            {
                var boneData = new TgcSkeletalAnimationBoneData();
                boneData.id = TgcParserUtils.parseInt(boneNode.Attributes["id"].InnerText);
                boneData.keyFramesCount = TgcParserUtils.parseInt(boneNode.Attributes["keyFramesCount"].InnerText);
                boneData.keyFrames = new TgcSkeletalAnimationBoneFrameData[boneData.keyFramesCount];

                //Parsear frames
                var frames = 0;
                foreach (XmlElement frameNode in boneNode.ChildNodes)
                {
                    var frameData = new TgcSkeletalAnimationBoneFrameData();
                    frameData.frame = TgcParserUtils.parseInt(frameNode.Attributes["n"].InnerText);
                    frameData.position = TgcParserUtils.parseFloat3Array(frameNode.Attributes["pos"].InnerText);
                    frameData.rotation = TgcParserUtils.parseFloat4Array(frameNode.Attributes["rotQuat"].InnerText);

                    boneData.keyFrames[frames++] = frameData;
                }

                animation.bonesFrames[boneIdx++] = boneData;
            }

            return animation;
        }
    }
}