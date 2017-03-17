using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una esfera 3D con la herramienta TgcSphere, cuyos parámetros
    ///     pueden ser modificados.
    ///     Autor: Daniela Kazarian
    /// </summary>
    public class Esfera : TGCExampleViewer
    {
        private string currentTexture;
        private TgcSphere sphere;
        private bool useTexture;

        public Esfera(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Geometry Basics";
            Name = "Esfera";
            Description =
                "Muestra como crear una  esfera 3D con la herramienta TgcSphere, cuyos parámetros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear esfera
            sphere = new TgcSphere();
            //No recomendamos utilizar AutoTransformEnable, con juegos complejos se pierde el control.
            sphere.AutoTransformEnable = true;
            currentTexture = null;

            //Modifiers para vararis sus parametros
            Modifiers.addEnum("base", typeof(TgcSphere.eBasePoly), TgcSphere.eBasePoly.ICOSAHEDRON);
            Modifiers.addBoolean("inflate", "yes", true);
            Modifiers.addInterval("level of detail", new object[] { 0, 1, 2, 3, 4 }, 2);
            Modifiers.addBoolean("edges", "show", false);
            Modifiers.addFloat("radius", 0, 100, 10);
            Modifiers.addVertex3f("position", new TGCVector3(-100, -100, -100), new TGCVector3(100, 100, 100),
                TGCVector3.Empty);
            Modifiers.addVertex3f("rotation", new TGCVector3(-180, -180, -180), new TGCVector3(180, 180, 180),
                TGCVector3.Empty);
            Modifiers.addBoolean("Use texture", "yes", true);
            Modifiers.addTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            Modifiers.addVertex2f("offset", new TGCVector2(-0.5f, -0.5f), new TGCVector2(0.9f, 0.9f), TGCVector2.Zero);
            Modifiers.addVertex2f("tiling", new TGCVector2(0.1f, 0.1f), new TGCVector2(4, 4), TGCVector2.One);

            Modifiers.addColor("color", Color.White);
            Modifiers.addBoolean("boundingsphere", "show", false);

            UserVars.addVar("Vertices");
            UserVars.addVar("Triangulos");

            Camara = new TgcRotationalCamera(TGCVector3.Empty, 50f, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        /// <summary>
        ///     Actualiza los parámetros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateSphere()
        {
            var bTexture = (bool)Modifiers["Use texture"];
            var color = (Color)Modifiers["color"];
            sphere.RenderEdges = (bool)Modifiers["edges"];
            sphere.Inflate = (bool)Modifiers["inflate"];
            sphere.BasePoly = (TgcSphere.eBasePoly)Modifiers.getValue("base");

            if (bTexture)
            {
                //Cambiar textura
                var texturePath = (string)Modifiers["texture"];
                if (texturePath != currentTexture || !useTexture || (sphere.RenderEdges && sphere.Color != color))
                {
                    currentTexture = texturePath;
                    sphere.setColor(color);
                    sphere.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
                }
            }
            else sphere.setColor(color);

            useTexture = bTexture;

            //Radio, posición y color
            sphere.Radius = (float)Modifiers["radius"];
            sphere.Position = (TGCVector3)Modifiers["position"];
            sphere.LevelOfDetail = (int)Modifiers["level of detail"];

            //Rotación, converitr a radianes
            var rotation = (TGCVector3)Modifiers["rotation"];
            sphere.Rotation = new TGCVector3(Geometry.DegreeToRadian(rotation.X), Geometry.DegreeToRadian(rotation.Y),
                Geometry.DegreeToRadian(rotation.Z));

            //Offset de textura
            sphere.UVOffset = (TGCVector2)Modifiers["offset"];

            //Tiling de textura
            sphere.UVTiling = (TGCVector2)Modifiers["tiling"];

            //Actualizar valores en la caja.
            sphere.updateValues();
        }

        public override void Render()
        {
            PreRender();

            //Actualizar parametros de la caja
            updateSphere();

            UserVars.setValue("Vertices", sphere.VertexCount);
            UserVars.setValue("Triangulos", sphere.TriangleCount);
            //Renderizar caja
            sphere.render();

            //Mostrar Boundingsphere de la caja
            var boundingsphere = (bool)Modifiers["boundingsphere"];
            if (boundingsphere)
            {
                sphere.BoundingSphere.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            sphere.dispose();
        }
    }
}