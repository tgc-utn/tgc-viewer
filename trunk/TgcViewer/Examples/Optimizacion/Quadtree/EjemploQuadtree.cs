using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Optimizacion.Quadtree
{
    /// <summary>
    /// Ejemplo EjemploQuadtree
    /// Unidades Involucradas:
    ///     # Unidad 7 - Optimizaci�n - Quadtree
    /// 
    /// Muestra como crear y utilizar una Quadtree para optimizar el renderizado de un escenario por Frustum Culling.
    /// El escenario es una isla con palmeras, rocas y el suelo. Solo las palmeras y rocas se optimizan con esta t�cnica.
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploQuadtree : TgcExample
    {

        TgcSkyBox skyBox;
        List<TgcMesh> objetosIsla;
        TgcMesh terreno;
        Quadtree quadtree;

        public override string getCategory()
        {
            return "Optimizacion";
        }

        public override string getName()
        {
            return "Quadtree";
        }

        public override string getDescription()
        {
            return "Muestra como crear y utilizar una Quadtree para optimizar el renderizado de un escenario por Frustum Culling.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            skyBox.updateValues();


            //Cargar escenario de Isla
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "Optimizacion\\Isla\\Isla-TgcScene.xml");

            //Separar el Terreno del resto de los objetos
            List<TgcMesh> list1 = new List<TgcMesh>();
            scene.separeteMeshList(new string[] { "Terreno" }, out list1, out objetosIsla);
            terreno = list1[0];

            //Crear Quadtree
            quadtree = new Quadtree();
            quadtree.create(objetosIsla, scene.BoundingBox);
            quadtree.createDebugQuadtreeMeshes();


            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1500, 800, 0), new Vector3(0, 0, -1));
            GuiController.Instance.FpsCamera.MovementSpeed = 500f;
            GuiController.Instance.FpsCamera.JumpSpeed = 500f;

            GuiController.Instance.Modifiers.addBoolean("showQuadtree", "Show Quadtree", false);
            GuiController.Instance.Modifiers.addBoolean("showTerrain", "Show Terrain", true);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            bool showQuadtree = (bool)GuiController.Instance.Modifiers["showQuadtree"];
            bool showTerrain = (bool)GuiController.Instance.Modifiers["showTerrain"];

            skyBox.render();
            if (showTerrain)
            {
                terreno.render();
            }
            quadtree.render(GuiController.Instance.Frustum, showQuadtree);
        }

        public override void close()
        {
            skyBox.dispose();
            terreno.dispose();
            foreach (TgcMesh mesh in objetosIsla)
            {
                mesh.dispose();
            }
        }

    }
}
