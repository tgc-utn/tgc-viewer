using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Lights
{
    /// <summary>
    /// Herramienta para aplicar Bilateral Filtering a una imagen.
    /// Sirve para mejorar la generacion de un NormalMap.
    /// Basado en: http://code.google.com/p/bilateralfilter/source/browse/trunk/BilateralFilter.cpp?r=3
    /// </summary>
    public class BilateralFiltering
    {

        float[,] image;
        int kernelRadius;
        float[,] kernelD;
        float[] gaussSimilarity;

        public BilateralFiltering(float[,] image, float sigmaD, float sigmaR)
        {
            this.image = image;
            int sigmaMax = (int)FastMath.Max(sigmaD, sigmaR);
            this.kernelRadius = (int)FastMath.Ceiling(2 * sigmaMax);


            float twoSigmaRSquared = 2 * sigmaR * sigmaR;

            int kernelSize = this.kernelRadius * 2 + 1;
            this.kernelD = new float[kernelSize, kernelSize];

            int center = (kernelSize - 1) / 2;
            for (int x = -center; x < -center + kernelSize; x++)
            {
                for (int y = -center; y < -center + kernelSize; y++)
                {
                    this.kernelD[x + center, y + center] = this.gauss(sigmaD, x, y);
                }
            }


            this.gaussSimilarity = new float[256];
            for (int i = 0; i < this.gaussSimilarity.Length; i++)
            {
                this.gaussSimilarity[i] = FastMath.Exp((float)-((i) / twoSigmaRSquared));
            }



            //rimage = cvCloneImage(image);
        }

        /// <summary>
        /// Aplicar filtro
        /// </summary>
        /// <returns>Imagen original con el filtro aplicado</returns>
        public float[,] runFilter()
        {
            for (int i = 0; i < image.GetLength(1); i++)
            {
                for (int j = 0; j < image.GetLength(0); j++)
                {
                    this.apply(i, j);
                }
            }
            return this.image;
        }

        private void apply(int i, int j)
        {
            //i=y j=x
            if (i > 0 && j > 0 && i < image.GetLength(1) && j < image.GetLength(0))
            {
                float sum = 0;
                float totalWeight = 0;
                float intensityCenter = this.image[i, j];


                int mMax = i + kernelRadius;
                int nMax = j + kernelRadius;
                float weight;

                for (int m = i - kernelRadius; m < mMax; m++)
                {
                    for (int n = j - kernelRadius; n < nMax; n++)
                    {
                        if (this.isInsideBoundaries(m, n))
                        {
                            float intensityKernelPos = this.image[m, n];
                            weight = this.getSpatialWeight(m, n, i, j) * this.similarity(intensityKernelPos, intensityCenter);
                            totalWeight += weight;
                            sum += (weight * intensityKernelPos);
                        }
                    }
                }
                int newvalue = (int)FastMath.Floor(sum / totalWeight);

                this.image[i, j] = newvalue;
            }
        }



        private float gauss(float sigma, int x, int y)
        {
            return FastMath.Exp(-((x * x + y * y) / (2 * sigma * sigma)));
        }

        private bool isInsideBoundaries(int m,int n) 
        {
            if (m > -1 && n > -1 && m < image.GetLength(1) && n < image.GetLength(0))
                    return true;
                else 
                    return false;
        }

        private float getSpatialWeight(int m, int n,int i,int j)
        {
                return kernelD[(int)(i-m + kernelRadius), (int)(j-n + kernelRadius)];
        }

        private float similarity(float p, float s)
        {
                // this equals: Math.exp(-(( Math.abs(p-s)) /  2 * this->sigmaR * this->sigmaR));
                // but is precomputed to improve performance
                return this.gaussSimilarity[(int)FastMath.Abs(p-s)];
        }

    }

}
