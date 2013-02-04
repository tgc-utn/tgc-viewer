using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using Examples.SceneEditor;
using System.Windows.Forms;
using TgcViewer.Utils.Terrain;
using System.Xml;
using System.Globalization;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;

namespace Examples.SceneEditor
{
    /// <summary>
    /// Ejemplo TgcSceneEditor:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh, Transformaciones, GameEngine
    ///     # Unidad 6 - Detecci�n de Colisiones - BoundingBox, Picking
    ///     # Unidad 7 - T�cnicas de Optimizaci�n - Frustum Culling
    /// 
    /// Ejemplo que muestra como crear un editor de escenarios.
    /// Permite cargar varias mallas estaticas, moverlas, rotarlas y escalarlas.
    /// Tambi�n permite crear un terreno.
    /// El ejemplo crea su propio Modifier con todos los controles visuales de .NET que necesita.
    /// Luego toda la informaci�n puede ser exportada a un archivo para su posterior uso.
    /// Se utiliza el m�todo "exportScene()" para grabar la informaci�n de la escena en un XML de formato TgcScene.
    /// Las instrucciones se muestran al hacer clic en el bot�n "Help" de este Modifier.
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class TgcSceneEditor : TgcExample
    {
        SceneEditorModifier modifier;

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
                "Luego esa informaci�n se puede exportar a un archivo XML para su posterior uso.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            modifier = new SceneEditorModifier("SceneEditor", this);
            GuiController.Instance.Modifiers.add(modifier);

            GuiController.Instance.RotCamera.Enable = false;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Delegar render al control
            modifier.EditorControl.render();
        }

        public override void close()
        {
            //Delegar al control
            modifier.EditorControl.close();
        }



        /// <summary>
        /// M�todo que se llama cuando se quiere exportar la informacion de la escena a un XML.
        /// 
        /// 
        /// MODIFICAR ESTA SECCION PARA ADAPTARSE A LAS NECESIDADES DEL ALUMNO
        /// 
        /// 
        /// 
        /// </summary>
        internal void exportScene(string savePath)
        {
            //Crea XML
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("SceneEditor-Export");

            //Guardar info del terreno
            XmlElement terrainNode = doc.CreateElement("Terrain");
            TgcSimpleTerrain terrain = modifier.EditorControl.TgcTerrain;
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
            List<SceneEditorMeshObject> meshObjects = modifier.EditorControl.getMeshObjectsOrderByGroup();
            XmlElement meshesNode = doc.CreateElement("Meshes");
            int groupIndex = -1;
            XmlElement lastGroupNode = null;
            foreach (SceneEditorMeshObject meshObject in meshObjects)
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
                TgcMesh mesh = meshObject.mesh;
                XmlElement meshNode = doc.CreateElement("Mesh");
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
