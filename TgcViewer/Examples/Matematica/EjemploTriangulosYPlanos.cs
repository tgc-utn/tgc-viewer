using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Utils;

namespace Examples.Matematica
{
    /// <summary>
    /// Ejemplo EjemploTriangulosYPlanos:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Anexo matemática 3D
    /// 
    /// Muestra como obtener los triángulos de un TgcMesh y generar un plano y un vector normal por cada uno.
    /// Luego muestra esos triángulos por pantalla junto con sus normales
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploTriangulosYPlanos : TgcExample
    {

        List<TgcTriangle> triangles;
        List<TgcArrow> normals;
        List<TgcQuad> planes;
        TgcMesh mesh;
        Random random = new Random();


        public override string getCategory()
        {
            return "Matematica";
        }

        public override string getName()
        {
            return "Triangulos y Planos";
        }

        public override string getDescription()
        {
            return "Muestra como obtener los triángulos de un TgcMesh y generar un plano y un vector normal por cada uno.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar un mesh
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");
            mesh = scene.Meshes[0];


            //Obtener los vértices del mesh (esta operacion es lenta, copia de la GPU a la CPU, no hacer a cada rato)
            Vector3[] vertices = mesh.getVertexPositions();

            //Iterar sobre todos los vertices y construir triangulos, normales y planos
            int triCount = vertices.Length / 3;
            triangles = new List<TgcTriangle>(triCount);
            normals = new List<TgcArrow>();
            planes = new List<TgcQuad>();
            for (int i = 0; i < triCount; i++)
            {
                //Obtenemos los 3 vertices del triangulo
                Vector3 a = vertices[i * 3];
                Vector3 b = vertices[i * 3 + 1];
                Vector3 c = vertices[i * 3 + 2];

                //Obtener normal del triangulo. El orden influye en si obtenemos el vector normal hacia adentro o hacia afuera del mesh
                Vector3 normal = Vector3.Cross(c - a, b - a);
                normal.Normalize();

                //Crear plano que contiene el triangulo a partir un vertice y la normal
                Plane plane = Plane.FromPointNormal(a, normal);

                //Calcular el centro del triangulo. Hay muchos tipos de centros para un triangulo (http://www.mathopenref.com/trianglecenters.html)
                //Aca calculamos el mas simple
                Vector3 center = Vector3.Scale(a + b + c, 1 / 3f);




                ///////////// Creacion de elementos para poder dibujar a pantalla (propios de este ejemplo) ///////////////

                //Crear un quad (pequeño plano) con la clase TgcQuad para poder dibujar el plano que contiene al triangulo
                TgcQuad quad = new TgcQuad();
                quad.Center = center;
                quad.Normal = normal;
                quad.Color = adaptColorRandom(Color.DarkGreen);
                quad.Size = new Vector2(10, 10);
                quad.updateValues();
                planes.Add(quad);

                //Creamos una flecha con la clase TgcArrow para poder dibujar la normal (la normal la estiramos un poco para que se pueda ver)
                normals.Add(TgcArrow.fromDirection(center, Vector3.Scale(normal, 10f)));

                //Creamos la clase TgcTriangle que es un helper para dibujar triangulos sueltos
                TgcTriangle t = new TgcTriangle();
                t.A = a;
                t.B = b;
                t.C = c;
                t.Color = adaptColorRandom(Color.Red);
                t.updateValues();
                triangles.Add(t);
            }



            //Modifiers
            GuiController.Instance.Modifiers.addBoolean("mesh", "mesh", true);
            GuiController.Instance.Modifiers.addBoolean("triangles", "triangles", true);
            GuiController.Instance.Modifiers.addBoolean("normals", "normals", true);
            GuiController.Instance.Modifiers.addBoolean("planes", "planes", false);


            //Camera
            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);

        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Draw mesh
            bool drawMesh = (bool)GuiController.Instance.Modifiers["mesh"];
            if (drawMesh)
            {
                mesh.render();
            }

            //Draw triangles (Ojo: renderizar triangulos asi es extremadamene lento, para algo eficiente se usa un VertexBuffer)
            bool drawTriangles = (bool)GuiController.Instance.Modifiers["triangles"];
            if (drawTriangles)
            {
                foreach (TgcTriangle t in triangles)
                {
                    t.render();
                }
            }

            //Draw normals
            bool drawNormals = (bool)GuiController.Instance.Modifiers["normals"];
            if (drawNormals)
            {
                foreach (TgcArrow a in normals)
                {
                    a.render();
                }
            }

            //Draw planes
            bool drawPlanes = (bool)GuiController.Instance.Modifiers["planes"];
            if (drawPlanes)
            {
                foreach (TgcQuad p in planes)
                {
                    p.render();
                }
            }
            
        }

        public Color adaptColorRandom(Color c)
        {
            int r = random.Next(0, 150);
            return Color.FromArgb((int)FastMath.Min(c.R + r, 255), (int)FastMath.Min(c.G + r, 255), (int)FastMath.Min(c.B + r, 255));
        }

        public override void close()
        {
            mesh.dispose();
            foreach (TgcTriangle t in triangles)
            {
                t.dispose();
            }
            foreach (TgcArrow a in normals)
            {
                a.dispose();
            }
            foreach (TgcQuad p in planes)
            {
                p.dispose();
            }
        }

    }
}
