using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Collision
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

        public EjemploComputeObb(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Collision";
            Name = "Compute OBB";
            Description =
                "Muestra como calcular un Oriented BoundingBox (OBB) a partir de una nueva aleatoria de puntos. Movimiento con mouse.";
        }

        public override void Init()
        {
            obb = new TgcObb();
            generateObb();
            generate = false;

            Modifiers.addButton("generate", "generate", random_clic);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public void random_clic(object sender, EventArgs args)
        {
            generate = true;
        }

        public override void Render()
        {
            PreRender();

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

            PostRender();
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
                var x = MIN_RAND + (float)rand.NextDouble() * (MAX_RAND - MIN_RAND);
                var y = MIN_RAND + (float)rand.NextDouble() * (MAX_RAND - MIN_RAND);
                var z = MIN_RAND + (float)rand.NextDouble() * (MAX_RAND - MIN_RAND);
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

        public override void Dispose()
        {
            obb.dispose();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].dispose();
            }
        }
    }
}