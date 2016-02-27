using System;
using System.Drawing;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Example;
using TGC.Core.Utils;

namespace Examples.SceneLoader
{
    /// <summary>
    ///     Ejemplo EjemploMeshLoader:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Permite cargar una malla estática de formato TGC desde el FileSystem.
    ///     Utiliza la herramienta TgcMeshLoader.
    ///     Esta herramienta crea un objeto TgcScene, compuesto a su vez por N TgcMesh
    ///     Cada uno representa una malla estática.
    ///     La escena es cargada desde un archivo XML de formato TGC
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMeshLoader : TgcExample
    {
        private bool currentAlphaBlending;
        private Color currentColor;
        private string currentPath;
        private TgcScene currentScene;

        public override string getCategory()
        {
            return "SceneLoader";
        }

        public override string getName()
        {
            return "MeshLoader";
        }

        public override string getDescription()
        {
            return "Ejemplo de como cargar una Malla estática en formato TGC";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Malla default
            var initialMeshFile = GuiController.Instance.ExamplesMediaDir +
                                  "MeshCreator\\Meshes\\Vehiculos\\CamionDeAgua\\" + "CamionDeAgua-TgcScene.xml";

            //Modifiers
            currentScene = null;
            currentPath = null;
            GuiController.Instance.Modifiers.addFile("Mesh", initialMeshFile, "-TgcScene.xml |*-TgcScene.xml");

            GuiController.Instance.Modifiers.addButton("Reload", "Reload", Reload_ButtonClick);

            currentColor = Color.White;
            GuiController.Instance.Modifiers.addColor("Color", currentColor);

            GuiController.Instance.Modifiers.addBoolean("BoundingBox", "BoundingBox", false);

            currentAlphaBlending = false;
            GuiController.Instance.Modifiers.addBoolean("AlphaBlending", "AlphaBlending", currentAlphaBlending);

            //UserVars
            GuiController.Instance.UserVars.addVar("Name");
            GuiController.Instance.UserVars.addVar("Meshes");
            GuiController.Instance.UserVars.addVar("Textures");
            GuiController.Instance.UserVars.addVar("Triangles");
            GuiController.Instance.UserVars.addVar("Vertices");
            GuiController.Instance.UserVars.addVar("SizeX");
            GuiController.Instance.UserVars.addVar("SizeY");
            GuiController.Instance.UserVars.addVar("SizeZ");
        }

        /// <summary>
        ///     Ver si hay que cargar una nueva malla
        /// </summary>
        private void checkLoadMesh(string path)
        {
            if (currentPath == null || currentPath != path)
            {
                loadMesh(path);
            }
        }

        /// <summary>
        ///     Carga una malla estatica de formato TGC
        /// </summary>
        private void loadMesh(string path)
        {
            currentPath = path;

            //Dispose de escena anterior
            if (currentScene != null)
            {
                currentScene.disposeAll();
            }

            //Cargar escena con herramienta TgcSceneLoader
            var loader = new TgcSceneLoader();
            currentScene = loader.loadSceneFromFile(path);

            //Ajustar camara en base al tamaño del objeto
            GuiController.Instance.RotCamera.targetObject(currentScene.BoundingBox);

            //Calcular cantidad de triangulos y texturas
            var triangleCount = 0;
            var verticesCount = 0;
            var texturesCount = 0;
            foreach (var mesh in currentScene.Meshes)
            {
                triangleCount += mesh.NumberTriangles;
                verticesCount += mesh.NumberVertices;
                texturesCount += mesh.RenderType == TgcMesh.MeshRenderType.VERTEX_COLOR ? 0 : mesh.DiffuseMaps.Length;
            }

            //UserVars
            GuiController.Instance.UserVars.setValue("Name", currentScene.SceneName);
            GuiController.Instance.UserVars.setValue("Meshes", currentScene.Meshes.Count);
            GuiController.Instance.UserVars.setValue("Textures", texturesCount);
            GuiController.Instance.UserVars.setValue("Triangles", triangleCount);
            GuiController.Instance.UserVars.setValue("Vertices", verticesCount);
            var size = currentScene.BoundingBox.calculateSize();
            GuiController.Instance.UserVars.setValue("SizeX", TgcParserUtils.printFloat(size.X));
            GuiController.Instance.UserVars.setValue("SizeY", TgcParserUtils.printFloat(size.Y));
            GuiController.Instance.UserVars.setValue("SizeZ", TgcParserUtils.printFloat(size.Z));
        }

        /// <summary>
        ///     Cambiar color de vertices de la malla
        /// </summary>
        /// <param name="color"></param>
        private void changeColor(Color color)
        {
            if (currentColor == null || currentColor != color)
            {
                currentColor = color;

                //Aplicar color a todas las mallas de la escena
                foreach (var mesh in currentScene.Meshes)
                {
                    mesh.setColor(color);
                }
            }
        }

        /// <summary>
        ///     Evento de clic en Reload
        /// </summary>
        private void Reload_ButtonClick(object sender, EventArgs e)
        {
            loadMesh(currentPath);
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio la malla
            var selectedPath = (string) GuiController.Instance.Modifiers["Mesh"];
            checkLoadMesh(selectedPath);

            //Ver si cambio el color
            var color = (Color) GuiController.Instance.Modifiers["Color"];
            changeColor(color);

            //Mostrar BoundingBox
            var showBoundingBox = (bool) GuiController.Instance.Modifiers["BoundingBox"];

            //AlphaBlending
            var alphaBlending = (bool) GuiController.Instance.Modifiers["AlphaBlending"];
            if (alphaBlending != currentAlphaBlending)
            {
                currentAlphaBlending = alphaBlending;
                foreach (var mesh in currentScene.Meshes)
                {
                    mesh.AlphaBlendEnable = currentAlphaBlending;
                }
            }

            //Renderizar escena entera
            currentScene.renderAll(showBoundingBox);
        }

        public override void close()
        {
            currentScene.disposeAll();
        }
    }
}