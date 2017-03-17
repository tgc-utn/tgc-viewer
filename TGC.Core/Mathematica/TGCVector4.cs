using System;
using System.Drawing;
using Microsoft.DirectX;

namespace TGC.Core.Mathematica
{

    /// <summary>
    /// Describes and manipulates a vector in four-dimensional (4-D) space.
    /// </summary>
    public class TGCVector4
    {

        /// <summary>
        /// Initializes a new instance of the TGCVector4 class.
        /// </summary>
        /// <param name="dxVector4">Vector4 from value.</param>        
        public TGCVector4(Vector4 dxVector4)
        {
            this.DXVector4 = dxVector4;
        }


        /// <summary>
        /// Retrieves or sets the DirectX of a 4-D vector.
        /// </summary>
        private Vector4 DXVector4;

        /// <summary>
        /// Retrieves or sets the x component of a 4-D vector.
        /// </summary>
        public float X {
            get { return this.DXVector4.X; }
            set { this.DXVector4.X = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float Y {
            get { return this.DXVector4.Y; }
            set { this.DXVector4.Y = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float Z {
            get { return this.DXVector4.Z; }
            set { this.DXVector4.Z = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float W {
            get { return this.DXVector4.W; }
            set { this.DXVector4.W = value; }
        }

        /// <summary>
        /// Retrieves the DirectX of a 4-D vector
        /// </summary>
        /// <returns></returns>
        public Vector4 ToVector4()
        {
            return DXVector4;
        }

        /// <summary>
        /// Cast TGCMatrix to DX Matrix
        /// </summary>
        /// <param name="vector"></param>
        public static implicit operator Vector4(TGCVector4 vector)
        {
            return vector.ToVector4();
        }

        #region Old TGCVectorUtils

        /// <summary>
        ///     convierte un color base(255,255,255,255) a un Vector4(1f,1f,1f,1f).
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Vector4 ColorToVector4(Color color)
        {
            return Vector4.Normalize(new Vector4(color.R, color.G, color.B, color.A));
        }
               

        #endregion
    }
}
