using System.Collections.Generic;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Example;

namespace Examples.Optimizacion.GrillaRegular
{
    /// <summary>
    ///     Ejemplo EjemploGrillaRegular
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimización - Grilla Regular
    ///     Muestra como crear y utilizar una Grilla Regular para optimizar el renderizado de un escenario por Frustum Culling.
    ///     El escenario es una isla con palmeras, rocas y el suelo. Solo las palmeras y rocas se optimizan con esta técnica.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploGrillaRegular : TgcExample
    {
        private GrillaRegular grilla;
        private List<TgcMesh> objetosIsla;
        private TgcSkyBox skyBox;
        private TgcMesh terreno;

        public override string getCategory()
        {
            return "Optimizacion";
        }

        public override string getName()
        {
            return "GrillaRegular";
        }

        public override string getDescription()
        {
            return
                "Muestra como crear y utilizar una Grilla Regular para optimizar el renderizado de un escenario por Frustum Culling.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            var texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            skyBox.updateValues();

            //Cargar escenario de Isla
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "Optimizacion\\Isla\\Isla-TgcScene.xml");

            //Separar el Terreno del resto de los objetos
            var list1 = new List<TgcMesh>();
            scene.separeteMeshList(new[] {"Terreno"}, out list1, out objetosIsla);
            terreno = list1[0];

            //Crear grilla
            grilla = new GrillaRegular();
            grilla.create(objetosIsla, scene.BoundingBox);
            grilla.createDebugMeshes();

            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1500, 800, 0), new Vector3(0, 0, -1));
            GuiController.Instance.FpsCamera.MovementSpeed = 500f;
            GuiController.Instance.FpsCamera.JumpSpeed = 500f;

            GuiController.Instance.Modifiers.addBoolean("showGrid", "Show Grid", false);
            GuiController.Instance.Modifiers.addBoolean("showTerrain", "Show Terrain", true);
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            var showGrid = (bool) GuiController.Instance.Modifiers["showGrid"];
            var showTerrain = (bool) GuiController.Instance.Modifiers["showTerrain"];

            skyBox.render();
            if (showTerrain)
            {
                terreno.render();
            }
            grilla.render(GuiController.Instance.Frustum, showGrid);
        }

        public override void close()
        {
            skyBox.dispose();
            terreno.dispose();
            foreach (var mesh in objetosIsla)
            {
                mesh.dispose();
            }
        }
    }
}