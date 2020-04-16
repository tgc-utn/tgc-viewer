using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo EjemploTriangulosYPlanos:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Anexo matematica 3D
    ///     Muestra como obtener los triangulos de un TgcMesh y generar un plano y un vector normal por cada uno.
    ///     Luego muestra esos triangulos por pantalla junto con sus normales
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTriangulosYPlanos : TGCExampleViewer
    {
        private TGCBooleanModifier meshModifier;
        private TGCBooleanModifier trianglesModifier;
        private TGCBooleanModifier normalsModifier;
        private TGCBooleanModifier planesModifier;

        private readonly Random random = new Random();
        private TgcMesh mesh;
        private List<TgcArrow> normals;
        private List<TGCQuad> planes;
        private List<TgcTriangle> triangles;

        public EjemploTriangulosYPlanos(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Mesh Examples";
            Name = "Obtener triangulos y planos";
            Description = "Muestra como obtener los triangulos de un TgcMesh y generar un plano y un vector normal por cada uno.";
        }

        public override void Init()
        {
            //Cargar un mesh
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Obtener los vértices del mesh (esta operacion es lenta, copia de la GPU a la CPU, no hacer a cada rato)
            var vertices = mesh.getVertexPositions();

            //Iterar sobre todos los vertices y construir triangulos, normales y planos
            var triCount = vertices.Length / 3;
            triangles = new List<TgcTriangle>(triCount);
            normals = new List<TgcArrow>();
            planes = new List<TGCQuad>();
            for (var i = 0; i < triCount; i++)
            {
                //Obtenemos los 3 vertices del triangulo, es importante saber como esta estructurado nuestro mesh.
                var a = vertices[i * 3];
                var b = vertices[i * 3 + 1];
                var c = vertices[i * 3 + 2];

                //Obtener normal del triangulo. El orden influye en si obtenemos el vector normal hacia adentro o hacia afuera del mesh
                var normal = TGCVector3.Cross(c - a, b - a);
                normal.Normalize();

                //Crear plano que contiene el triangulo a partir un vertice y la normal
                var plane = TGCPlane.FromPointNormal(a, normal);

                //Calcular el centro del triangulo. Hay muchos tipos de centros para un triangulo (http://www.mathopenref.com/trianglecenters.html)
                //Aca calculamos el mas simple
                var center = TGCVector3.Scale(a + b + c, 1 / 3f);

                ///////////// Creacion de elementos para poder dibujar a pantalla (propios de este ejemplo) ///////////////

                //Crear un quad (pequeno plano) con la clase TgcQuad para poder dibujar el plano que contiene al triangulo
                var quad = new TGCQuad();
                quad.Center = center;
                quad.Normal = normal;
                quad.Color = adaptColorRandom(Color.DarkGreen);
                quad.Size = new TGCVector2(10, 10);
                quad.updateValues();
                planes.Add(quad);

                //Creamos una flecha con la clase TgcArrow para poder dibujar la normal (la normal la estiramos un poco para que se pueda ver)
                normals.Add(TgcArrow.fromDirection(center, TGCVector3.Scale(normal, 10f)));

                //Creamos la clase TgcTriangle que es un helper para dibujar triangulos sueltos
                var t = new TgcTriangle();
                t.A = a;
                t.B = b;
                t.C = c;
                t.Color = adaptColorRandom(Color.Red);
                t.updateValues();
                triangles.Add(t);
            }

            //Modifiers
            meshModifier = AddBoolean("mesh", "mesh", true);
            trianglesModifier = AddBoolean("triangles", "triangles", true);
            normalsModifier = AddBoolean("normals", "normals", true);
            planesModifier = AddBoolean("planes", "planes", false);

            //Camara
            Camera = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            //Draw mesh
            var drawMesh = meshModifier.Value;
            if (drawMesh)
            {
                mesh.Render();
            }

            //Draw triangles (Ojo: renderizar triangulos asi es extremadamene lento, para algo eficiente se usa un VertexBuffer)
            var drawTriangles = trianglesModifier.Value;
            if (drawTriangles)
            {
                foreach (var t in triangles)
                {
                    t.Render();
                }
            }

            //Draw normals
            var drawNormals = normalsModifier.Value;
            if (drawNormals)
            {
                foreach (var a in normals)
                {
                    a.Render();
                }
            }

            //Draw planes
            var drawPlanes = planesModifier.Value;
            if (drawPlanes)
            {
                foreach (var p in planes)
                {
                    p.Render();
                }
            }

            PostRender();
        }

        public Color adaptColorRandom(Color c)
        {
            var r = random.Next(0, 150);
            return Color.FromArgb(FastMath.Min(c.R + r, 255), FastMath.Min(c.G + r, 255), FastMath.Min(c.B + r, 255));
        }

        public override void Dispose()
        {
            mesh.Dispose();
            foreach (var t in triangles)
            {
                t.Dispose();
            }
            foreach (var a in normals)
            {
                a.Dispose();
            }
            foreach (var p in planes)
            {
                p.Dispose();
            }
        }
    }
}