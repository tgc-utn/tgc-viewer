using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear una caja 3D con la herramienta TgcBox, cuyos parametros
    ///     pueden ser modificados.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class Caja : TGCExampleViewer
    {
        private TgcBox box;
        private string currentTexture;

        public Caja(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "Crear Caja 3D";
            Description =
                "Muestra como crear una caja 3D con la herramienta TgcBox, cuyos parametros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear caja vacia
            box = new TgcBox();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            Modifiers.addVertex3f("size", new Vector3(0, 0, 0), new Vector3(100, 100, 100), new Vector3(20, 20, 20));
            Modifiers.addVertex3f("position", new Vector3(-100, -100, -100), new Vector3(100, 100, 100),
                new Vector3(0, 0, 0));
            Modifiers.addVertex3f("rotation", new Vector3(-180, -180, -180), new Vector3(180, 180, 180),
                new Vector3(0, 0, 0));
            Modifiers.addTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f), new Vector2(0, 0));
            Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4), new Vector2(1, 1));
            Modifiers.addColor("color", Color.White);
            Modifiers.addBoolean("boundingBox", "BoundingBox", false);

            Camara = new TgcRotationalCamera(new Vector3(), 50f, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        /// <summary>
        ///     Actualiza los parametros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateBox()
        {
            //Cambiar textura
            var texturePath = (string)Modifiers["texture"];
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                box.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
            }

            //Tamano, posicion y color
            box.Size = (Vector3)Modifiers["size"];
            box.Position = (Vector3)Modifiers["position"];
            box.Color = (Color)Modifiers["color"];

            //Rotacion, converitr a radianes
            var rotaion = (Vector3)Modifiers["rotation"];
            box.Rotation = new Vector3(Geometry.DegreeToRadian(rotaion.X), Geometry.DegreeToRadian(rotaion.Y),
                Geometry.DegreeToRadian(rotaion.Z));

            //Offset y Tiling de textura
            box.UVOffset = (Vector2)Modifiers["offset"];
            box.UVTiling = (Vector2)Modifiers["tiling"];

            //Actualizar valores en la caja.
            box.updateValues();
        }

        public override void Render()
        {
            PreRender();

            //Actualizar parametros de la caja
            updateBox();

            //Renderizar caja
            box.render();

            //Mostrar BoundingBox de la caja
            var boundingBox = (bool)Modifiers["boundingBox"];
            if (boundingBox)
            {
                box.BoundingBox.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            box.dispose();
        }
    }
}