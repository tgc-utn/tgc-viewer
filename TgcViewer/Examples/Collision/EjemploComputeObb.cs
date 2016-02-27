using System;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TGC.Core.Example;

namespace Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploComputeObb:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Oriented BoundingBox
    ///     Muestra como calcular un Oriented BoundingBox (OBB) a partir de una nueva aleatoria de puntos
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploComputeObb : TgcExample
    {
        private static readonly Random rand = new Random();
        private bool generate;
        private TgcObb obb;
        private Vector3[] points;
        private TgcBox[] vertices;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Compute OBB";
        }

        public override string getDescription()
        {
            return
                "Muestra como calcular un Oriented BoundingBox (OBB) a partir de una nueva aleatoria de puntos. Movimiento con mouse.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            obb = new TgcObb();
            generateObb();
            generate = false;

            GuiController.Instance.Modifiers.addButton("generate", "generate", random_clic);
        }

        public void random_clic(object sender, EventArgs args)
        {
            generate = true;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            if (generate)
            {
                generateObb();
                generate = false;
            }

            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].render();
            }

            obb.render();
        }

        /// <summary>
        ///     Crear nube de puntos aleatorios y luego computar el mejor OBB que los ajusta
        /// </summary>
        private void generateObb()
        {
            obb.dispose();
            obb = null;

            //Crear nube ed puntos
            var COUNT = 10;
            var MIN_RAND = -20f;
            var MAX_RAND = 20f;

            points = new Vector3[COUNT];
            for (var i = 0; i < points.Length; i++)
            {
                var x = MIN_RAND + (float) rand.NextDouble()*(MAX_RAND - MIN_RAND);
                var y = MIN_RAND + (float) rand.NextDouble()*(MAX_RAND - MIN_RAND);
                var z = MIN_RAND + (float) rand.NextDouble()*(MAX_RAND - MIN_RAND);
                points[i] = new Vector3(x, y, z);
            }

            //Computar mejor OBB
            obb = TgcObb.computeFromPoints(points);

            if (vertices != null)
            {
                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i].dispose();
                }
            }

            vertices = new TgcBox[points.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = TgcBox.fromSize(points[i], new Vector3(1, 1, 1), Color.White);
            }
        }

        public override void close()
        {
            obb.dispose();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].dispose();
            }
        }
    }
}