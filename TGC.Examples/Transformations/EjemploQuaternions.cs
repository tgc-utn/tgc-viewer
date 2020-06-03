using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
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

        private TGCMatrix baseScaleRotation;
        private TGCMatrix baseEulerTranslation;
        private TGCMatrix baseQuaternionTranslation;

        private TgcMesh eulerMesh;
        private TgcMesh quaternionMesh;

        public EjemploTGCQuaternions(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Transformations";
            Name = "Euler vs TGCQuaternion";
            Description = "Euler vs TGCQuaternion";
        }

        public override void Init()
        {
            var loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(MediaDir + "\\XWing\\xwing-TgcScene.xml");

            eulerMesh = scene.Meshes[0];
            quaternionMesh = scene.Meshes[0].clone("quat");

            baseEulerTranslation = TGCMatrix.Translation(new TGCVector3(-10.0f, 0.0f, 0.0f));
            baseQuaternionTranslation = TGCMatrix.Translation(new TGCVector3(10.0f, 0.0f, 0.0f));

            rotacionModifier = AddVertex3f("Rotacion", TGCVector3.Empty, new TGCVector3(360, 360, 360), TGCVector3.Empty);

            baseScaleRotation = TGCMatrix.Scaling(new TGCVector3(0.05f, 0.05f, 0.05f)) * TGCMatrix.RotationY(FastMath.PI_HALF);

            Camera.SetCamera(new TGCVector3(0f, 1f, -25f), new TGCVector3(0f, 1f, 200f));
        }

        public override void Update()
        {
            var rot = rotacionModifier.Value;
            rot.X = Geometry.DegreeToRadian(rot.X);
            rot.Y = Geometry.DegreeToRadian(rot.Y);
            rot.Z = Geometry.DegreeToRadian(rot.Z);

            //Rotacion Euler
            eulerMesh.Transform = baseScaleRotation *
                TGCMatrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z) *
                baseEulerTranslation;

            //Rotacion TGCQuaternion
            TGCQuaternion rotationX = TGCQuaternion.RotationAxis(new TGCVector3(1.0f, 0.0f, 0.0f), rot.X);
            TGCQuaternion rotationY = TGCQuaternion.RotationAxis(new TGCVector3(0.0f, 1.0f, 0.0f), rot.Y);
            TGCQuaternion rotationZ = TGCQuaternion.RotationAxis(new TGCVector3(0.0f, 0.0f, 1.0f), rot.Z);

            TGCQuaternion rotation = rotationX * rotationY * rotationZ;

            quaternionMesh.Transform = baseScaleRotation *
                TGCMatrix.RotationTGCQuaternion(rotation) *
                baseQuaternionTranslation;
        }

        public override void Render()
        {
            PreRender();

            eulerMesh.Render();
            quaternionMesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            eulerMesh.Dispose();
            quaternionMesh.Dispose();
        }
    }
}