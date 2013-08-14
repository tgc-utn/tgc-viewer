using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

namespace TgcViewer.Utils.Shaders
{
    /// <summary>
    /// Utilidades generales para shaders de post-procesado
    /// </summary>
    public abstract class TgcPostProcessingUtils
    {

        /// <summary>
        /// Calcular 15 offsets y weights para usar en un shader de Gaussian Blur de doble pasada (horizontal y vertical)
        /// </summary>
        /// <param name="textureSize">Ancho o alto de la textura, segun si es para horizontal o vertical</param>
        /// <param name="deviation">desviacion standard para distribucion de Gauss</param>
        /// <param name="multiplier">escala del weight de Gauss</param>
        /// <param name="horizontal">En true devuelve los valores de texCoordOffsets para hacer una pasada de shader horizontal, 
        /// sino devuelve para una pasada verticual</param>
        /// <param name="texCoordOffsets">offsets generados para hacer sampling en el shader</param>
        /// <param name="colorWeights">weights generados para multiplicar los valores de los samplers en el shader</param>
        public static void computeGaussianBlurSampleOffsets15(float textureSize, float deviation, float multiplier, bool horizontal, 
            out Vector2[] texCoordOffsets, out float[] colorWeights)
        {
            texCoordOffsets = new Vector2[15];
            colorWeights = new float[15];

            int i = 0;
            float tu = 1.0f / textureSize;
            float offset;

            // Fill the center texel
            float weight = multiplier * FastMath.GaussianDistribution(0, 0, deviation);
            colorWeights[0] = weight;

            texCoordOffsets[0] = new Vector2(0, 0);

            // Fill the first half
            for (i = 1; i < 8; i++)
            {
                // Get the Gaussian intensity for this offset
                weight = multiplier * FastMath.GaussianDistribution(i, 0, deviation);
                offset = i * tu;
                if (horizontal)
                {
                    texCoordOffsets[i] = new Vector2(offset, 0);
                }
                else
                {
                    texCoordOffsets[i] = new Vector2(0, offset);
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
        /// Calcular 16 offsets para hacer un down-sampling de 4x4
        /// </summary>
        /// <param name="textureWidth">Ancho de la textura original</param>
        /// <param name="textureHeight">Alto de la textura original</param>
        /// <returns>Offsets calculados</returns>
        public static Vector2[] computeDownScaleOffsets4x4(int textureWidth, int textureHeight)
        {
            Vector2[] offsets = new Vector2[16];

            float tU = 1.0f / textureWidth;
            float tV = 1.0f / textureHeight;

            // Sample from the 16 surrounding points. Since the center point will be in
            // the exact center of 16 texels, a 0.5f offset is needed to specify a texel
            // center.
            int index = 0;
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    offsets[index] = new Vector2((x - 1.5f) * tU, (y - 1.5f) * tV);
                    index++;
                }
            }

            return offsets;
        }



    }
}
