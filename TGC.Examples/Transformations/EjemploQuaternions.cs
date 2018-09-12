using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Euler vs TGCQuaternions
    /// </summary>
    public class EjemploTGCQuaternions : TGCExampleViewer
    {
        private TGCVertex3fModifier rotacionModifier;

        private TGCBox boxEuler;
        private TGCBox boxTGCQuaternion;

        public EjemploTGCQuaternions(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Transformations";
            Name = "Euler vs TGCQuaternion";
            Description = "Euler vs TGCQuaternion";
        }

        public override void Init()
        {
            var textureEuler = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg");
            boxEuler = TGCBox.fromSize(new TGCVector3(-50, 0, 0), new TGCVector3(50, 50, 50), textureEuler);

            var textureQuat = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\paredMuyRugosa.jpg");
            boxTGCQuaternion = TGCBox.fromSize(new TGCVector3(50, 0, 0), new TGCVector3(50, 50, 50), textureQuat);
            boxTGCQuaternion.AutoTransformEnable = false;

            rotacionModifier = AddVertex3f("Rotacion", TGCVector3.Empty, new TGCVector3(360, 360, 360), TGCVector3.Empty);

            Camara.SetCamera(new TGCVector3(0f, 1f, -200f), new TGCVector3(0f, 1f, 500f));
        }

        public override void Update()
        {
            PreUpdate();

            var rot = rotacionModifier.Value;
            rot.X = Geometry.DegreeToRadian(rot.X);
            rot.Y = Geometry.DegreeToRadian(rot.Y);
            rot.Z = Geometry.DegreeToRadian(rot.Z);

            //Rotacion Euler
            boxEuler.Transform = TGCMatrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z) *
                                 TGCMatrix.Translation(boxEuler.Position);

            //Rotacion TGCQuaternion
            var q = TGCQuaternion.RotationYawPitchRoll(rot.Y, rot.X, rot.Z);
            boxTGCQuaternion.Transform = TGCMatrix.RotationTGCQuaternion(q) * TGCMatrix.Translation(boxTGCQuaternion.Position);

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            boxEuler.Render();
            boxTGCQuaternion.Render();

            PostRender();
        }

        public override void Dispose()
        {
            boxEuler.Dispose();
            boxTGCQuaternion.Dispose();
        }
    }
}