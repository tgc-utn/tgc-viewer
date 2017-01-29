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
            this.X = dxVector4.X;
            this.Y = dxVector4.Y;
            this.Z = dxVector4.Z;
            this.W = dxVector4.W;
            this.DXVector4 = dxVector4;
        }


        /// <summary>
        /// Retrieves or sets the DirectX of a 4-D vector.
        /// </summary>
        public Vector4 DXVector4 { get; set; }

        /// <summary>
        /// Retrieves or sets the x component of a 4-D vector.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float W { get; set; }

        /// <summary>
        /// Retrieves the DirectX of a 4-D vector
        /// </summary>
        /// <returns></returns>
        public Vector4 ToVector4()
        {
            return DXVector4;
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
