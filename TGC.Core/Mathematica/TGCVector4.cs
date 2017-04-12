using Microsoft.DirectX;
using System.Drawing;

namespace TGC.Core.Mathematica
{
	/// <summary>
	/// Describes and manipulates a vector in four-dimensional (4-D) space.
	/// </summary>
	public struct TGCVector4
	{
		/// <summary>
		/// Retrieves or sets the DirectX of a 4-D vector.
		/// </summary>
		private Vector4 dxVector4;

		/// <summary>
		/// Initializes a new instance of the Vector4 class.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		/// <param name="w">The w coordinate.</param>
		public TGCVector4(float x, float y, float z, float w)
		{
			this.dxVector4 = new Vector4(x, y, z, w);
		}

		/// <summary>
		/// Initializes a new instance of the TGCVector4 class.
		/// </summary>
		/// <param name="dxVector4">Vector4 from value.</param>
		public TGCVector4(Vector4 dxVector4)
		{
			this.dxVector4 = dxVector4;
		}

		/// <summary>
		/// Retrieves or sets the x component of a 4-D vector.
		/// </summary>
		public float X
		{
			get { return this.dxVector4.X; }
			set { this.dxVector4.X = value; }
		}

		/// <summary>
		/// Retrieves or sets the y component of a 4-D vector.
		/// </summary>
		public float Y
		{
			get { return this.dxVector4.Y; }
			set { this.dxVector4.Y = value; }
		}

		/// <summary>
		/// Retrieves or sets the y component of a 4-D vector.
		/// </summary>
		public float Z
		{
			get { return this.dxVector4.Z; }
			set { this.dxVector4.Z = value; }
		}

		/// <summary>
		/// Retrieves or sets the y component of a 4-D vector.
		/// </summary>
		public float W
		{
			get { return this.dxVector4.W; }
			set { this.dxVector4.W = value; }
		}

		/// <summary>
		/// Retrieves a 4-D vector (0,0,0,0).
		/// </summary>
		public static TGCVector4 Empty
		{
			get { return new TGCVector4(0f, 0f, 0f, 0f); }
		}

		/// <summary>
		/// Retrieves the DirectX of a 4-D vector
		/// </summary>
		/// <returns></returns>
		private Vector4 ToVector4()
		{
			return dxVector4;
		}

		/// <summary>
		/// Cast TGCVector4 to DX Vector4
		/// </summary>
		/// <param name="vector">TGCVector4 to become into Vector4</param>
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