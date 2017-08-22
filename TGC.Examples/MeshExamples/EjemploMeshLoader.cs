using System;
using System.Drawing;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo EjemploMeshLoader:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Permite cargar una malla estatica de formato TGC desde el FileSystem.
    ///     Utiliza la herramienta TgcMeshLoader.
    ///     Esta herramienta crea un objeto TgcScene, compuesto a su vez por N TgcMesh
    ///     Cada uno representa una malla estatica.
    ///     La escena es cargada desde un archivo XML de formato TGC
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMeshLoader : TGCExampleViewer
    {
        private bool currentAlphaBlending;
        private Color currentColor;
        private string currentPath;
        private TgcScene currentScene;

        public EjemploMeshLoader(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Mesh Examples";
            Name = "Mesh Loader";
            Description = "Ejemplo de como cargar una Malla estatica en formato TGC.";
        }

        public override void Init()
        {
            //Malla default
            var initialMeshFile = MediaDir + "MeshCreator\\Meshes\\Vehiculos\\CamionDeAgua\\CamionDeAgua-TgcScene.xml";

            //Modifiers
            currentScene = null;
            currentPath = null;
            Modifiers.addFile("Mesh", initialMeshFile, "-TgcScene.xml |*-TgcScene.xml");

            Modifiers.addButton("Reload", "Reload", Reload_ButtonClick);

            currentColor = Color.White;
            Modifiers.addColor("Color", currentColor);

            Modifiers.addBoolean("BoundingBox", "BoundingBox", false);

            currentAlphaBlending = false;
            Modifiers.addBoolean("AlphaBlending", "AlphaBlending", currentAlphaBlending);

            //UserVars
            UserVars.addVar("Name");
            UserVars.addVar("Meshes");
            UserVars.addVar("Textures");
            UserVars.addVar("Triangles");
            UserVars.addVar("Vertices");
            UserVars.addVar("SizeX");
            UserVars.addVar("SizeY");
            UserVars.addVar("SizeZ");
        }

        public override void Update()
        {
            PreUpdate();
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

            //Ajustar camara en base al tamano del objeto
            Camara = new TgcRotationalCamera(currentScene.BoundingBox.calculateBoxCenter(),
                currentScene.BoundingBox.calculateBoxRadius() * 2, Input);

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
            UserVars.setValue("Name", currentScene.SceneName);
            UserVars.setValue("Meshes", currentScene.Meshes.Count);
            UserVars.setValue("Textures", texturesCount);
            UserVars.setValue("Triangles", triangleCount);
            UserVars.setValue("Vertices", verticesCount);
            var size = currentScene.BoundingBox.calculateSize();
            UserVars.setValue("SizeX", TgcParserUtils.printFloat(size.X));
            UserVars.setValue("SizeY", TgcParserUtils.printFloat(size.Y));
            UserVars.setValue("SizeZ", TgcParserUtils.printFloat(size.Z));
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

        public override void Render()
        {
            PreRender();

            //Ver si cambio la malla
            var selectedPath = (string)Modifiers["Mesh"];
            checkLoadMesh(selectedPath);

            //Ver si cambio el color
            var color = (Color)Modifiers["Color"];
            changeColor(color);

            //Mostrar BoundingBox
            var showBoundingBox = (bool)Modifiers["BoundingBox"];

            //AlphaBlending
            var alphaBlending = (bool)Modifiers["AlphaBlending"];
            if (alphaBlending != currentAlphaBlending)
            {
                currentAlphaBlending = alphaBlending;
                foreach (var mesh in currentScene.Meshes)
                {
                    mesh.AlphaBlend = currentAlphaBlending;
                }
            }

            //Renderizar escena entera
            currentScene.renderAll(showBoundingBox);

            PostRender();
        }

        public override void Dispose()
        {
            currentScene.disposeAll();
        }
    }
}