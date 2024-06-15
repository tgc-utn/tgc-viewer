using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;

namespace TGC.Core.Geometry
{
    /// <remarks> https://gamedev.stackexchange.com/questions/31308/algorithm-for-creating-spheres David Lively</remarks>
    public static class GeometryProvider
    {
        /// <summary> Divide cada triangulo en cuatro.</summary>
        /// <remarks>
        ///     i0
        ///     /  \
        ///     m02-m01
        ///     /  \ /  \
        ///     i2---m12---i1
        /// </remarks>
        /// <param name="vectors"></param>
        /// <param name="indices"></param>
        public static void Subdivide(List<TGCVector3> vectors, List<int> indices)
        {
            var midpointIndices = new Dictionary<string, int>();

            var newIndices = new List<int>(indices.Count * 4);

            for (var i = 0; i < indices.Count - 2; i += 3)
            {
                var i0 = indices[i];
                var i1 = indices[i + 1];
                var i2 = indices[i + 2];

                var m01 = GetMidpointIndex(midpointIndices, vectors, i0, i1);
                var m12 = GetMidpointIndex(midpointIndices, vectors, i1, i2);
                var m02 = GetMidpointIndex(midpointIndices, vectors, i2, i0);

                newIndices.AddRange(
                    new[]
                    {
                        i0, m01, m02,
                        i1, m12, m01,
                        i2, m02, m12,
                        m02, m01, m12
                    }
                );
            }

            indices.Clear();
            indices.AddRange(newIndices);
        }

        /// <summary>
        ///     Busca el indice del vertice que se encuentra entre los vertices i0 e i1, de no existir crea el vertice.
        /// </summary>
        /// <param name="midpointIndices"></param>
        /// <param name="vertices"></param>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <returns></returns>
        private static int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<TGCVector3> vertices, int i0,
            int i1)
        {
            var edgeKey = $"{Math.Min(i0, i1)}_{Math.Max(i0, i1)}";

            var midpointIndex = -1;

            if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
            {
                var v0 = vertices[i0];
                var v1 = vertices[i1];

                var midpoint = (v0 + v1) * 0.5f;

                if (vertices.Contains(midpoint))
                {
                    midpointIndex = vertices.IndexOf(midpoint);
                }
                else
                {
                    midpointIndex = vertices.Count;
                    vertices.Add(midpoint);
                }
            }

            return midpointIndex;
        }

        /// <summary>
        ///     Retorna la posicion de los vertices y los indices de un cubo.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public static void Cube(List<TGCVector3> vertices, List<int> indices)
        {
            vertices.AddRange(new[]
            {
                new TGCVector3(-0.65f, -0.65f, -0.65f),
                new TGCVector3(-0.65f, -0.65f, 0.65f),
                new TGCVector3(-0.65f, 0.65f, -0.65f),
                new TGCVector3(-0.65f, 0.65f, 0.65f),
                new TGCVector3(0.65f, -0.65f, -0.65f),
                new TGCVector3(0.65f, -0.65f, 0.65f),
                new TGCVector3(0.65f, 0.65f, -0.65f),
                new TGCVector3(0.65f, 0.65f, 0.65f)
            });
            indices.AddRange(new[]
            {
                0, 2, 6,
                0, 6, 4,
                7, 4, 6,
                7, 5, 4,
                7, 1, 5,
                7, 3, 1,
                0, 3, 2,
                0, 1, 3,
                0, 5, 1,
                0, 4, 5,
                7, 6, 2,
                7, 2, 3
            });
        }

        /// <summary>
        ///     Retorna la posicion de los vertices y los indices de un icosaedro regular (Un d20)
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public static void Icosahedron(List<TGCVector3> vertices, List<int> indices)
        {
            indices.AddRange(
                new[]
                {
                    0, 4, 1,
                    0, 9, 4,
                    9, 5, 4,
                    4, 5, 8,
                    4, 8, 1,
                    8, 10, 1,
                    8, 3, 10,
                    5, 3, 8,
                    5, 2, 3,
                    2, 7, 3,
                    7, 10, 3,
                    7, 6, 10,
                    7, 11, 6,
                    11, 0, 6,
                    0, 1, 6,
                    6, 1, 10,
                    9, 0, 11,
                    9, 11, 2,
                    9, 2, 5,
                    7, 2, 11
                });

            for (var i = 0; i < indices.Count; i++)
            {
                indices[i] += vertices.Count;
            }

            var x = 0.525731112119133606f;
            var z = 0.850650808352039932f;

            vertices.AddRange(
                new[]
                {
                    new TGCVector3(-x, 0f, z),
                    new TGCVector3(x, 0f, z),
                    new TGCVector3(-x, 0f, -z),
                    new TGCVector3(x, 0f, -z),
                    new TGCVector3(0f, z, x),
                    new TGCVector3(0f, z, -x),
                    new TGCVector3(0f, -z, x),
                    new TGCVector3(0f, -z, -x),
                    new TGCVector3(z, x, 0f),
                    new TGCVector3(-z, x, 0f),
                    new TGCVector3(z, -x, 0f),
                    new TGCVector3(-z, -x, 0f)
                }
            );
        }
    }
}
