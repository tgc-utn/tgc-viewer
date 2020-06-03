using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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
        private TGCBooleanModifier alphaBlendingModifier;
        private TGCColorModifier colorModifier;
        private TGCBooleanModifier boundingBoxModifier;
        private TGCFileModifier meshModifier;
        private TGCButtonModifier reloadModifier;

        private bool currentAlphaBlending;
        private Color currentColor;
        private string currentPath;
        private TgcScene currentScene;

        public EjemploMeshLoader(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
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
            meshModifier = AddFile("Mesh", initialMeshFile, "-TgcScene.xml |*-TgcScene.xml");

            reloadModifier = AddButton("Reload", "Reload", Reload_ButtonClick);

            currentColor = Color.White;
            colorModifier = AddColor("Color", currentColor);

            boundingBoxModifier = AddBoolean("BoundingBox", "BoundingBox", false);

            currentAlphaBlending = false;
            alphaBlendingModifier = AddBoolean("AlphaBlending", "AlphaBlending", currentAlphaBlending);

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
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
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
                currentScene.DisposeAll();
            }

            //Cargar escena con herramienta TgcSceneLoader
            var loader = new TgcSceneLoader();
            currentScene = loader.loadSceneFromFile(path);

            //Ajustar camara en base al tamano del objeto
            Camera = new TgcRotationalCamera(currentScene.BoundingBox.calculateBoxCenter(), currentScene.BoundingBox.calculateBoxRadius() * 2, Input);

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
            var selectedPath = meshModifier.Value;
            checkLoadMesh(selectedPath);

            //Ver si cambio el color
            var color = colorModifier.Value;
            changeColor(color);

            //Mostrar BoundingBox
            var showBoundingBox = boundingBoxModifier.Value;

            //AlphaBlending
            var alphaBlending = alphaBlendingModifier.Value;
            if (alphaBlending != currentAlphaBlending)
            {
                currentAlphaBlending = alphaBlending;
                foreach (var mesh in currentScene.Meshes)
                {
                    mesh.AlphaBlendEnable = currentAlphaBlending;
                }
            }

            //Renderizar escena entera
            currentScene.RenderAll(showBoundingBox);

            PostRender();
        }

        public override void Dispose()
        {
            currentScene.DisposeAll();
        }
    }
}