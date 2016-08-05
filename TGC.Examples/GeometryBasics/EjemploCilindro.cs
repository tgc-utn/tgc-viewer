using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Muestra como crear un cilindro 3D con la herramienta TgcCylinder, cuyos parametros pueden ser modificados.
    ///     Movimiento con mouse.
    ///     Esta es una vieja version, es recomendado utilizar trasnformaciones en vez de regenerar los triangulos en render.
    /// </summary>
    public class EjemploCilindro : TGCExampleViewer
    {
        private string currentTexture;
        private TgcCylinder cylinder;

        public EjemploCilindro(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Geometry Basics";
            Name = "Cilindro";
            Description =
                "Muestra como crear un cilindro 3D con la herramienta TgcCylinder, cuyos parametros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            cylinder = new TgcCylinder(new Vector3(0, 0, 0), 2, 4);
            cylinder.AutoTransformEnable = true;

            cylinder.AlphaBlendEnable = true;

            Modifiers.addBoolean("boundingCylinder", "boundingCylinder", false);
            Modifiers.addColor("color", Color.White);
            Modifiers.addInt("alpha", 0, 255, 255);
            Modifiers.addTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            Modifiers.addBoolean("useTexture", "useTexture", true);

            Modifiers.addVertex3f("size", new Vector3(-3, -3, 1), new Vector3(7, 7, 10), new Vector3(2, 2, 5));
            Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20), new Vector3(0, 0, 0));
            var angle = FastMath.TWO_PI;
            Modifiers.addVertex3f("rotation", new Vector3(-angle, -angle, -angle), new Vector3(angle, angle, angle),
                new Vector3(0, 0, 0));

            Camara = new TgcRotationalCamera(Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            var modifiers = Modifiers;
            var size = (Vector3)modifiers.getValue("size");
            var position = (Vector3)modifiers.getValue("position");
            var rotation = (Vector3)modifiers.getValue("rotation");

            var texturePath = (string)modifiers.getValue("texture");
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                cylinder.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
            }

            cylinder.UseTexture = (bool)modifiers.getValue("useTexture");

            cylinder.Position = position;
            cylinder.Rotation = rotation;
            cylinder.TopRadius = size.X;
            cylinder.BottomRadius = size.Y;
            cylinder.Length = size.Z;

            var alpha = (int)modifiers.getValue("alpha");
            var color = (Color)modifiers.getValue("color");
            cylinder.Color = Color.FromArgb(alpha, color);

            cylinder.updateValues();

            if ((bool)modifiers.getValue("boundingCylinder"))
                cylinder.BoundingCylinder.render();
            else
                cylinder.render();

            PostRender();
        }

        public override void Dispose()
        {
            cylinder.dispose();
        }
    }
}