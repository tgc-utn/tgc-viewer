using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     EjemploVerticalWall.
    /// </summary>
    public class EjemploVerticalWall : TGCExampleViewer
    {
        private TgcTexture currentTexture;
        private TgcMesh mesh;
        private TgcPlaneWall wall;

        public EjemploVerticalWall(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "EjemploVerticalWall";
            Description = "EjemploVerticalWall";
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                MediaDir + "ModelosTgc\\Box\\");
            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(0.25f, 0.25f, 0.25f);

            Camara = new TgcFpsCamera(new Vector3(7.9711f, 11.7f, -32.5475f));

            Modifiers.addVertex3f("origin", new Vector3(-100, -100, -100), new Vector3(100, 100, 100),
                new Vector3(0, 0, 0));
            Modifiers.addVertex3f("dimension", new Vector3(-100, -100, -100), new Vector3(1000, 1000, 100),
                new Vector3(10, 10, 10));
            Modifiers.addInterval("orientation", new[] { "XY", "XZ", "YZ" }, 0);
            Modifiers.addVertex2f("tiling", new Vector2(0, 0), new Vector2(10, 10), new Vector2(1, 1));

            var texturePath = MediaDir + "Texturas\\Quake\\TexturePack2\\brick1_1.jpg";
            currentTexture = TgcTexture.createTexture(d3dDevice, texturePath);
            Modifiers.addTexture("texture", currentTexture.FilePath);

            updateWall();
        }

        public override void Update()
        {
            PreUpdate();
        }

        private void updateWall()
        {
            var origin = (Vector3)Modifiers["origin"];
            var dimension = (Vector3)Modifiers["dimension"];
            var tiling = (Vector2)Modifiers["tiling"];

            var orientation = (string)Modifiers["orientation"];
            TgcPlaneWall.Orientations or;
            if (orientation == "XY") or = TgcPlaneWall.Orientations.XYplane;
            else if (orientation == "XZ") or = TgcPlaneWall.Orientations.XZplane;
            else or = TgcPlaneWall.Orientations.YZplane;

            if (wall == null)
            {
                wall = new TgcPlaneWall(origin, dimension, or, currentTexture, tiling.X, tiling.Y);
            }
            else
            {
                //wall.updateValues(origin, dimension, or, tiling.X, tiling.Y);
                wall.updateValues();
            }
        }

        public override void Render()
        {
            PreRender();

            var d3dDevice = D3DDevice.Instance.Device;

            updateWall();

            var text = (string)Modifiers["texture"];
            if (text != currentTexture.FilePath)
            {
                currentTexture = TgcTexture.createTexture(d3dDevice, text);
                wall.setTexture(currentTexture);
            }

            wall.render();
            mesh.render();

            PostRender();
        }

        public override void Dispose()
        {
            wall.dispose();
        }
    }
}