using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using TGC.Core.Utils;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.PortalRendering
{
    /// <summary>
    /// Parser de la información de Portal Rendering exportada por el plugin
    /// </summary>
    public class TgcPortalRenderingParser
    {
        /// <summary>
        /// Parsea la información de PortalRendering
        /// </summary>
        public TgcPortalRenderingData parseFromXmlNode(XmlElement portalRenderingNode)
        {
            TgcPortalRenderingData portalRenderingData = new TgcPortalRenderingData();
            int count;
            int i;
            char[] meshSeparator = new char[]{','};

            //Cells
            XmlNode cellsNode = portalRenderingNode.GetElementsByTagName("cells")[0];
            XmlNodeList cellNodes = cellsNode.ChildNodes;
            portalRenderingData.cells = new TgcPortalRenderingCellData[cellNodes.Count];
            i = 0;
            foreach (XmlElement cellElement in cellNodes)
            {
                TgcPortalRenderingCellData cellData = new TgcPortalRenderingCellData();
                portalRenderingData.cells[i++] = cellData;

                //id
                cellData.id = int.Parse(cellElement.Attributes["id"].InnerText);

                //name
                cellData.name = cellElement.Attributes["name"].InnerText;

                //facePlanes
                XmlNode facePlanesNode = cellElement.GetElementsByTagName("facePlanes")[0];
                count = int.Parse(facePlanesNode.Attributes["count"].InnerText);
                cellData.facePlanes = TgcParserUtils.parseFloatStream(facePlanesNode.InnerText, count);

                //boundingVertices
                XmlNode boundingVerticesNode = cellElement.GetElementsByTagName("boundingVertices")[0];
                count = int.Parse(boundingVerticesNode.Attributes["count"].InnerText);
                cellData.boundingVertices = TgcParserUtils.parseFloatStream(boundingVerticesNode.InnerText, count);

                //meshes
                XmlNode meshesNode = cellElement.GetElementsByTagName("meshes")[0];
                count = int.Parse(meshesNode.Attributes["count"].InnerText);
                cellData.meshes = meshesNode.InnerText.Split(meshSeparator);
            }

            //Portals
            XmlNode portalsNode = portalRenderingNode.GetElementsByTagName("portals")[0];
            XmlNodeList portalNodes = portalsNode.ChildNodes;
            portalRenderingData.portals = new TgcPortalRenderingPortalData[portalNodes.Count];
            i = 0;
            foreach (XmlElement portalElement in portalNodes)
            {
                TgcPortalRenderingPortalData portalData = new TgcPortalRenderingPortalData();
                portalRenderingData.portals[i++] = portalData;

                //name
                portalData.name = portalElement.Attributes["name"].InnerText;

                //boundingBox
                XmlNode boundingBoxNode = portalElement.GetElementsByTagName("boundingBox")[0];
                portalData.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                portalData.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);

                //cellA
                XmlElement cellAElement = (XmlElement)portalElement.GetElementsByTagName("cellA")[0];
                portalData.cellA = int.Parse(cellAElement.Attributes["id"].InnerText);

                //planeA
                XmlNode planeANode = cellAElement.GetElementsByTagName("plane")[0];
                portalData.planeA = TgcParserUtils.parseFloat4Array(planeANode.InnerText);

                //verticesA
                XmlNode verticesANode = cellAElement.GetElementsByTagName("vertices")[0];
                count = int.Parse(verticesANode.Attributes["count"].InnerText);
                portalData.boundingVerticesA = TgcParserUtils.parseFloatStream(verticesANode.InnerText, count);

                //cellB
                XmlElement cellBElement = (XmlElement)portalElement.GetElementsByTagName("cellB")[0];
                portalData.cellB = int.Parse(cellBElement.Attributes["id"].InnerText);

                //planeB
                XmlNode planeBNode = cellBElement.GetElementsByTagName("plane")[0];
                portalData.planeB = TgcParserUtils.parseFloat4Array(planeBNode.InnerText);

                //verticesB
                XmlNode verticesBNode = cellBElement.GetElementsByTagName("vertices")[0];
                count = int.Parse(verticesBNode.Attributes["count"].InnerText);
                portalData.boundingVerticesB = TgcParserUtils.parseFloatStream(verticesBNode.InnerText, count);
            }

            return portalRenderingData;
        }


    }
}
