using System.Xml;
using TgcViewer;
using TGC.Core.Example;
using TGC.Core.Utils;

namespace Examples.SceneEditor
{
    /// <summary>
    ///     Ejemplo TgcSceneEditor:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh, Transformaciones, GameEngine
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox, Picking
    ///     # Unidad 7 - Técnicas de Optimización - Frustum Culling
    ///     Ejemplo que muestra como crear un editor de escenarios.
    ///     Permite cargar varias mallas estaticas, moverlas, rotarlas y escalarlas.
    ///     También permite crear un terreno.
    ///     El ejemplo crea su propio Modifier con todos los controles visuales de .NET que necesita.
    ///     Luego toda la información puede ser exportada a un archivo para su posterior uso.
    ///     Se utiliza el método "exportScene()" para grabar la información de la escena en un XML de formato TgcScene.
    ///     Las instrucciones se muestran al hacer clic en el botón "Help" de este Modifier.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class TgcSceneEditor : TgcExample
    {
        private SceneEditorModifier modifier;

        public override string getCategory()
        {
            return "Utils";
        }

        public override string getName()
        {
            return "SceneEditor";
        }

        public override string getDescription()
        {
            return "Editor de escena. Permite abrir modelos en formato TGC y posicionarlos dentro de un escenario." +
                   "Luego esa información se puede exportar a un archivo XML para su posterior uso.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            modifier = new SceneEditorModifier("SceneEditor", this);
            GuiController.Instance.Modifiers.add(modifier);

            GuiController.Instance.RotCamera.Enable = false;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Delegar render al control
            modifier.EditorControl.render();
        }

        public override void close()
        {
            //Delegar al control
            modifier.EditorControl.close();
        }

        /// <summary>
        ///     Método que se llama cuando se quiere exportar la informacion de la escena a un XML.
        ///     MODIFICAR ESTA SECCION PARA ADAPTARSE A LAS NECESIDADES DEL ALUMNO
        /// </summary>
        internal void exportScene(string savePath)
        {
            //Crea XML
            var doc = new XmlDocument();
            XmlNode root = doc.CreateElement("SceneEditor-Export");

            //Guardar info del terreno
            var terrainNode = doc.CreateElement("Terrain");
            var terrain = modifier.EditorControl.TgcTerrain;
            if (terrain != null)
            {
                terrainNode.SetAttribute("enable", true.ToString());
                terrainNode.SetAttribute("heightmap", modifier.EditorControl.heighmap.Text);
                terrainNode.SetAttribute("texture", modifier.EditorControl.terrainTexture.Text);
                terrainNode.SetAttribute("xzScale", modifier.EditorControl.terrainXZscale.Value.ToString());
                terrainNode.SetAttribute("yScale", modifier.EditorControl.terrainYscale.Value.ToString());
                terrainNode.SetAttribute("center", TgcParserUtils.printVector3FromString(
                    modifier.EditorControl.terrainCenterX.Text,
                    modifier.EditorControl.terrainCenterY.Text,
                    modifier.EditorControl.terrainCenterZ.Text));
            }
            else
            {
                terrainNode.SetAttribute("enable", false.ToString());
            }
            root.AppendChild(terrainNode);

            //Recorrer Meshes del escenario, ordenadas por grupo
            var meshObjects = modifier.EditorControl.getMeshObjectsOrderByGroup();
            var meshesNode = doc.CreateElement("Meshes");
            var groupIndex = -1;
            XmlElement lastGroupNode = null;
            foreach (var meshObject in meshObjects)
            {
                //Crear grupo con corte de control
                if (meshObject.groupIndex > groupIndex)
                {
                    groupIndex = meshObject.groupIndex;
                    lastGroupNode = doc.CreateElement("MeshGroup");
                    lastGroupNode.SetAttribute("groupIndex", groupIndex.ToString());
                    meshesNode.AppendChild(lastGroupNode);
                }

                //Guardar info de mesh
                var mesh = meshObject.mesh;
                var meshNode = doc.CreateElement("Mesh");
                meshNode.SetAttribute("name", meshObject.name);
                meshNode.SetAttribute("index", meshObject.index.ToString());
                meshNode.SetAttribute("file", meshObject.fileName);
                meshNode.SetAttribute("folder", meshObject.folderName);
                meshNode.SetAttribute("position", TgcParserUtils.printVector3(mesh.Position));
                meshNode.SetAttribute("rotation", TgcParserUtils.printVector3(mesh.Rotation));
                meshNode.SetAttribute("scale", TgcParserUtils.printVector3(mesh.Scale));
                meshNode.SetAttribute("userInfo", meshObject.userInfo);

                lastGroupNode.AppendChild(meshNode);
            }
            root.AppendChild(meshesNode);

            //Guardar XML
            doc.AppendChild(root);
            doc.Save(savePath);
        }
    }
}