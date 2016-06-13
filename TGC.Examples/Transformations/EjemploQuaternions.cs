using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Euler vs Quaternions
    /// </summary>
    public class EjemploQuaternions : TgcExample
    {
        private TgcBox boxEuler;
        private TgcBox boxQuaternion;

        public EjemploQuaternions(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Transformations";
            Name = "Euler vs Quaternion";
            Description = "Euler vs Quaternion";
        }

        public override void Init()
        {
            var textureEuler = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg");
            boxEuler = TgcBox.fromSize(new Vector3(-50, 0, 0), new Vector3(50, 50, 50), textureEuler);

            var textureQuat = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\paredMuyRugosa.jpg");
            boxQuaternion = TgcBox.fromSize(new Vector3(50, 0, 0), new Vector3(50, 50, 50), textureQuat);
            boxQuaternion.AutoTransformEnable = false;

            Modifiers.addVertex3f("Rot-Euler", new Vector3(0, 0, 0), new Vector3(360, 360, 360), new Vector3(0, 0, 0));
            Modifiers.addVertex3f("Rot-Quaternion", new Vector3(0, 0, 0), new Vector3(360, 360, 360),
                new Vector3(0, 0, 0));

            Camara.setCamera(new Vector3(0f, 1f, -160.2245f), new Vector3(0f, 1f, 839.7755f));
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            //Rotación Euler
            var rotEuler = (Vector3)Modifiers["Rot-Euler"];
            rotEuler.X = Geometry.DegreeToRadian(rotEuler.X);
            rotEuler.Y = Geometry.DegreeToRadian(rotEuler.Y);
            rotEuler.Z = Geometry.DegreeToRadian(rotEuler.Z);
            boxEuler.Rotation = rotEuler;

            //Rotación Quaternion
            var rotQuat = (Vector3)Modifiers["Rot-Quaternion"];
            rotQuat.X = Geometry.DegreeToRadian(rotQuat.X);
            rotQuat.Y = Geometry.DegreeToRadian(rotQuat.Y);
            rotQuat.Z = Geometry.DegreeToRadian(rotQuat.Z);
            var q = Quaternion.RotationYawPitchRoll(rotQuat.Y, rotQuat.X, rotQuat.Z);
            boxQuaternion.Transform = Matrix.RotationQuaternion(q) * Matrix.Translation(boxQuaternion.Position);

            boxEuler.render();
            boxQuaternion.render();

            helperPostRender();
        }

        public override void Close()
        {
            base.Close();

            boxEuler.dispose();
            boxQuaternion.dispose();
        }
    }
}