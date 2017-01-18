using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Euler vs Quaternions
    /// </summary>
    public class EjemploQuaternions : TGCExampleViewer
    {
        private TgcBox boxEuler;
        private TgcBox boxQuaternion;

        public EjemploQuaternions(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Transformations";
            Name = "Euler vs Quaternion";
            Description = "Euler vs Quaternion";
        }

        public override void Init()
        {
            var textureEuler = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg");
            boxEuler = TgcBox.fromSize(new TGCVector3(-50, 0, 0), new TGCVector3(50, 50, 50), textureEuler);

            var textureQuat = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\paredMuyRugosa.jpg");
            boxQuaternion = TgcBox.fromSize(new TGCVector3(50, 0, 0), new TGCVector3(50, 50, 50), textureQuat);
            boxQuaternion.AutoTransformEnable = false;

            Modifiers.addVertex3f("Rotacion", TGCVector3.Empty, new TGCVector3(360, 360, 360), TGCVector3.Empty);

            Camara.SetCamera(new TGCVector3(0f, 1f, -200f), new TGCVector3(0f, 1f, 500f));
        }

        public override void Update()
        {
            PreUpdate();

            var rot = (TGCVector3)Modifiers["Rotacion"];
            rot.X = Geometry.DegreeToRadian(rot.X);
            rot.Y = Geometry.DegreeToRadian(rot.Y);
            rot.Z = Geometry.DegreeToRadian(rot.Z);

            //Rotacion Euler
            boxEuler.Transform = TGCMatrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z) *
                                 TGCMatrix.Translation(boxEuler.Position);

            //Rotacion Quaternion
            var q = Quaternion.RotationYawPitchRoll(rot.Y, rot.X, rot.Z);
            boxQuaternion.Transform = TGCMatrix.RotationQuaternion(q) * TGCMatrix.Translation(boxQuaternion.Position);
        }

        public override void Render()
        {
            PreRender();

            boxEuler.render();
            boxQuaternion.render();

            PostRender();
        }

        public override void Dispose()
        {
            boxEuler.dispose();
            boxQuaternion.dispose();
        }
    }
}