using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.PortalRendering;

namespace TgcViewer.Utils.TgcSceneLoader
{
    /// <summary>
    /// Parser de XML de escena creado con plugin TgcSceneExporter.ms de 3DsMax
    /// </summary>
    public class TgcSceneParser
    {
        /// <summary>
        /// Levanta la informacion del XML
        /// </summary>
        /// <param name="xmlString">contenido del XML</param>
        /// <returns></returns>
        public TgcSceneData parseSceneFromString(string xmlString)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xmlString);
            XmlElement root = dom.DocumentElement;

            string sceneName = root.GetElementsByTagName("name")[0].InnerText;
            TgcSceneData tgcSceneData = new TgcSceneData();
            tgcSceneData.name = sceneName;

            //Ver si tiene exportacion de texturas
            XmlNode texturesExportNode = root.GetElementsByTagName("texturesExport")[0];
            bool texturesExportEnabled = bool.Parse(texturesExportNode.Attributes["enabled"].InnerText);
            if (texturesExportEnabled)
            {
                tgcSceneData.texturesDir = texturesExportNode.Attributes["dir"].InnerText;
            }

            //Ver si tiene LightMaps
            XmlNode lightmapsExportNode = root.GetElementsByTagName("lightmapExport")[0];
            tgcSceneData.lightmapsEnabled = bool.Parse(lightmapsExportNode.Attributes["enabled"].InnerText);
            if (tgcSceneData.lightmapsEnabled)
            {
                tgcSceneData.lightmapsDir = lightmapsExportNode.Attributes["dir"].InnerText;
            }

            //sceneBoundingBox, si está
            XmlNodeList sceneBoundingBoxNodes = root.GetElementsByTagName("sceneBoundingBox");
            if (sceneBoundingBoxNodes != null && sceneBoundingBoxNodes.Count == 1)
            {
                XmlNode sceneBoundingBoxNode = sceneBoundingBoxNodes[0];
                tgcSceneData.pMin = TgcParserUtils.parseFloat3Array(sceneBoundingBoxNode.Attributes["min"].InnerText);
                tgcSceneData.pMax = TgcParserUtils.parseFloat3Array(sceneBoundingBoxNode.Attributes["max"].InnerText);
            }


            //Parsear Texturas
            XmlNodeList materialNodes = root.GetElementsByTagName("materials")[0].ChildNodes;
            tgcSceneData.materialsData = new TgcMaterialData[materialNodes.Count];
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

