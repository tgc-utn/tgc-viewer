﻿using TGC.Core.Mathematica;

namespace TGC.Core.Shaders
{
    /// <summary>
    ///     Utilidades generales para shaders de post-procesado
    /// </summary>
    public abstract class TGCPostProcessingUtils
    {
        /// <summary>
        ///     Calcular 15 offsets y weights para usar en un shader de Gaussian Blur de doble pasada (horizontal y vertical)
        /// </summary>
        /// <param name="textureSize">Ancho o alto de la textura, segun si es para horizontal o vertical</param>
        /// <param name="deviation">desviacion standard para distribucion de Gauss</param>
        /// <param name="multiplier">escala del weight de Gauss</param>
        /// <param name="horizontal">
        ///     En true devuelve los valores de texCoordOffsets para hacer una pasada de shader horizontal,
        ///     sino devuelve para una pasada verticual
        /// </param>
        /// <param name="texCoordOffsets">offsets generados para hacer sampling en el shader</param>
        /// <param name="colorWeights">weights generados para multiplicar los valores de los samplers en el shader</param>
        public static void computeGaussianBlurSampleOffsets15(float textureSize, float deviation, float multiplier,
            bool horizontal,
            out TGCVector2[] texCoordOffsets, out float[] colorWeights)
        {
            texCoordOffsets = new TGCVector2[15];
            colorWeights = new float[15];

            var i = 0;
            var tu = 1.0f / textureSize;
            float offset;

            // Fill the center texel
            var weight = multiplier * FastMath.GaussianDistribution(0, 0, deviation);
            colorWeights[0] = weight;

            texCoordOffsets[0] = TGCVector2.Zero;

            // Fill the first half
            for (i = 1; i < 8; i++)
            {
                // Get the Gaussian intensity for this offset
                weight = multiplier * FastMath.GaussianDistribution(i, 0, deviation);
                offset = i * tu;
                if (horizontal)
                {
                    texCoordOffsets[i] = new TGCVector2(offset, 0);
                }
                else
                {
                    texCoordOffsets[i] = new TGCVector2(0, offset);
                }

                colorWeights[i] = weight;
            }

            // Mirror to the second half
            for (i = 8; i < 15; i++)
            {
                colorWeights[i] = colorWeights[i - 7];
                texCoordOffsets[i] = -texCoordOffsets[i - 7];
            }
        }

        /// <summary>
        ///     Calcular 16 offsets para hacer un down-sampling de 4x4
        /// </summary>
        /// <param name="textureWidth">Ancho de la textura original</param>
        /// <param name="textureHeight">Alto de la textura original</param>
        /// <returns>Offsets calculados</returns>
        public static TGCVector2[] computeDownScaleOffsets4x4(int textureWidth, int textureHeight)
        {
            var offsets = new TGCVector2[16];

            var tU = 1.0f / textureWidth;
            var tV = 1.0f / textureHeight;

            // Sample from the 16 surrounding points. Since the center point will be in
            // the exact center of 16 texels, a 0.5f offset is needed to specify a texel
            // center.
            var index = 0;
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    offsets[index] = new TGCVector2((x - 1.5f) * tU, (y - 1.5f) * tV);
                    index++;
                }
            }

            return offsets;
        }
    }
}