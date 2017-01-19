using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core.Mathematica;

namespace TGC.Examples.Engine2D.Spaceship.Core
{
    public class CustomSprite : IDisposable
    {
        public CustomSprite()
        {
            initialize();
        }

        #region Miembros de IDisposable

        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
            }
        }

        #endregion Miembros de IDisposable

        private void initialize()
        {
            //Set the identity matrix.
            TransformationMatrix = TGCMatrix.Identity;

            //Set an empty rectangle to indicate the entire bitmap.
            SrcRect = Rectangle.Empty;

            //Initialize transformation properties.
            position = TGCVector2.Empty;
            scaling = TGCVector2.One;
            scalingCenter = TGCVector2.Empty;
            rotation = 0;
            rotationCenter = TGCVector2.Empty;

            Color = Color.White;
        }

        private void UpdateTransformationMatrix()
        {
            TransformationMatrix = TGCMatrix.Transformation2D(scalingCenter, 0, scaling, rotationCenter, rotation, position);
        }

        #region Public members

        /// <summary>
        ///     The transformation matrix.
        /// </summary>
        public TGCMatrix TransformationMatrix { get; set; }

        /// <summary>
        ///     The source rectangle to be drawn from the bitmap.
        /// </summary>
        public Rectangle SrcRect { get; set; }

        /// <summary>
        ///     The linked bitmap for the sprite.
        /// </summary>
        public CustomBitmap Bitmap { get; set; }

        /// <summary>
        ///     The color of the sprite.
        /// </summary>
        public Color Color { get; set; }

        private TGCVector2 position;

        /// <summary>
        ///     The sprite position in the 2D plane.
        /// </summary>
        public TGCVector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                UpdateTransformationMatrix();
            }
        }

        private float rotation;

        /// <summary>
        ///     The angle of rotation in radians.
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                UpdateTransformationMatrix();
            }
        }

        private TGCVector2 rotationCenter;

        /// <summary>
        ///     The position of the centre of rotation
        /// </summary>
        public TGCVector2 RotationCenter
        {
            get { return rotationCenter; }
            set
            {
                rotationCenter = value;
                UpdateTransformationMatrix();
            }
        }

        private TGCVector2 scalingCenter;

        /// <summary>
        ///     The position of the centre of scaling
        /// </summary>
        public TGCVector2 ScalingCenter
        {
            get { return scalingCenter; }
            set
            {
                scalingCenter = value;
                UpdateTransformationMatrix();
            }
        }

        private TGCVector2 scaling;

        /// <summary>
        ///     The scaling factors in the x and y axes.
        /// </summary>
        public TGCVector2 Scaling
        {
            get { return scaling; }
            set
            {
                scaling = value;
                UpdateTransformationMatrix();
            }
        }

        #endregion Public members
    }
}