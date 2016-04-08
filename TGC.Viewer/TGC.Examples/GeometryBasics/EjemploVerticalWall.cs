using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using EjemploDirectX.TgcViewer.Utils.TgcSceneLoader;

namespace Examples.GeometryBasics
{
    /// <summary>
    /// EjemploVerticalWall.
    /// </summary>
    public class EjemploVerticalWall : TgcExample
    {
        TgcPlaneWall wall;
        TgcMesh mesh;
        TgcTexture currentTexture;

        public override string getName()
        {
            return "EjemploVerticalWall";
        }

        public override string getDescription()
        {
            return "EjemploVerticalWall.";
        }

        

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                    GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                    GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\");
            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(0.25f, 0.25f, 0.25f);


            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(7.9711f, 11.7f, -32.5475f), new Vector3(7.972f, 11.4178f, -31.5475f));


            GuiController.Instance.Modifiers.addVertex3f("origin", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("dimension", new Vector3(-100, -100, -100), new Vector3(1000, 1000, 100), new Vector3(10, 10, 10));
            GuiController.Instance.Modifiers.addInterval("orientation", new string[] { "XY", "XZ", "YZ" }, 0);
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0,0), new Vector2(10,10),new Vector2(1,1));

            string texturePath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\brick1_1.jpg";
            currentTexture = TgcTexture.createTexture(d3dDevice, texturePath);
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture.FilePath);



            updateWall();
        
        }

        private void updateWall()
        {
            Vector3 origin = (Vector3)GuiController.Instance.Modifiers["origin"];
            Vector3 dimension = (Vector3)GuiController.Instance.Modifiers["dimension"];
            Vector2 tiling = (Vector2)GuiController.Instance.Modifiers["tiling"];

            string orientation = (string)GuiController.Instance.Modifiers["orientation"];
            TgcPlaneWall.Orientations or;
            if(orientation == "XY") or = TgcPlaneWall.Orientations.XYplane;
            else if(orientation == "XZ") or = TgcPlaneWall.Orientations.XZplane;
            else or = TgcPlaneWall.Orientations.YZplane;

            if (wall == null)
            {
                wall = new TgcPlaneWall(origin, dimension, or, currentTexture, tiling.X, tiling.Y);
            }
            else
            {
                wall.updateValues(origin, dimension, or, tiling.X, tiling.Y);
            }
            
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            updateWall();

            string text = (string)GuiController.Instance.Modifiers["texture"];
            if (text != currentTexture.FilePath)
            {
                currentTexture = TgcTexture.createTexture(d3dDevice, text);
                wall.setTexture(currentTexture);
            }

            wall.render();
            mesh.render();
        }

        public override void close()
        {
            wall.dispose();
        }

    }
}
