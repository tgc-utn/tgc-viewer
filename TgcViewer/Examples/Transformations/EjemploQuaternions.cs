using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Textures;
using TGC.Viewer;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Euler vs Quaternions
    /// </summary>
    public class EjemploQuaternions : TgcExample
    {
        private TgcBox boxEuler;
        private TgcBox boxQuaternion;

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
            var textureEuler = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg");
            boxEuler = TgcBox.fromSize(new Vector3(-50, 0, 0), new Vector3(50, 50, 50), textureEuler);

            var textureQuat = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\paredMuyRugosa.jpg");
            boxQuaternion = TgcBox.fromSize(new Vector3(50, 0, 0), new Vector3(50, 50, 50), textureQuat);
            boxQuaternion.AutoTransformEnable = false;

            GuiController.Instance.Modifiers.addVertex3f("Rot-Euler", new Vector3(0, 0, 0), new Vector3(360, 360, 360),
                new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("Rot-Quaternion", new Vector3(0, 0, 0),
                new Vector3(360, 360, 360), new Vector3(0, 0, 0));

            //Camara fija
            GuiController.Instance.RotCamera.Enable = false;
            GuiController.Instance.setCamera(new Vector3(0f, 1f, -160.2245f), new Vector3(0f, 1f, 839.7755f));
        }

        public override void render(float elapsedTime)
        {
            //Rotación Euler
            var rotEuler = (Vector3)GuiController.Instance.Modifiers["Rot-Euler"];
            rotEuler.X = Geometry.DegreeToRadian(rotEuler.X);
            rotEuler.Y = Geometry.DegreeToRadian(rotEuler.Y);
            rotEuler.Z = Geometry.DegreeToRadian(rotEuler.Z);
            boxEuler.Rotation = rotEuler;

            //Rotación Quaternion
            var rotQuat = (Vector3)GuiController.Instance.Modifiers["Rot-Quaternion"];
            rotQuat.X = Geometry.DegreeToRadian(rotQuat.X);
            rotQuat.Y = Geometry.DegreeToRadian(rotQuat.Y);
            rotQuat.Z = Geometry.DegreeToRadian(rotQuat.Z);
            var q = Quaternion.RotationYawPitchRoll(rotQuat.Y, rotQuat.X, rotQuat.Z);
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