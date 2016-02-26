using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TGC.Core.Utils;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    /// <summary>
    /// Parser de XML de mesh animado por KeyFrame creado con plugin TgcKeyFrameExporter.ms de 3DsMax
    /// </summary>
    public class TgcKeyFrameParser
    {
        /// <summary>
        /// Levanta la informacion del mesh a partir de un XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <returns></returns>
        public TgcKeyFrameMeshData parseMeshFromString(string xmlString)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xmlString);
            XmlElement root = dom.DocumentElement;

            TgcKeyFrameMeshData meshData = new TgcKeyFrameMeshData();

            //Ver si tiene exportacion de texturas
            XmlNode texturesExportNode = root.GetElementsByTagName("texturesExport")[0];
            bool texturesExportEnabled = bool.Parse(texturesExportNode.Attributes["enabled"].InnerText);
            if (texturesExportEnabled)
            {
                meshData.texturesDir = texturesExportNode.Attributes["dir"].InnerText;
            }

            //Parsear Texturas
            XmlNodeList materialNodes = root.GetElementsByTagName("materials")[0].ChildNodes;
            meshData.materialsData = new TgcMaterialData[materialNodes.Count];
            int i = 0;
            foreach (XmlElement matNode in materialNodes)
            {
                //determinar tipo de Material
                TgcMaterialData material = new TgcMaterialData();
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
                    XmlNodeList subMaterialsNodes = matNode.GetElementsByTagName("subM");
                    material.subMaterials = new TgcMaterialData[subMaterialsNodes.Count];
                    for (int j = 0; j < subMaterialsNodes.Count; j++)
                    {
                        TgcMaterialData subMaterial = new TgcMaterialData();
                        parseStandardMaterial(subMaterial, (XmlElement)subMaterialsNodes[j]);
                        material.subMaterials[j] = subMaterial;
                    }
                }

                meshData.materialsData[i++] = material;
            }


            //Parsear Mesh
            XmlElement meshNode = (XmlElement)root.GetElementsByTagName("mesh")[0];

            //parser y convertir valores
            meshData.name = meshNode.Attributes["name"].InnerText;
            meshData.materialId = int.Parse(meshNode.Attributes["matId"].InnerText);
            meshData.color = TgcParserUtils.parseFloat3Array(meshNode.Attributes["color"].InnerText);

            //TODO: formatear bien visibility
            string visibilityStr = meshNode.Attributes["visibility"].InnerText;

            //boundingBox, si esta
            XmlNodeList boundingBoxNodes = root.GetElementsByTagName("boundingBox");
            if (boundingBoxNodes != null && boundingBoxNodes.Count == 1)
            {
                XmlNode boundingBoxNode = boundingBoxNodes[0];
                meshData.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                meshData.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);
            }


            int count;

            //parsear coordinatesIdx
            XmlNode coordinatesIdxNode = meshNode.GetElementsByTagName("coordinatesIdx")[0];
            count = int.Parse(coordinatesIdxNode.Attributes["count"].InnerText);
            meshData.coordinatesIndices = TgcParserUtils.parseIntStream(coordinatesIdxNode.InnerText, count);

            //parsear textCoordsIdx
            XmlNode textCoordsIdxNode = meshNode.GetElementsByTagName("textCoordsIdx")[0];
            count = int.Parse(textCoordsIdxNode.Attributes["count"].InnerText);
            meshData.texCoordinatesIndices = TgcParserUtils.parseIntStream(textCoordsIdxNode.InnerText, count);

            //parsear colorsIdx
            XmlNode colorsIdxNode = meshNode.GetElementsByTagName("colorsIdx")[0];
            count = int.Parse(colorsIdxNode.Attributes["count"].InnerText);
            meshData.colorIndices = TgcParserUtils.parseIntStream(colorsIdxNode.InnerText, count);

            //parsear matIds
            if (meshData.materialsData.Length > 0)
            {
                XmlNode matIdsNode = meshNode.GetElementsByTagName("matIds")[0];
                count = int.Parse(matIdsNode.Attributes["count"].InnerText);
                meshData.materialsIds = TgcParserUtils.parseIntStream(matIdsNode.InnerText, count);
            }

            //parsear vertices
            XmlNode verticesNode = meshNode.GetElementsByTagName("vertices")[0];
            count = int.Parse(verticesNode.Attributes["count"].InnerText);
            meshData.verticesCoordinates = TgcParserUtils.parseFloatStream(verticesNode.InnerText, count);

            //parsear texCoords
            XmlNode texCoordsNode = meshNode.GetElementsByTagName("texCoords")[0];
            count = int.Parse(texCoordsNode.Attributes["count"].InnerText);
            meshData.textureCoordinates = TgcParserUtils.parseFloatStream(texCoordsNode.InnerText, count);

            //parsear colors
            XmlNode colorsNode = meshNode.GetElementsByTagName("colors")[0];
            count = int.Parse(colorsNode.Attributes["count"].InnerText);
            float[] colorsArray = TgcParserUtils.parseFloatStream(colorsNode.InnerText, count);
            //convertir a format TV
            meshData.verticesColors = new int[count / 3];
            for (int j = 0; j < meshData.verticesColors.Length; j++)
            {
                meshData.verticesColors[j] = Color.FromArgb(
                    (int)colorsArray[j * 3],
                    (int)colorsArray[j * 3 + 1],
                    (int)colorsArray[j * 3 + 2]).ToArgb();
            }

            return meshData;
        }

        private void parseStandardMaterial(TgcMaterialData material, XmlElement matNode)
        {
            material.name = matNode.Attributes["name"].InnerText;
            material.type = matNode.Attributes["type"].InnerText;

            //Valores de Material
            string ambientStr = matNode.GetElementsByTagName("ambient")[0].InnerText;
            material.ambientColor = TgcParserUtils.parseFloat4Array(ambientStr);
            TgcParserUtils.divFloatArrayValues(ref material.ambientColor, 255f);

            string diffuseStr = matNode.GetElementsByTagName("diffuse")[0].InnerText;
            material.diffuseColor = TgcParserUtils.parseFloat4Array(diffuseStr);
            TgcParserUtils.divFloatArrayValues(ref material.diffuseColor, 255f);

            string specularStr = matNode.GetElementsByTagName("specular")[0].InnerText;
            material.specularColor = TgcParserUtils.parseFloat4Array(specularStr);
            TgcParserUtils.divFloatArrayValues(ref material.specularColor, 255f);

            string opacityStr = matNode.GetElementsByTagName("opacity")[0].InnerText;
            material.opacity = TgcParserUtils.parseFloat(opacityStr) / 100f;


            //Valores de Bitmap
            XmlNode bitmapNode = matNode.GetElementsByTagName("bitmap")[0];
            if (bitmapNode != null)
            {
                material.fileName = bitmapNode.InnerText;

                //TODO: formatear correctamente TILING y OFFSET
                string uvTilingStr = bitmapNode.Attributes["uvTiling"].InnerText;
                material.uvTiling = TgcParserUtils.parseFloat2Array(uvTilingStr);

                string uvOffsetStr = bitmapNode.Attributes["uvOffset"].InnerText;
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
        /// Levanta la informacion de una animacion a partir del XML
        /// </summary>
        /// <param name="xmlString">Contenido que el XML</param>
        public TgcKeyFrameAnimationData parseAnimationFromString(string xmlString)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xmlString);
            XmlElement root = dom.DocumentElement;

            TgcKeyFrameAnimationData animation = new TgcKeyFrameAnimationData();

            //Parsear informacion general de animation
            XmlElement animationNode = (XmlElement)root.GetElementsByTagName("animation")[0];
            animation.name = animationNode.Attributes["name"].InnerText;
            animation.verticesCount = int.Parse(animationNode.Attributes["verticesCount"].InnerText);
            animation.framesCount = int.Parse(animationNode.Attributes["framesCount"].InnerText);
            animation.keyFramesCount = int.Parse(animationNode.Attributes["keyFramesCount"].InnerText);
            animation.frameRate = int.Parse(animationNode.Attributes["frameRate"].InnerText);
            animation.startFrame = int.Parse(animationNode.Attributes["startFrame"].InnerText);
            animation.endFrame = int.Parse(animationNode.Attributes["endFrame"].InnerText);

            //Parsear boundingBox, si esta
            XmlNodeList boundingBoxNodes = animationNode.GetElementsByTagName("boundingBox");
            if (boundingBoxNodes != null && boundingBoxNodes.Count == 1)
            {
                XmlNode boundingBoxNode = boundingBoxNodes[0];
                animation.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                animation.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);
            }

            XmlNodeList frameNodes = animationNode.GetElementsByTagName("frame");
            animation.keyFrames = new TgcKeyFrameFrameData[frameNodes.Count];
            int i = 0;
            foreach (XmlElement frameNode in frameNodes)
            {
                TgcKeyFrameFrameData frame = new TgcKeyFrameFrameData();
                int frameNumber = (int)TgcParserUtils.parseFloat(frameNode.Attributes["time"].InnerText);
                frame.relativeTime = (float)frameNumber / animation.framesCount;


                //parsear vertices
                frame.verticesCoordinates = TgcParserUtils.parseFloatStream(frameNode.InnerText, animation.verticesCount);

                animation.keyFrames[i++] = frame;
            }



            return animation;
        }



    }
}
