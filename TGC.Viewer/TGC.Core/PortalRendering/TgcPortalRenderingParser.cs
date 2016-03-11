using System.Xml;
using TGC.Core.Utils;

namespace TGC.Core.PortalRendering
{
    /// <summary>
    ///     Parser de la información de Portal Rendering exportada por el plugin
    /// </summary>
    public class TgcPortalRenderingParser
    {
        /// <summary>
        ///     Parsea la información de PortalRendering
        /// </summary>
        public TgcPortalRenderingData parseFromXmlNode(XmlElement portalRenderingNode)
        {
            var portalRenderingData = new TgcPortalRenderingData();
            int count;
            int i;
            char[] meshSeparator = { ',' };

            //Cells
            var cellsNode = portalRenderingNode.GetElementsByTagName("cells")[0];
            var cellNodes = cellsNode.ChildNodes;
            portalRenderingData.cells = new TgcPortalRenderingCellData[cellNodes.Count];
            i = 0;
            foreach (XmlElement cellElement in cellNodes)
            {
                var cellData = new TgcPortalRenderingCellData();
                portalRenderingData.cells[i++] = cellData;

                //id
                cellData.id = int.Parse(cellElement.Attributes["id"].InnerText);

                //name
                cellData.name = cellElement.Attributes["name"].InnerText;

                //facePlanes
                var facePlanesNode = cellElement.GetElementsByTagName("facePlanes")[0];
                count = int.Parse(facePlanesNode.Attributes["count"].InnerText);
                cellData.facePlanes = TgcParserUtils.parseFloatStream(facePlanesNode.InnerText, count);

                //boundingVertices
                var boundingVerticesNode = cellElement.GetElementsByTagName("boundingVertices")[0];
                count = int.Parse(boundingVerticesNode.Attributes["count"].InnerText);
                cellData.boundingVertices = TgcParserUtils.parseFloatStream(boundingVerticesNode.InnerText, count);

                //meshes
                var meshesNode = cellElement.GetElementsByTagName("meshes")[0];
                count = int.Parse(meshesNode.Attributes["count"].InnerText);
                cellData.meshes = meshesNode.InnerText.Split(meshSeparator);
            }

            //Portals
            var portalsNode = portalRenderingNode.GetElementsByTagName("portals")[0];
            var portalNodes = portalsNode.ChildNodes;
            portalRenderingData.portals = new TgcPortalRenderingPortalData[portalNodes.Count];
            i = 0;
            foreach (XmlElement portalElement in portalNodes)
            {
                var portalData = new TgcPortalRenderingPortalData();
                portalRenderingData.portals[i++] = portalData;

                //name
                portalData.name = portalElement.Attributes["name"].InnerText;

                //boundingBox
                var boundingBoxNode = portalElement.GetElementsByTagName("boundingBox")[0];
                portalData.pMin = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["min"].InnerText);
                portalData.pMax = TgcParserUtils.parseFloat3Array(boundingBoxNode.Attributes["max"].InnerText);

                //cellA
                var cellAElement = (XmlElement)portalElement.GetElementsByTagName("cellA")[0];
                portalData.cellA = int.Parse(cellAElement.Attributes["id"].InnerText);

                //planeA
                var planeANode = cellAElement.GetElementsByTagName("plane")[0];
                portalData.planeA = TgcParserUtils.parseFloat4Array(planeANode.InnerText);

                //verticesA
                var verticesANode = cellAElement.GetElementsByTagName("vertices")[0];
                count = int.Parse(verticesANode.Attributes["count"].InnerText);
                portalData.boundingVerticesA = TgcParserUtils.parseFloatStream(verticesANode.InnerText, count);

                //cellB
                var cellBElement = (XmlElement)portalElement.GetElementsByTagName("cellB")[0];
                portalData.cellB = int.Parse(cellBElement.Attributes["id"].InnerText);

                //planeB
                var planeBNode = cellBElement.GetElementsByTagName("plane")[0];
                portalData.planeB = TgcParserUtils.parseFloat4Array(planeBNode.InnerText);

                //verticesB
                var verticesBNode = cellBElement.GetElementsByTagName("vertices")[0];
                count = int.Parse(verticesBNode.Attributes["count"].InnerText);
                portalData.boundingVerticesB = TgcParserUtils.parseFloatStream(verticesBNode.InnerText, count);
            }

            return portalRenderingData;
        }
    }
}