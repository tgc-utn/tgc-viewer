using TGC.Core.Utils;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Herramienta para aplicar Bilateral Filtering a una imagen.
    ///     Sirve para mejorar la generacion de un NormalMap.
    ///     Basado en: http://code.google.com/p/bilateralfilter/source/browse/trunk/BilateralFilter.cpp?r=3
    /// </summary>
    public class BilateralFiltering
    {
        private readonly float[] gaussSimilarity;
        private readonly float[,] image;
        private readonly float[,] kernelD;
        private readonly int kernelRadius;

        public BilateralFiltering(float[,] image, float sigmaD, float sigmaR)
        {
            this.image = image;
            var sigmaMax = (int)FastMath.Max(sigmaD, sigmaR);
            kernelRadius = (int)FastMath.Ceiling(2 * sigmaMax);

            var twoSigmaRSquared = 2 * sigmaR * sigmaR;

            var kernelSize = kernelRadius * 2 + 1;
            kernelD = new float[kernelSize, kernelSize];

            var center = (kernelSize - 1) / 2;
            for (var x = -center; x < -center + kernelSize; x++)
            {
                for (var y = -center; y < -center + kernelSize; y++)
                {
                    kernelD[x + center, y + center] = gauss(sigmaD, x, y);
                }
            }

            gaussSimilarity = new float[256];
            for (var i = 0; i < gaussSimilarity.Length; i++)
            {
                gaussSimilarity[i] = FastMath.Exp(-(i / twoSigmaRSquared));
            }

            //rimage = cvCloneImage(image);
        }

        /// <summary>
        ///     Aplicar filtro
        /// </summary>
        /// <returns>Imagen original con el filtro aplicado</returns>
        public float[,] runFilter()
        {
            for (var i = 0; i < image.GetLength(1); i++)
            {
                for (var j = 0; j < image.GetLength(0); j++)
                {
                    apply(i, j);
                }
            }
            return image;
        }

        private void apply(int i, int j)
        {
            //i=y j=x
            if (i > 0 && j > 0 && i < image.GetLength(1) && j < image.GetLength(0))
            {
                float sum = 0;
                float totalWeight = 0;
                var intensityCenter = image[i, j];

                var mMax = i + kernelRadius;
                var nMax = j + kernelRadius;
                float weight;

                for (var m = i - kernelRadius; m < mMax; m++)
                {
                    for (var n = j - kernelRadius; n < nMax; n++)
                    {
                        if (isInsideBoundaries(m, n))
                        {
                            var intensityKernelPos = image[m, n];
                            weight = getSpatialWeight(m, n, i, j) * similarity(intensityKernelPos, intensityCenter);
                            totalWeight += weight;
                            sum += weight * intensityKernelPos;
                        }
                    }
                }
                var newvalue = (int)FastMath.Floor(sum / totalWeight);

                image[i, j] = newvalue;
            }
        }

        private float gauss(float sigma, int x, int y)
        {
            return FastMath.Exp(-((x * x + y * y) / (2 * sigma * sigma)));
        }

        private bool isInsideBoundaries(int m, int n)
        {
            if (m > -1 && n > -1 && m < image.GetLength(1) && n < image.GetLength(0))
                return true;
            return false;
        }

        private float getSpatialWeight(int m, int n, int i, int j)
        {
            return kernelD[i - m + kernelRadius, j - n + kernelRadius];
        }

        private float similarity(float p, float s)
        {
            // this equals: Math.exp(-(( Math.abs(p-s)) /  2 * this->sigmaR * this->sigmaR));
            // but is precomputed to improve performance
            return gaussSimilarity[(int)FastMath.Abs(p - s)];
        }
    }
}