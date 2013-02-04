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

namespace Examples.Transformations
{
    /// <summary>
    /// Euler vs Quaternions
    /// </summary>
    public class EjemploQuaternions : TgcExample
    {
        TgcBox boxEuler;
        TgcBox boxQuaternion;

        public override string getCategory()
        {
            return "Transformations";
        }

        public override string getName()
        {
            return "Euler vs Quaternion";
        }

        public override string getDescription()
        {
            return "Euler vs Quaternion";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            TgcTexture textureEuler = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg");
            boxEuler = TgcBox.fromSize(new Vector3(-50, 0, 0), new Vector3(50, 50, 50), textureEuler);

            TgcTexture textureQuat = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\paredMuyRugosa.jpg");
            boxQuaternion = TgcBox.fromSize(new Vector3(50, 0, 0), new Vector3(50, 50, 50), textureQuat);
            boxQuaternion.AutoTransformEnable = false;

            GuiController.Instance.Modifiers.addVertex3f("Rot-Euler", new Vector3(0, 0, 0), new Vector3(360, 360, 360), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("Rot-Quaternion", new Vector3(0, 0, 0), new Vector3(360, 360, 360), new Vector3(0, 0, 0));


            //Camara fija
            GuiController.Instance.RotCamera.Enable = false;
            GuiController.Instance.setCamera(new Vector3(0f, 1f, -160.2245f), new Vector3(0f, 1f, 839.7755f));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            //Rotación Euler
            Vector3 rotEuler = (Vector3)GuiController.Instance.Modifiers["Rot-Euler"];
            rotEuler.X = Geometry.DegreeToRadian(rotEuler.X);
            rotEuler.Y = Geometry.DegreeToRadian(rotEuler.Y);
            rotEuler.Z = Geometry.DegreeToRadian(rotEuler.Z);
            boxEuler.Rotation = rotEuler;


            //Rotación Quaternion
            Vector3 rotQuat = (Vector3)GuiController.Instance.Modifiers["Rot-Quaternion"];
            rotQuat.X = Geometry.DegreeToRadian(rotQuat.X);
            rotQuat.Y = Geometry.DegreeToRadian(rotQuat.Y);
            rotQuat.Z = Geometry.DegreeToRadian(rotQuat.Z);
            Quaternion q = Quaternion.RotationYawPitchRoll(rotQuat.Y, rotQuat.X, rotQuat.Z);
            boxQuaternion.Transform = Matrix.RotationQuaternion(q) * Matrix.Translation(boxQuaternion.Position);



            boxEuler.render();
            boxQuaternion.render();
        }

        public override void close()
        {
            boxEuler.dispose();
            boxQuaternion.dispose();
        }

    }
}
