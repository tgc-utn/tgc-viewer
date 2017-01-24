using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear una cara rectanglar 3D (Quad) orientable en base a un vector normal,
    ///     utilizando la herramienta TgcQuad.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploQuad : TGCExampleViewer
    {
        private TgcArrow normalArrow;
        private TgcQuad quad;

        public EjemploQuad(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Geometry Basics";
            Name = "Quad";
            Description =
                "Muestra como crear una cara rectanglar 3D (Quad) orientable en base a un vector normal. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear Quad vacio
            quad = new TgcQuad();

            //Modifiers para vararia sus parametros
            Modifiers.addVertex2f("size", TGCVector2.Empty, new TGCVector2(100, 100), new TGCVector2(20, 20));
            Modifiers.addVertex3f("normal", new TGCVector3(-10, -10, -10), new TGCVector3(10, 10, 10), new TGCVector3(0, 1, 1));
            Modifiers.addVertex3f("center", new TGCVector3(-10, -10, -10), new TGCVector3(10, 10, 10), TGCVector3.Empty);
            Modifiers.addColor("color", Color.Coral);

            //Flecha para mostrar el sentido del vector normal
            normalArrow = new TgcArrow();
            Modifiers.addBoolean("showNormal", "Show normal", true);

            Camara = new TgcRotationalCamera(new TGCVector3(), 50f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            //Actualizar parametros del quad
            updateQuad();
        }

        /// <summary>
        ///     Actualiza los parametros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateQuad()
        {
            var size = (TGCVector2)Modifiers["size"];
            var normal = (TGCVector3)Modifiers["normal"];
            var center = (TGCVector3)Modifiers["center"];
            var color = (Color)Modifiers["color"];

            //Cargar valores del quad.
            quad.Center = center;
            quad.Size = size;
            quad.Normal = normal;
            quad.Color = color;

            //Actualizar valors para hacerlos efectivos
            //Los quad actualizan el vertexBuffer, al ser solo 2 triangulos no tiene gran costo recalcular los valores,
            //pero es mas recomendado utilizar transformaciones
            quad.updateValues();

            //Actualizar valores de la flecha
            if ((bool)Modifiers["showNormal"])
            {
                normalArrow.PStart = quad.Center;
                normalArrow.PEnd = quad.Center + quad.Normal * 10;
                normalArrow.updateValues();
            }
        }

        public override void Render()
        {
            PreRender();

            quad.render();

            if ((bool)Modifiers["showNormal"])
            {
                normalArrow.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            quad.dispose();
            normalArrow.dispose();
        }
    }
}