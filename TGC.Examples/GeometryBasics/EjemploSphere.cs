using Microsoft.DirectX.Direct3D;
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
    ///     Ejemplo esfera.
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una esfera 3D con la herramienta TgcSphere, cuyos parámetros
    ///     pueden ser modificados.
    ///     Autor: Daniela Kazarian
    /// </summary>
    public class EjemploEsfera : TGCExampleViewer
    {
        private TGCEnumModifier baseModifier;
        private TGCBooleanModifier inflateModifier;
        private TGCIntervalModifier levelOfDetailModifier;
        private TGCBooleanModifier edgesModifier;
        private TGCFloatModifier radiusModifier;
        private TGCVertex3fModifier positionModifier;
        private TGCVertex3fModifier rotationModifier;
        private TGCBooleanModifier useTextureModifier;
        private TGCTextureModifier textureModifier;
        private TGCVertex2fModifier offsetModifier;
        private TGCVertex2fModifier tilingModifier;
        private TGCColorModifier colorModifier;
        private TGCBooleanModifier boundingsphereModifier;

        private string currentTexture;
        private TGCSphere sphere;
        private bool useTexture;

        public EjemploEsfera(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Geometry Basics";
            Name = "Esfera";
            Description = "Muestra como crear una  esfera 3D con la herramienta TgcSphere, cuyos parámetros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear esfera
            sphere = new TGCSphere();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            baseModifier = AddEnum("base", typeof(TGCSphere.eBasePoly), TGCSphere.eBasePoly.ICOSAHEDRON);
            inflateModifier = AddBoolean("inflate", "yes", true);
            levelOfDetailModifier = AddInterval("level of detail", new object[] { 0, 1, 2, 3, 4 }, 2);
            edgesModifier = AddBoolean("edges", "show", false);
            radiusModifier = AddFloat("radius", 0, 100, 10);
            positionModifier = AddVertex3f("position", new TGCVector3(-100, -100, -100), new TGCVector3(100, 100, 100), TGCVector3.Empty);
            rotationModifier = AddVertex3f("rotation", new TGCVector3(-180, -180, -180), new TGCVector3(180, 180, 180), TGCVector3.Empty);
            useTextureModifier = AddBoolean("Use texture", "yes", true);
            textureModifier = AddTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            offsetModifier = AddVertex2f("offset", new TGCVector2(-0.5f, -0.5f), new TGCVector2(0.9f, 0.9f), TGCVector2.Zero);
            tilingModifier = AddVertex2f("tiling", new TGCVector2(0.1f, 0.1f), new TGCVector2(4, 4), TGCVector2.One);

            colorModifier = AddColor("color", Color.White);
            boundingsphereModifier = AddBoolean("boundingsphere", "show", false);

            UserVars.addVar("Vertices");
            UserVars.addVar("Triangulos");

            Camera = new TgcRotationalCamera(TGCVector3.Empty, 50f, Input);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        /// <summary>
        ///     Actualiza los parámetros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateSphere()
        {
            var bTexture = useTextureModifier.Value;
            var color = colorModifier.Value;
            sphere.RenderEdges = edgesModifier.Value;
            sphere.Inflate = inflateModifier.Value;
            sphere.BasePoly = (TGCSphere.eBasePoly)baseModifier.Value;

            if (bTexture)
            {
                //Cambiar textura
                var texturePath = textureModifier.Value;
                if (texturePath != currentTexture || !useTexture || (sphere.RenderEdges && sphere.Color != color))
                {
                    currentTexture = texturePath;
                    sphere.SetColor(color);
                    sphere.SetTexture(TGCTexture.CreateTexture(D3DDevice.Instance.Device, currentTexture));
                }
            }
            else sphere.SetColor(color);

            useTexture = bTexture;

            //Radio, posición y color
            sphere.Radius = radiusModifier.Value;
            sphere.Position = positionModifier.Value;
            sphere.LevelOfDetail = (int)levelOfDetailModifier.Value;

            //Rotación, converitr a radianes
            var rotation = rotationModifier.Value;
            sphere.Rotation = new TGCVector3(Geometry.DegreeToRadian(rotation.X), Geometry.DegreeToRadian(rotation.Y), Geometry.DegreeToRadian(rotation.Z));

            //Offset de textura
            sphere.UVOffset = offsetModifier.Value;

            //Tiling de textura
            sphere.UVTiling = tilingModifier.Value;

            //Actualizar valores en la caja.
            sphere.UpdateValues();
        }

        public override void Render()
        {
            PreRender();

            //Actualizar parametros de la caja
            updateSphere();

            UserVars.setValue("Vertices", sphere.VertexCount);
            UserVars.setValue("Triangulos", sphere.TriangleCount);

            //Renderizar esfera
            sphere.Transform = TGCMatrix.Scaling(sphere.Radius, sphere.Radius, sphere.Radius) * TGCMatrix.RotationYawPitchRoll(sphere.Rotation.Y, sphere.Rotation.X, sphere.Rotation.Z) * TGCMatrix.Translation(sphere.Position);
            sphere.Render();

            //Mostrar Boundingsphere de la caja
            var boundingsphere = boundingsphereModifier.Value;
            if (boundingsphere)
            {
                sphere.BoundingSphere.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            sphere.Dispose();
        }
    }
}