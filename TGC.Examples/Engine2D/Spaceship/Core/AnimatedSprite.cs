using System;
using System.Drawing;
using TGC.Core.Direct3D;

namespace TGC.Examples.Engine2D.Spaceship.Core
{
    /// <summary>
    ///     Utilidad para Sprites 2D animados en forma de tile dentro de una textura
    /// </summary>
    public class AnimatedSprite : CustomSprite
    {
        private readonly int framesPerColumn;
        private readonly int framesPerRow;

        private readonly float textureHeight;
        private readonly float textureWidth;
        private readonly int totalFrames;
        private float animationTimeLenght;

        protected int currentFrame;
        private float currentTime;

        protected bool enabled;

        protected float frameRate;
        private Size frameSize;

        protected bool playing;

        /// <summary>
        ///     Crear un nuevo Sprite animado
        /// </summary>
        /// <param name="texturePath">path de la textura animada</param>
        /// <param name="frameSize">tamaño de un tile de la animacion</param>
        /// <param name="totalFrames">cantidad de frames que tiene la animacion</param>
        /// <param name="frameRate">velocidad en cuadros por segundo</param>
        public AnimatedSprite(string texturePath, Size frameSize, int totalFrames, float frameRate)
        {
            enabled = true;
            currentFrame = 0;
            this.frameSize = frameSize;
            this.totalFrames = totalFrames;
            currentTime = 0;
            playing = true;

            //Sprite
            Bitmap = new CustomBitmap(texturePath, D3DDevice.Instance.Device);

            //Calcular valores de frames de la textura
            textureWidth = Bitmap.Width;
            textureHeight = Bitmap.Height;
            framesPerColumn = (int)textureWidth / frameSize.Width;
            framesPerRow = (int)textureHeight / frameSize.Height;
            var realTotalFrames = framesPerRow * framesPerColumn;
            if (realTotalFrames > totalFrames)
            {
                throw new Exception(
                    "Error en AnimatedSprite. No coinciden la cantidad de frames y el tamaño de la textura: " +
                    totalFrames);
            }

            setFrameRate(frameRate);
        }

        /// <summary>
        ///     Indica si el sprite esta habilitado para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        ///     Arrancar o parar avance de animacion
        /// </summary>
        public bool Playing
        {
            get { return playing; }
            set { playing = value; }
        }

        /// <summary>
        ///     Velocidad de la animacion medida en cuadros por segundo.
        /// </summary>
        public float FrameRate
        {
            get { return frameRate; }
        }

        /// <summary>
        ///     Frame actual de la textura animada
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }

        /// <summary>
        ///     Cambiar la velocidad de animacion
        /// </summary>
        public void setFrameRate(float frameRate)
        {
            this.frameRate = frameRate;
            animationTimeLenght = totalFrames / frameRate;
        }

        /// <summary>
        ///     Actualizar frame de animacion
        /// </summary>
        public void update(float elapsedTime)
        {
            if (!enabled)
                return;

            //Avanzar tiempo
            if (playing)
            {
                currentTime += elapsedTime;
                if (currentTime > animationTimeLenght)
                {
                    //Reiniciar al llegar al final
                    currentTime = 0;
                }
            }

            //Obtener cuadro actual
            currentFrame = (int)(currentTime * frameRate);

            //Obtener rectangulo de dibujado de la textura para este frame
            var srcRect = new Rectangle();
            srcRect.Y = frameSize.Width * (currentFrame % framesPerRow);
            srcRect.Width = frameSize.Width;
            srcRect.X = frameSize.Height * (currentFrame % framesPerColumn);
            srcRect.Height = frameSize.Height;
            SrcRect = srcRect;
        }
    }
}