using System.Drawing;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Trackball usando Quaternions
    /// </summary>
    public class EjemploTrackball : TGCExampleViewer
    {
        private readonly float radius = 10.0f;

        private TGCQuaternion stacked;
        private TGCMatrix baseSharkTransform;

        private TgcBoundingSphere boundingSphere;

        private TGCSphere sphere;
        private TgcMesh shark;

        private TgcPickingRay pickingRay;

        private TGCVector3 fromDrag;
        private TGCVector3 normalizedFrom;
        private TGCVector3 currentPick;
        private bool dragging;

        private TgcArrow fromArrow;
        private TgcArrow toArrow;
        private TgcArrow crossArrow;

        public EjemploTrackball(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Transformations";
            Name = "Trackball usando Quaternion";
            Description = "Trackball usando Quaternion";
        }

        public override void Init()
        {
            stacked = TGCQuaternion.Identity;
            fromDrag = TGCVector3.Empty;
            dragging = false;
            currentPick = TGCVector3.Empty;

            InitializeSphere();
            InitializeShark();
            InitializeArrows();

            pickingRay = new TgcPickingRay(Input);

            Camera.SetCamera(new TGCVector3(20f, 0f, 20f), new TGCVector3(-1f, 0f, -1f));
        }

        public override void Update()
        {
            currentPick = PerformPicking();

            if (PickingInsideSphere())
            {
                if (dragging)
                {
                    TGCVector3 normalizedTo = currentPick;
                    normalizedTo.Normalize();

                    var cross = TGCVector3.Cross(normalizedFrom, normalizedTo);
                    var newRotation = TGCQuaternion.RotationAxis(cross, FastMath.Acos(TGCVector3.Dot(normalizedFrom, normalizedTo)));
                    var currentRotation = TGCQuaternion.Multiply(stacked, newRotation);

                    shark.Transform = baseSharkTransform * TGCMatrix.RotationTGCQuaternion(currentRotation);

                    if (Input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        stacked = currentRotation;
                        shark.Transform = baseSharkTransform * TGCMatrix.RotationTGCQuaternion(stacked);
                        dragging = false;
                    }
                }
                else if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    dragging = true;
                    fromDrag = currentPick;
                    normalizedFrom = TGCVector3.Normalize(fromDrag);
                }
            }

            UpdateArrows();
        }

        public override void Render()
        {
            PreRender();
            shark.Render();
            fromArrow.Render();
            toArrow.Render();
            crossArrow.Render();
            sphere.Render();
            PostRender();
        }

        public override void Dispose()
        {
            boundingSphere.Dispose();
            sphere.Dispose();
            shark.Dispose();
        }

        private bool PickingInsideSphere()
        {
            return currentPick.LengthSq() > 0.0f;
        }

        private TGCVector3 PerformPicking()
        {
            pickingRay.updateRay();
            var ray = pickingRay.Ray;
            float distance = 0.0f;
            TGCVector3 point = TGCVector3.Empty;
            TgcCollisionUtils.intersectRaySphere(ray, boundingSphere, out distance, out point);
            return point;
        }

        private void InitializeShark()
        {
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "Aquatic\\Meshes\\shark-TgcScene.xml");
            shark = scene.Meshes[0];

            TGCVector3 rotationDirection = TGCVector3.Up;
            rotationDirection.Normalize();
            float angle = FastMath.QUARTER_PI / 2.0f;

            baseSharkTransform = TGCMatrix.Scaling(new TGCVector3(0.08f, 0.08f, 0.08f)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(0.0f, FastMath.Sin(angle), 0.0f, FastMath.Cos(angle)));

            shark.Transform = baseSharkTransform;
        }

        private void InitializeSphere()
        {
            var transparentTexture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\transparent.png");

            boundingSphere = new TgcBoundingSphere(TGCVector3.Empty, radius);

            sphere = new TGCSphere(1.0f, transparentTexture, TGCVector3.Empty);
            sphere.AlphaBlendEnable = true;
            sphere.UpdateValues();
            sphere.Transform = TGCMatrix.Scaling(new TGCVector3(radius, radius, radius));
        }

        private void InitializeArrows()
        {
            fromArrow = new TgcArrow();
            fromArrow.BodyColor = Color.DarkBlue;
            fromArrow.HeadColor = Color.Blue;
            fromArrow.HeadSize = new TGCVector2(0.2f, 0.2f);
            fromArrow.Thickness = 0.1f;
            fromArrow.PStart = TGCVector3.Empty;

            toArrow = new TgcArrow();
            toArrow.BodyColor = Color.DarkRed;
            toArrow.HeadColor = Color.Red;
            toArrow.HeadSize = new TGCVector2(0.2f, 0.2f);
            toArrow.Thickness = 0.1f;
            toArrow.PStart = TGCVector3.Empty;

            crossArrow = new TgcArrow();
            crossArrow.BodyColor = Color.DarkGoldenrod;
            crossArrow.HeadColor = Color.Yellow;
            crossArrow.HeadSize = new TGCVector2(0.2f, 0.2f);
            crossArrow.Thickness = 0.1f;
            crossArrow.PStart = TGCVector3.Empty;
        }

        private void UpdateArrows()
        {
            if (dragging)
            {
                toArrow.PEnd = currentPick;
                toArrow.updateValues();

                var cross = TGCVector3.Cross(fromDrag, currentPick);
                cross.Normalize();
                cross.Scale(-radius);
                crossArrow.PEnd = cross;
                crossArrow.updateValues();
            }
            else
            {
                fromArrow.PEnd = currentPick;
                fromArrow.updateValues();

                toArrow.PEnd = TGCVector3.Empty;
                toArrow.updateValues();

                toArrow.PEnd = TGCVector3.Empty;
                toArrow.updateValues();
            }
        }
    }
}