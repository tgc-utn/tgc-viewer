using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils._2D
{
    /// <summary>
    /// Representa un Sprite 2D con transformaciones
    /// </summary>
    public class TgcSprite
    {
        Rectangle srcRect;
        /// <summary>
        /// Region rectangular a dibujar de la textura
        /// </summary>
        public Rectangle SrcRect
        {
            get { return srcRect; }
            set { srcRect = value; }
        }

        protected Matrix transformationMatrix;
        /// <summary>
        /// Transformacion del Sprite
        /// </summary>
        public Matrix TransformationMatrix
        {
            get { return transformationMatrix; }
        }

        TgcTexture texture;
        /// <summary>
        /// Textura del Sprite.
        /// </summary>
        public TgcTexture Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        Color color;
        /// <summary>
        /// Color del Sprite
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        protected Vector2 position;
        /// <summary>
        /// Posicion en 2D del Sprite
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { 
                position = value;
                dirtyValues = true;
            }
        }

        protected float rotation;
        /// <summary>
        /// Rotacion del Sprite, en radianes
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set { 
                rotation = value;
                dirtyValues = true;
            }
        }

        protected Vector2 rotationCenter;
        /// <summary>
        /// Centro de rotacion
        /// </summary>
        public Vector2 RotationCenter
        {
            get { return rotationCenter; }
            set { 
                rotationCenter = value;
                dirtyValues = true;
            }
        }

        protected Vector2 scalingCenter;
        /// <summary>
        /// Centro de escalado
        /// </summary>
        public Vector2 ScalingCenter
        {
            get { return scalingCenter; }
            set { 
                scalingCenter = value;
                dirtyValues = true;
            }
        }

        protected Vector2 scaling;
        /// <summary>
        /// Factor de escalado en X e Y
        /// </summary>
        public Vector2 Scaling
        {
            get { return scaling; }
            set { 
                scaling = value;
                dirtyValues = true;
            }
        }

        bool enabled;
        /// <summary>
        /// Habilitar sprite
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }


        bool dirtyValues;

        /// <summary>
        /// Crear un nuevo Sprite
        /// </summary>
        public TgcSprite()
        {
            initialize();
        }

        /// <summary>
        /// Cargar valores iniciales
        /// </summary>
        public void initialize()
        {
            //Matriz identidad
            transformationMatrix = Matrix.Identity;

            //Rectangulo vacio
            srcRect = Rectangle.Empty;

            //Propiedades de transformacion default
            position = Vector2.Empty;
            scaling = new Vector2(1, 1);
            scalingCenter = Vector2.Empty;
            rotation = 0;
            rotationCenter = Vector2.Empty;

            color = Color.White;
            enabled = true;
            dirtyValues = false;
        }

        /// <summary>
        /// Actualizar matriz de transformacion en base a todas las propiedades del Sprite
        /// 
        /// </summary>
        public void updateTransformationMatrix()
        {
            this.transformationMatrix = Matrix.Transformation2D(scalingCenter, 0, scaling, rotationCenter, rotation, position);
        }

        /// <summary>
        /// Renderizar Sprite
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            //Si hubo modificaciones de propiedades, actualizar matriz de transformacion
            if (dirtyValues)
            {
                updateTransformationMatrix();
            }

            GuiController.Instance.Drawer2D.drawSprite(this);
        }

        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void dispose()
        {
            texture.dispose();
        }
    }

}
