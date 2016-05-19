using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.MathExamples
{
    /// <summary>
    ///     Ejemplo EjemploTriangulosYPlanos:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Anexo matemática 3D
    ///     Muestra como obtener los triángulos de un TgcMesh y generar un plano y un vector normal por cada uno.
    ///     Luego muestra esos triángulos por pantalla junto con sus normales
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTriangulosYPlanos : TgcExample
    {
        private readonly Random random = new Random();
        private TgcMesh mesh;
        private List<TgcArrow> normals;
        private List<TgcQuad> planes;
        private List<TgcTriangle> triangles;

        public EjemploTriangulosYPlanos(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Math";
            Name = "Triangulos y Planos";
            Description =
                "Muestra como obtener los triángulos de un TgcMesh y generar un plano y un vector normal por cada uno.";
        }

        public override void Init()
        {
            //Cargar un mesh
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Obtener los vértices del mesh (esta operacion es lenta, copia de la GPU a la CPU, no hacer a cada rato)
            var vertices = mesh.getVertexPositions();

            //Iterar sobre todos los vertices y construir triangulos, normales y planos
            var triCount = vertices.Length / 3;
            triangles = new List<TgcTriangle>(triCount);
            normals = new List<TgcArrow>();
            planes = new List<TgcQuad>();
            for (var i = 0; i < triCount; i++)
            {
                //Obtenemos los 3 vertices del triangulo
                var a = vertices[i * 3];
                var b = vertices[i * 3 + 1];
                var c = vertices[i * 3 + 2];

                //Obtener normal del triangulo. El orden influye en si obtenemos el vector normal hacia adentro o hacia afuera del mesh
                var normal = Vector3.Cross(c - a, b - a);
                normal.Normalize();

                //Crear plano que contiene el triangulo a partir un vertice y la normal
                var plane = Plane.FromPointNormal(a, normal);

                //Calcular el centro del triangulo. Hay muchos tipos de centros para un triangulo (http://www.mathopenref.com/trianglecenters.html)
                //Aca calculamos el mas simple
                var center = Vector3.Scale(a + b + c, 1 / 3f);

                ///////////// Creacion de elementos para poder dibujar a pantalla (propios de este ejemplo) ///////////////

                //Crear un quad (pequeño plano) con la clase TgcQuad para poder dibujar el plano que contiene al triangulo
                var quad = new TgcQuad();
                quad.Center = center;
                quad.Normal = normal;
                quad.Color = adaptColorRandom(Color.DarkGreen);
                quad.Size = new Vector2(10, 10);
                quad.updateValues();
                planes.Add(quad);

                //Creamos una flecha con la clase TgcArrow para poder dibujar la normal (la normal la estiramos un poco para que se pueda ver)
                normals.Add(TgcArrow.fromDirection(center, Vector3.Scale(normal, 10f)));

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
            Modifiers.addBoolean("mesh", "mesh", true);
            Modifiers.addBoolean("triangles", "triangles", true);
            Modifiers.addBoolean("normals", "normals", true);
            Modifiers.addBoolean("planes", "planes", false);

            //Camera
            ((TgcRotationalCamera)Camara).targetObject(mesh.BoundingBox);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Draw mesh
            var drawMesh = (bool)Modifiers["mesh"];
            if (drawMesh)
            {
                mesh.render();
            }

            //Draw triangles (Ojo: renderizar triangulos asi es extremadamene lento, para algo eficiente se usa un VertexBuffer)
            var drawTriangles = (bool)Modifiers["triangles"];
            if (drawTriangles)
            {
                foreach (var t in triangles)
                {
                    t.render();
                }
            }

            //Draw normals
            var drawNormals = (bool)Modifiers["normals"];
            if (drawNormals)
            {
                foreach (var a in normals)
                {
                    a.render();
                }
            }

            //Draw planes
            var drawPlanes = (bool)Modifiers["planes"];
            if (drawPlanes)
            {
                foreach (var p in planes)
                {
                    p.render();
                }
            }

            FinalizarEscena();
        }

        public Color adaptColorRandom(Color c)
        {
            var r = random.Next(0, 150);
            return Color.FromArgb(FastMath.Min(c.R + r, 255), FastMath.Min(c.G + r, 255), FastMath.Min(c.B + r, 255));
        }

        public override void Close()
        {
            base.Close();

            mesh.dispose();
            foreach (var t in triangles)
            {
                t.dispose();
            }
            foreach (var a in normals)
            {
                a.dispose();
            }
            foreach (var p in planes)
            {
                p.dispose();
            }
        }
    }
}