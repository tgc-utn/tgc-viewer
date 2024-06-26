using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Muestra como crear un cilindro 3D con la herramienta TgcCylinder, cuyos parametros pueden ser modificados.
    ///     Movimiento con mouse.
    ///     Esta es una vieja version, es recomendado utilizar trasnformaciones en vez de regenerar los triangulos en render.
    /// </summary>
    public class EjemploCilindro : TGCExampleViewer
    {
        private TGCBooleanModifier boundingCylinderModifier;
        private TGCColorModifier colorModifier;
        private TGCIntModifier alphaModifier;
        private TGCTextureModifier textureModifier;
        private TGCBooleanModifier useTextureModifier;
        private TGCVertex3fModifier sizeModifier;
        private TGCVertex3fModifier positionModifier;
        private TGCVertex3fModifier rotationModifier;

        private string currentTexture;
        private TGCCylinder cylinder;

        public EjemploCilindro(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Geometry Basics";
            Name = "Cilindro";
            Description = "Muestra como crear un cilindro 3D con la herramienta TgcCylinder, cuyos parametros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            cylinder = new TGCCylinder(TGCVector3.Empty, 2, 4);

            cylinder.AlphaBlendEnable = true;

            boundingCylinderModifier = AddBoolean("boundingCylinder", "boundingCylinder", false);
            colorModifier = AddColor("color", Color.White);
            alphaModifier = AddInt("alpha", 0, 255, 255);
            textureModifier = AddTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            useTextureModifier = AddBoolean("useTexture", "useTexture", true);

            sizeModifier = AddVertex3f("size", new TGCVector3(-3, -3, 1), new TGCVector3(7, 7, 10), new TGCVector3(2, 2, 5));
            positionModifier = AddVertex3f("position", new TGCVector3(-20, -20, -20), new TGCVector3(20, 20, 20), TGCVector3.Empty);
            var angle = FastMath.TWO_PI;
            rotationModifier = AddVertex3f("rotation", new TGCVector3(-angle, -angle, -angle), new TGCVector3(angle, angle, angle), TGCVector3.Empty);

            Camera = new TgcRotationalCamera(Input);
        }

        public override void Update()
        {
            //  Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            var size = sizeModifier.Value;
            var position = positionModifier.Value;
            var rotation = rotationModifier.Value;

            var texturePath = textureModifier.Value;
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                cylinder.SetTexture(TGCTexture.CreateTexture(D3DDevice.Instance.Device, currentTexture));
            }

            cylinder.UseTexture = useTextureModifier.Value;

            cylinder.Position = position;
            cylinder.Rotation = rotation;
            cylinder.TopRadius = size.X;
            cylinder.BottomRadius = size.Y;
            cylinder.Length = size.Z;

            var alpha = alphaModifier.Value;
            var color = colorModifier.Value;
            cylinder.Color = Color.FromArgb(alpha, color);

            cylinder.UpdateValues();

            cylinder.Transform = TGCMatrix.Scaling(cylinder.Scale) * TGCMatrix.RotationYawPitchRoll(cylinder.Rotation.Y, cylinder.Rotation.X, cylinder.Rotation.Z) * TGCMatrix.Translation(cylinder.Position);

            if (boundingCylinderModifier.Value)
                cylinder.BoundingCylinder.Render();
            else
                cylinder.Render();

            PostRender();
        }

        public override void Dispose()
        {
            cylinder.Dispose();
        }
    }
}