using System.Collections.Generic;
using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Optimization.KdtTree
{
    /// <summary>
    ///     Ejemplo EjemploKdTree
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimizacion - KD-Tree
    ///     Muestra como crear y utilizar una KD-Tree para optimizar el renderizado de un escenario por Frustum Culling.
    ///     El escenario es una isla con palmeras, rocas y el suelo. Solo las palmeras y rocas se optimizan con esta tecnica.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploKdTree : TGCExampleViewer
    {
        private TGCBooleanModifier showKdTreeModifier;
        private TGCBooleanModifier showTerrainModifier;

        private KdTree kdtree;
        private List<TgcMesh> objetosIsla;
        private TGCSkyBox skyBox;
        private TgcMesh terreno;

        public EjemploKdTree(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Optimization";
            Name = "KdTree";
            Description = "Muestra como crear y utilizar una KD-Tree para optimizar el renderizado de un escenario por Frustum Culling.";
        }

        public override void Init()
        {
            //Crear SkyBox
            skyBox = new TGCSkyBox();
            skyBox.Center = new TGCVector3(0, 500, 0);
            skyBox.Size = new TGCVector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox.SetFaceTexture(TGCSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.SetFaceTexture(TGCSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.SetFaceTexture(TGCSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.SetFaceTexture(TGCSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            skyBox.SetFaceTexture(TGCSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.SetFaceTexture(TGCSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            skyBox.Init();

            //Cargar escenario de Isla
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "Isla\\Isla-TgcScene.xml");

            //Separar el Terreno del resto de los objetos
            var list1 = new List<TgcMesh>();
            scene.separeteMeshList(new[] { "Terreno" }, out list1, out objetosIsla);
            terreno = list1[0];

            //Crear KdTree
            kdtree = new KdTree();
            kdtree.create(objetosIsla, scene.BoundingBox);
            kdtree.createDebugKdTreeMeshes();

            //Camara en 1ra persona
            Camera = new TgcFpsCamera(new TGCVector3(1500, 800, 0), Input);

            showKdTreeModifier = AddBoolean("showKdTree", "Show KdTree", false);
            showTerrainModifier = AddBoolean("showTerrain", "Show Terrain", true);
        }

        public override void Update()
        {
            //  Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            var showKdTree = showKdTreeModifier.Value;
            var showTerrain = showTerrainModifier.Value;

            skyBox.Render();
            if (showTerrain)
            {
                terreno.Render();
            }
            kdtree.render(Frustum, showKdTree);

            PostRender();
        }

        public override void Dispose()
        {
            skyBox.Dispose();
            terreno.Dispose();
            foreach (var mesh in objetosIsla)
            {
                mesh.Dispose();
            }
        }
    }
}