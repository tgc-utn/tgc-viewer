using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace TgcViewer.Utils.Particles
{
    /// <summary>
    /// Particula a ser emitida por el emisor de particulas
    /// </summary>
    public class Particle
    {
        /// <summary>
        /// Estructura de Vertice para Particula
        /// </summary>
        public struct ParticleVertex
        {
            public Vector3 Position;
            public float PointSize;
            public int Color;
        }

        /// <summary>
        /// FVF para formato de vertice de Particula
        /// </summary>
        public static readonly VertexElement[] ParticleVertexElements = new VertexElement[]
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                                    DeclarationMethod.Default,
                                    DeclarationUsage.Position, 0),
                                    
            new VertexElement(0, 12, DeclarationType.Float1,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.PointSize, 0),

            new VertexElement(0, 16, DeclarationType.Color,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.Color, 0),

            VertexElement.VertexDeclarationEnd 
        };

        /// <summary>
        /// Color default de particula
        /// </summary>
        public static readonly Color DEFAULT_COLOR = System.Drawing.Color.White;


        private float totalTimeToLive;
        /// <summary>
        /// Tiempo total de vida de la particula
        /// </summary>
        public float TotalTimeToLive
        {
            get { return totalTimeToLive; }
            set { totalTimeToLive = value; }
        }

        private float timeToLive;
        /// <summary>
        /// Tiempo que le queda de vida a la particula
        /// </summary>
        public float TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }

        private Vector3 speed;
        /// <summary>
        /// Velocidad de la particula
        /// </summary>
        public Vector3 Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        private ParticleVertex pointSprite;
        /// <summary>
        /// Vertice de la particula
        /// </summary>
        public ParticleVertex PointSprite
        {
            get { return pointSprite; }
        }
        
        /// <summary>
        /// Posicion de la particula
        /// </summary>
        public Vector3 Position
        {
            get { return pointSprite.Position; }
            set { pointSprite.Position = value;  }
        }

        /// <summary>
        /// Color de la particula
        /// </summary>
        public int Color
        {
            get { return pointSprite.Color; }
            set { pointSprite.Color = value; }
        }

        /// <summary>
        /// Tamaño de la particula
        /// </summary>
        public float PointSize
        {
            get { return pointSprite.PointSize; }
            set { pointSprite.PointSize = value; }
        }

        public Particle()
		{
            pointSprite = new ParticleVertex();
            pointSprite.Position = new Vector3(0, 0, 0);
            pointSprite.PointSize = 1.0f;
            pointSprite.Color = DEFAULT_COLOR.ToArgb();
		}


    }
}
