using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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
        private TGCBooleanModifier showNormalModifier;
        private TGCVertex2fModifier sizeModifier;
        private TGCVertex3fModifier normalModifier;
        private TGCVertex3fModifier centerModifier;
        private TGCColorModifier colorModifier;

        private TgcArrow normalArrow;
        private TGCQuad quad;

        public EjemploQuad(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Geometry Basics";
            Name = "Quad";
            Description =
                "Muestra como crear una cara rectanglar 3D (Quad) orientable en base a un vector normal. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear Quad vacio
            quad = new TGCQuad();

            //Modifiers para vararia sus parametros
            sizeModifier = AddVertex2f("size", TGCVector2.Zero, new TGCVector2(100, 100), new TGCVector2(20, 20));
            normalModifier = AddVertex3f("normal", new TGCVector3(-10, -10, -10), new TGCVector3(10, 10, 10), new TGCVector3(0, 1, 1));
            centerModifier = AddVertex3f("center", new TGCVector3(-10, -10, -10), new TGCVector3(10, 10, 10), TGCVector3.Empty);
            colorModifier = AddColor("color", Color.Coral);

            //Flecha para mostrar el sentido del vector normal
            normalArrow = new TgcArrow();
            showNormalModifier = AddBoolean("showNormal", "Show normal", true);

            Camara = new TgcRotationalCamera(new TGCVector3(0, 5, 0), 50f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            //Actualizar parametros del quad
            updateQuad();

            PostUpdate();
        }

        /// <summary>
        ///     Actualiza los parametros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateQuad()
        {
            var size = sizeModifier.Value;
            var normal = normalModifier.Value;
            var center = centerModifier.Value;
            var color = colorModifier.Value;

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
            if (showNormalModifier.Value)
            {
                normalArrow.PStart = quad.Center;
                normalArrow.PEnd = quad.Center + quad.Normal * 10;
                normalArrow.updateValues();
            }
        }

        public override void Render()
        {
            PreRender();

            quad.Render();

            if (showNormalModifier.Value)
            {
                normalArrow.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            quad.Dispose();
            normalArrow.Dispose();
        }
    }
}