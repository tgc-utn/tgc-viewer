using System.Collections.Generic;
using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     Influencias de huesos sobre un vertice.
    ///     Un vertice puede estar influenciado por mas de un hueso
    /// </summary>
    public class TgcSkeletalVertexWeight
    {
        public TgcSkeletalVertexWeight()
        {
            Weights = new List<BoneWeight>();
        }

        /// <summary>
        ///     Influencias del vertice
        /// </summary>
        public List<BoneWeight> Weights { get; }

        /// <summary>
        ///     Convierte los weights del vertice en dos Vector4, uno con los valores de los weights y otro con los indices de los
        ///     huesos.
        ///     Si tiene menos de 4 weights completa con 0.
        /// </summary>
        /// <param name="vector4">Valores de Weights</param>
        /// <param name="vector4_2">Indices de huesos</param>
        public void createVector4WeightsAndIndices(out Vector4 blendWeights, out Vector4 blendIndices)
        {
            blendWeights = new Vector4(
                getBlendWeight(0),
                getBlendWeight(1),
                getBlendWeight(2),
                getBlendWeight(3)
                );

            blendIndices = new Vector4(
                getBlendIndex(0),
                getBlendIndex(1),
                getBlendIndex(2),
                getBlendIndex(3)
                );
        }

        /// <summary>
        ///     Obtener indice de hueso para el weight numero n.
        ///     Devuelve 0 si no hay
        /// </summary>
        private int getBlendIndex(int n)
        {
            return n < Weights.Count ? Weights[n].Bone.Index : 0;
        }

        /// <summary>
        ///     Obtener valor de influencia para el weight numero n.
        ///     Devuelve 0 si no hay
        /// </summary>
        private float getBlendWeight(int n)
        {
            return n < Weights.Count ? Weights[n].Weight : 0;
        }

        /// <summary>
        ///     Influencia de un hueso sobre un vertice
        /// </summary>
        public class BoneWeight
        {
            public BoneWeight(TgcSkeletalBone bone, float weight)
            {
                Bone = bone;
                Weight = weight;
            }

            /// <summary>
            ///     Hueso que influye
            /// </summary>
            public TgcSkeletalBone Bone { get; set; }

            /// <summary>
            ///     Influencia del hueso sobre el vertice. Valor normalizado entre 0 y 1
            /// </summary>
            public float Weight { get; set; }

            /// <summary>
            ///     Comparador segun weight
            /// </summary>
            public class GreaterComparer : IComparer<BoneWeight>
            {
                public int Compare(BoneWeight x, BoneWeight y)
                {
                    return x.Weight >= y.Weight ? 1 : -1;
                }
            }
        }
    }
}