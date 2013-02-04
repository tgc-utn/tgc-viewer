using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;

namespace TgcViewer.Utils._2D
{
    /// <summary>
    /// Utilidad para Sprites 2D animados en forma de tile dentro de una textura
    /// </summary>
    public class TgcAnimatedSprite
    {
        Size frameSize;
        int totalFrames;
        float currentTime;
        float animationTimeLenght;
        int framesPerRow;
        int framesPerColumn;
        float textureWidth;
        float textureHeight;

        protected bool enabled;
        /// <summary>
        /// Indica si el sprite esta habilitado para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        protected bool playing;
        /// <summary>
        /// Arrancar o parar avance de animacion
        /// </summary>
        public bool Playing
        {
            get { return playing; }
            set { playing = value; }
        }

        TgcSprite sprite;
        /// <summary>
        /// Sprite con toda la textura a animar
        /// </summary>
        public TgcSprite Sprite
        {
            get { return sprite; }
        }

        protected float frameRate;
        /// <summary>
        /// Velocidad de la animacion medida en cuadros por segundo.
        /// </summary>
        public float FrameRate
        {
            get { return frameRate; }
        }

        protected int currentFrame;
        /// <summary>
        /// Frame actual de la textura animada
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }

        /// <summary>
        /// Posicion del sprite
        /// </summary>
        public Vector2 Position
        {
            get { return sprite.Position; }
            set { sprite.Position = value; }
        }

        /// <summary>
        /// Factor de escala en X e Y
        /// </summary>
        public Vector2 Scaling
        {
            get { return sprite.Scaling; }
            set { sprite.Scaling = value; }
        }

        /// <summary>
        /// Angulo de rotación en radianes
        /// </summary>
        public float Rotation
        {
            get { return sprite.Rotation; }
            set { sprite.Rotation = value; }
        }

        /// <summary>
        /// Crear un nuevo Sprite animado
        /// </summary>
        /// <param name="texturePath">path de la textura animada</param>
        /// <param name="frameSize">tamaño de un tile de la animacion</param>
        /// <param name="totalFrames">cantidad de frames que tiene la animacion</param>
        /// <param name="frameRate">velocidad en cuadros por segundo</param>
        public TgcAnimatedSprite(string texturePath, Size frameSize, int totalFrames, float frameRate)
        {
            this.enabled = true;
            this.currentFrame = 0;
            this.frameSize = frameSize;
            this.totalFrames = totalFrames;
            this.currentTime = 0;
            this.playing = true;

            //Crear textura
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture texture = TgcTexture.createTexture(d3dDevice, texturePath);

            //Sprite
            sprite = new TgcSprite();
            sprite.Texture = texture;

            //Calcular valores de frames de la textura
            textureWidth = texture.Width;
            textureHeight = texture.Height;
            framesPerColumn = (int)textureWidth / frameSize.Width;
            framesPerRow = (int)textureHeight / frameSize.Height;
            int realTotalFrames = framesPerRow * framesPerColumn;
            if (realTotalFrames > totalFrames)
            {
                throw new Exception("Error en AnimatedSprite. No coinciden la cantidad de frames y el tamaño de la textura: " + totalFrames);
            }

            setFrameRate(frameRate);
        }

        /// <summary>
        /// Cambiar la velocidad de animacion
        /// </summary>
        public void setFrameRate(float frameRate)
        {
            this.frameRate = frameRate;
            animationTimeLenght = (float)totalFrames / frameRate;
        }

        /// <summary>
        /// Actualizar frame de animacion
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
            Rectangle srcRect = new Rectangle();
            srcRect.Y = frameSize.Width * (currentFrame % framesPerRow);
            srcRect.Width = frameSize.Width;
            srcRect.X = frameSize.Height * (currentFrame % framesPerColumn);
            srcRect.Height = frameSize.Height;
            sprite.SrcRect = srcRect;
        }

        /// <summary>
        /// Renderizar Sprite.
        /// Se debe llamar primero a update().
        /// Sino se dibuja el ultimo estado actualizado.
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            //Dibujar sprite
            sprite.render();
        }

        /// <summary>
        /// Actualiza la animacion y dibuja el Sprite
        /// </summary>
        public void updateAndRender()
        {
            update();
            render();
        }

        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void dispose()
        {
            sprite.dispose();
        }

    }
}
