using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Textures;

namespace TGC.Core._2D
{
    /// <summary>
    ///     Representa un Sprite 2D con transformaciones
    /// </summary>
    public class TgcSprite
    {
        private bool dirtyValues;

        protected Vector2 position;

        protected float rotation;

        protected Vector2 rotationCenter;

        protected Vector2 scaling;

        protected Vector2 scalingCenter;

        protected Matrix transformationMatrix;

        /// <summary>
        ///     Crear un nuevo Sprite
        /// </summary>
        public TgcSprite()
        {
            initialize();
        }

        /// <summary>
        ///     Region rectangular a dibujar de la textura
        /// </summary>
        public Rectangle SrcRect { get; set; }

        /// <summary>
        ///     Transformacion del Sprite
        /// </summary>
        public Matrix TransformationMatrix
        {
            get { return transformationMatrix; }
        }

        /// <summary>
        ///     Textura del Sprite.
        /// </summary>
        public TgcTexture Texture { get; set; }

        /// <summary>
        ///     Color del Sprite
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Posicion en 2D del Sprite
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Rotacion del Sprite, en radianes
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Centro de rotacion
        /// </summary>
        public Vector2 RotationCenter
        {
            get { return rotationCenter; }
            set
            {
                rotationCenter = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Centro de escalado
        /// </summary>
        public Vector2 ScalingCenter
        {
            get { return scalingCenter; }
            set
            {
                scalingCenter = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Factor de escalado en X e Y
        /// </summary>
        public Vector2 Scaling
        {
            get { return scaling; }
            set
            {
                scaling = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Habilitar sprite
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Cargar valores iniciales
        /// </summary>
        public void initialize()
        {
            //Matriz identidad
            transformationMatrix = Matrix.Identity;

            //Rectangulo vacio
            SrcRect = Rectangle.Empty;

            //Propiedades de transformacion default
            position = Vector2.Empty;
            scaling = new Vector2(1, 1);
            scalingCenter = Vector2.Empty;
            rotation = 0;
            rotationCenter = Vector2.Empty;

            Color = Color.White;
            Enabled = true;
            dirtyValues = false;
        }

        /// <summary>
        ///     Actualizar matriz de transformacion en base a todas las propiedades del Sprite
        /// </summary>
        public void updateTransformationMatrix()
        {
            transformationMatrix = Matrix.Transformation2D(scalingCenter, 0, scaling, rotationCenter, rotation, position);
        }

        /// <summary>
        ///     Renderizar Sprite
        /// </summary>
        public void render()
        {
            if (!Enabled)
                return;

            //Si hubo modificaciones de propiedades, actualizar matriz de transformacion
            if (dirtyValues)
            {
                updateTransformationMatrix();
            }

            TgcDrawer2D.Instance.drawSprite(this);
        }

        /// <summary>
        ///     Liberar recursos
        /// </summary>
        public void dispose()
        {
            Texture.dispose();
        }
    }
}