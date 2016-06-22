using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

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
    public class Esfera : TgcExample
    {
        private string currentTexture;
        private TgcSphere sphere;
        private bool useTexture;

        public Esfera(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "Crear Esfera 3D";
            Description =
                "Muestra como crear una  esfera 3D con la herramienta TgcSphere, cuyos parámetros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear esfera
            sphere = new TgcSphere();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            Modifiers.addEnum("base", typeof(TgcSphere.eBasePoly), TgcSphere.eBasePoly.ICOSAHEDRON);
            Modifiers.addBoolean("inflate", "yes", true);
            Modifiers.addInterval("level of detail", new object[] { 0, 1, 2, 3, 4 }, 2);
            Modifiers.addBoolean("edges", "show", false);
            Modifiers.addFloat("radius", 0, 100, 10);
            Modifiers.addVertex3f("position", new Vector3(-100, -100, -100), new Vector3(100, 100, 100),
                new Vector3(0, 0, 0));
            Modifiers.addVertex3f("rotation", new Vector3(-180, -180, -180), new Vector3(180, 180, 180),
                new Vector3(0, 0, 0));
            Modifiers.addBoolean("Use texture", "yes", true);
            Modifiers.addTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f), new Vector2(0, 0));
            Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4), new Vector2(1, 1));

            Modifiers.addColor("color", Color.White);
            Modifiers.addBoolean("boundingsphere", "show", false);

            UserVars.addVar("Vertices");
            UserVars.addVar("Triangulos");

            Camara = new TgcRotationalCamera(new Vector3(), 50f);
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
            sphere.Position = (Vector3)Modifiers["position"];
            sphere.LevelOfDetail = (int)Modifiers["level of detail"];

            //Rotación, converitr a radianes
            var rotation = (Vector3)Modifiers["rotation"];
            sphere.Rotation = new Vector3(Geometry.DegreeToRadian(rotation.X), Geometry.DegreeToRadian(rotation.Y),
                Geometry.DegreeToRadian(rotation.Z));

            //Offset de textura
            sphere.UVOffset = (Vector2)Modifiers["offset"];

            //Tiling de textura
            sphere.UVTiling = (Vector2)Modifiers["tiling"];

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