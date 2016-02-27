using System.Drawing;
using System.Xml;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    /// <summary>
    ///     Parser de XML de mesh animado por KeyFrame creado con plugin TgcKeyFrameExporter.ms de 3DsMax
    /// </summary>
    public class TgcKeyFrameParser
    {
        /// <summary>
        ///     Levanta la informacion del mesh a partir de un XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <returns></returns>
        public TgcKeyFrameMeshData parseMeshFromString(string xmlString)
        {
            var dom = new XmlDocument();
            dom.LoadXml(xmlString);
            var root = dom.DocumentElement;

            var meshData = new TgcKeyFrameMeshData();

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
                        parseStandardMaterial(subMaterial, (XmlElement) subMaterialsNodes[j]);
                        material.subMaterials[j] = subMaterial;
                    }
                }

                meshData.materialsData[i++] = material;
            }

            //Parsear Mesh
            var meshNode = (XmlElement) root.GetElementsByTagName("mesh")[0];

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
            meshData.verticesColors = new int[count/3];
            for (var j = 0; j < meshData.verticesColors.Length; j++)
            {
                meshData.verticesColors[j] = Color.FromArgb(
                    (int) colorsArray[j*3],
                    (int) colorsArray[j*3 + 1],
                    (int) colorsArray[j*3 + 2]).ToArgb();
            }

            return meshData;
        }

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
            material.opacity = TgcParserUtils.parseFloat(opacityStr)/100f;

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
        public TgcKeyFrameAnimationData parseAnimationFromString(string xmlString)
        {
            var dom = new XmlDocument();
            dom.LoadXml(xmlString);
            var root = dom.DocumentElement;

            var animation = new TgcKeyFrameAnimationData();

            //Parsear informacion general de animation
            var animationNode = (XmlElement) root.GetElementsByTagName("animation")[0];
            animation.name = animationNode.Attributes["name"].InnerText;
            animation.verticesCount = int.Parse(animationNode.Attributes["verticesCount"].InnerText);
            animation.framesCount = int.Parse(animationNode.Attributes["framesCount"].InnerText);
            animation.keyFramesCount = int.Parse(animationNode.Attributes["keyFramesCount"].InnerText);
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

            var frameNodes = animationNode.GetElementsByTagName("frame");
            animation.keyFrames = new TgcKeyFrameFrameData[frameNodes.Count];
            var i = 0;
            foreach (XmlElement frameNode in frameNodes)
            {
                var frame = new TgcKeyFrameFrameData();
                var frameNumber = (int) TgcParserUtils.parseFloat(frameNode.Attributes["time"].InnerText);
                frame.relativeTime = (float) frameNumber/animation.framesCount;

                //parsear vertices
                frame.verticesCoordinates = TgcParserUtils.parseFloatStream(frameNode.InnerText, animation.verticesCount);

                animation.keyFrames[i++] = frame;
            }

            return animation;
        }
    }
}