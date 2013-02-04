using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;

namespace Examples.Engine2D
{
    public class Sprite
    {
        #region Public members

        Rectangle srcRect;


        Matrix transformationMatrix;

        /// <summary>
        /// The transformation matrix.
        /// </summary>
        public Matrix TransformationMatrix
        {
            get { return transformationMatrix; }
            
        }

        /// <summary>
        /// The source rectangle to be drawn from the bitmap.
        /// </summary>
        public Rectangle SrcRect
        {
            get { return srcRect; }
            set { srcRect = value; }
        }


        Bitmap bitmap;

        /// <summary>
        /// The linked bitmap for the sprite.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        Color color;

        /// <summary>
        /// The color of the sprite.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private Vector2 position;

        /// <summary>
        /// The sprite position in the 2D plane.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; UpdateTransformationMatrix(); }
        }

        private float rotation;

        /// <summary>
        /// The angle of rotation in radians.
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; UpdateTransformationMatrix(); }
        }

        private Vector2 rotationCenter;

        /// <summary>
        /// The position of the centre of rotation
        /// </summary>
        public Vector2 RotationCenter
        {
            get { return rotationCenter; }
            set { rotationCenter = value; UpdateTransformationMatrix(); }
        }

        private Vector2 scalingCenter;

        /// <summary>
        /// The position of the centre of scaling
        /// </summary>
        public Vector2 ScalingCenter
        {
            get { return scalingCenter; }
            set { scalingCenter = value; UpdateTransformationMatrix(); }
        }

        private Vector2 scaling;

        /// <summary>
        /// The scaling factors in the x and y axes.
        /// </summary>
        public Vector2 Scaling
        {
            get { return scaling; }
            set { scaling = value; UpdateTransformationMatrix(); }
        }


        #endregion

        public Sprite()
        {

            initialize();
        }

        private void initialize()
        {
            //Set the identity matrix.
            transformationMatrix = Matrix.Identity;

            //Set an empty rectangle to indicate the entire bitmap.
            srcRect = Rectangle.Empty;

            //Initialize transformation properties.
            position = new Vector2(0, 0);
            scaling = new Vector2(1, 1);
            scalingCenter = new Vector2(0, 0);
            rotation = 0;
            rotationCenter = new Vector2(0, 0);
            

            color = Color.White;

        }

        private void UpdateTransformationMatrix()
        {
            this.transformationMatrix = Matrix.Transformation2D(scalingCenter, 0, scaling, rotationCenter, rotation, position);
        }


    }
}