                tgcSceneData.materialsData[i++] = material;
            }



            //Parsear Meshes
            XmlNodeList meshesNodes = root.GetElementsByTagName("meshes")[0].ChildNodes;
            tgcSceneData.meshesData = new TgcMeshData[meshesNodes.Count];
            i = 0;
            int count;
            foreach (XmlElement meshNode in meshesNodes)
            {
                TgcMeshData meshData = new TgcMeshData();
                tgcSceneData.meshesData[i++] = meshData;

                //parser y convertir valores
                meshData.name = meshNode.Attributes["name"].InnerText;
                meshData.materialId = int.Parse(meshNode.Attributes["matId"].InnerText);
                meshData.color = TgcParserUtils.parseFloat3Array(meshNode.Attributes["color"].InnerText);
                meshData.lightmap = meshNode.Attributes["lightmap"].InnerText;

                //type
                XmlAttribute typeAttr = meshNode.Attributes["type"];
                meshData.instanceType = TgcMeshData.ORIGINAL;
                if (typeAttr != null)
                {
                    meshData.instanceType = typeAttr.InnerText;
                }

                //layer
                XmlAttribute layerAttr = meshNode.Attributes["layer"];
                if (layerAttr != null)
                {
                    meshData.layerName = layerAttr.InnerText;
                }

                //visibility
                float visibility = TgcParserUtils.parseFloat(meshNode.Attributes["visibility"].InnerText);
                meshData.alphaBlending = visibility != 1.0f ? true : false;

                //parsear boundingBox
                XmlNodeList boundingBoxNodes = meshNode.GetElementsByTagName("boundingBox");
                if(boundingBoxNodes != null && boundingBoxNodes.Count == 1)
                {
                    XmlNode boundingBoxNode = boundingBoxNodes[0];
                    meshData.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                    meshData.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);
                }

                //parsear datos de mesh Original
                if (meshData.instanceType.Equals(TgcMeshData.ORIGINAL))
                {
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
                    //TODO: ver bien como calcula esto el SCRIPT de Exportacion
                    if (meshData.materialId != -1)
                    {
                        XmlNode matIdsNode = meshNode.GetElementsByTagName("matIds")[0];
                        count = int.Parse(matIdsNode.Attributes["count"].InnerText);
                        meshData.materialsIds = TgcParserUtils.parseIntStream(matIdsNode.InnerText, count);
                    }

                    //parsear textCoordsLightMapIdx
                    meshData.lightmapEnabled = tgcSceneData.lightmapsEnabled && (meshData.lightmap.Trim()).Length > 0;
                    if (meshData.lightmapEnabled)
                    {
                        XmlNode textCoordsLightMapIdxNode = meshNode.GetElementsByTagName("textCoordsLightMapIdx")[0];
                        count = int.Parse(textCoordsLightMapIdxNode.Attributes["count"].InnerText);
                        meshData.texCoordinatesIndicesLightMap = TgcParserUtils.parseIntStream(textCoordsLightMapIdxNode.InnerText, count);
                    }


                    //parsear vertices
                    XmlNode verticesNode = meshNode.GetElementsByTagName("vertices")[0];
                    count = int.Parse(verticesNode.Attributes["count"].InnerText);
                    meshData.verticesCoordinates = TgcParserUtils.parseFloatStream(verticesNode.InnerText, count);

                    //parsear normals
                    XmlNode normalsNode = meshNode.GetElementsByTagName("normals")[0];
                    count = int.Parse(normalsNode.Attributes["count"].InnerText);
                    meshData.verticesNormals = TgcParserUtils.parseFloatStream(normalsNode.InnerText, count);

                    //parsear tangents, si hay
                    XmlNodeList tangentsNodes = meshNode.GetElementsByTagName("tangents");
                    if (tangentsNodes != null && tangentsNodes.Count == 1)
                    {
                        XmlNode tangentsNode = tangentsNodes[0];
                        count = int.Parse(tangentsNode.Attributes["count"].InnerText);
                        meshData.verticesTangents = TgcParserUtils.parseFloatStream(tangentsNode.InnerText, count);
                    }

                    //parsear binormals, si hay
                    XmlNodeList binormalsNodes = meshNode.GetElementsByTagName("binormals");
                    if (binormalsNodes != null && binormalsNodes.Count == 1)
                    {
                        XmlNode binormalsNode = binormalsNodes[0];
                        count = int.Parse(binormalsNode.Attributes["count"].InnerText);
                        meshData.verticesBinormals = TgcParserUtils.parseFloatStream(binormalsNode.InnerText, count);
                    }

                    //parsear texCoords
                    XmlNode texCoordsNode = meshNode.GetElementsByTagName("texCoords")[0];
                    count = int.Parse(texCoordsNode.Attributes["count"].InnerText);
                    meshData.textureCoordinates = TgcParserUtils.parseFloatStream(texCoordsNode.InnerText, count);

                    //parsear colors
                    XmlNode colorsNode = meshNode.GetElementsByTagName("colors")[0];
                    count = int.Parse(colorsNode.Attributes["count"].InnerText);
                    float[] colorsArray = TgcParserUtils.parseFloatStream(colorsNode.InnerText, count);
                    //convertir a formato DirectX
                    meshData.verticesColors = new int[count / 3];
                    for (int j = 0; j < meshData.verticesColors.Length; j++)
                    {
                        meshData.verticesColors[j] = Color.FromArgb(
                            (int)colorsArray[j * 3],
                            (int)colorsArray[j * 3 + 1],
                            (int)colorsArray[j * 3 + 2]).ToArgb();
                    }

                    //parsear texCoordsLightMap
                    if (meshData.lightmapEnabled)
                    {
                        XmlNode texCoordsLightMapNode = meshNode.GetElementsByTagName("texCoordsLightMap")[0];
                        count = int.Parse(texCoordsLightMapNode.Attributes["count"].InnerText);
                        meshData.textureCoordinatesLightMap = TgcParserUtils.parseFloatStream(texCoordsLightMapNode.InnerText, count);
                    }
                }

                //parsear datos de mesh Instancia
                else if(meshData.instanceType.Equals(TgcMeshData.INSTANCE))
                {
                    //originalMesh
                    XmlNode originalMeshNode = meshNode.GetElementsByTagName("originalMesh")[0];
                    meshData.originalMesh = TgcParserUtils.parseInt(originalMeshNode.InnerText);

                    //transform
                    XmlNode transformNode = meshNode.GetElementsByTagName("transform")[0];
                    meshData.position = TgcParserUtils.parseFloat3Array(transformNode.Attributes["pos"].InnerText);
                    meshData.rotation = TgcParserUtils.parseFloat4Array(transformNode.Attributes["rotQuat"].InnerText);
                    meshData.scale = TgcParserUtils.parseFloat3Array(transformNode.Attributes["scale"].InnerText);
                }

                //Parsear userProperties, si hay
                XmlNodeList userPropsNodes = meshNode.GetElementsByTagName("userProps");
                if (userPropsNodes != null && userPropsNodes.Count == 1)
                {
                    meshData.userProperties = new Dictionary<string, string>();
                    XmlElement userPropsNode = (XmlElement)userPropsNodes[0];
                    foreach (XmlElement prop in userPropsNode.ChildNodes)
                    {
                        meshData.userProperties.Add(prop.Name, prop.InnerText);
                    }
                }
            }


            //Parsear PortalRendering, si hay información
            XmlNodeList portalRenderingNodes = root.GetElementsByTagName("portalRendering");
            if (portalRenderingNodes != null && portalRenderingNodes.Count == 1)
            {
                XmlElement portalRenderingNode = (XmlElement)portalRenderingNodes[0];
                TgcPortalRenderingParser portalParser = new TgcPortalRenderingParser();
                tgcSceneData.portalData = portalParser.parseFromXmlNode(portalRenderingNode);
            }

            

            return tgcSceneData;
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

            XmlNode alphaBlendEnableNode = matNode.GetElementsByTagName("alphaBlendEnable")[0];
            if (alphaBlendEnableNode != null)
            {
                string alphaBlendEnableStr = alphaBlendEnableNode.InnerText;
                material.alphaBlendEnable = bool.Parse(alphaBlendEnableStr);
            }
            


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


    }
}
