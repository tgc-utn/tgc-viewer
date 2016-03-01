using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Viewer.Utils._2D
{
    /// <summary>
    ///     Utilidad para Sprites 2D animados en forma de tile dentro de una textura
    /// </summary>
    public class TgcAnimatedSprite
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
        public TgcAnimatedSprite(string texturePath, Size frameSize, int totalFrames, float frameRate)
        {
            enabled = true;
            currentFrame = 0;
            this.frameSize = frameSize;
            this.totalFrames = totalFrames;
            currentTime = 0;
            playing = true;

            //Crear textura
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, texturePath);

            //Sprite
            Sprite = new TgcSprite();
            Sprite.Texture = texture;

            //Calcular valores de frames de la textura
            textureWidth = texture.Width;
            textureHeight = texture.Height;
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
        ///     Sprite con toda la textura a animar
        /// </summary>
        public TgcSprite Sprite { get; }

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
        ///     Posicion del sprite
        /// </summary>
        public Vector2 Position
        {
            get { return Sprite.Position; }
            set { Sprite.Position = value; }
        }

        /// <summary>
        ///     Factor de escala en X e Y
        /// </summary>
        public Vector2 Scaling
        {
            get { return Sprite.Scaling; }
            set { Sprite.Scaling = value; }
        }

        /// <summary>
        ///     Angulo de rotación en radianes
        /// </summary>
        public float Rotation
        {
            get { return Sprite.Rotation; }
            set { Sprite.Rotation = value; }
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
        public void update()
        {
            if (!enabled)
                return;

            //Avanzar tiempo
            if (playing)
            {
                currentTime += GuiController.Instance.ElapsedTime;
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
            Sprite.SrcRect = srcRect;
        }

        /// <summary>
        ///     Renderizar Sprite.
        ///     Se debe llamar primero a update().
        ///     Sino se dibuja el ultimo estado actualizado.
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            //Dibujar sprite
            Sprite.render();
        }

        /// <summary>
        ///     Actualiza la animacion y dibuja el Sprite
        /// </summary>
        public void updateAndRender()
        {
            update();
            render();
        }

        /// <summary>
        ///     Liberar recursos
        /// </summary>
        public void dispose()
        {
            Sprite.dispose();
        }
    }
}