using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
	/// <summary>
	///     Pared 3D plana que solo crece en dos dimensiones.
	/// </summary>
	public class TgcPlane : IRenderObject
	{
		/// <summary>
		///     Orientaciones posibles de la pared
		/// </summary>
		public enum Orientations
		{
			/// <summary>
			///     Pared vertical a lo largo de X
			/// </summary>
			XYplane = 0,

			/// <summary>
			///     Pared horizontal
			/// </summary>
			XZplane = 1,

			/// <summary>
			///     Pared vertical a lo largo de Z
			/// </summary>
			YZplane = 2
		}

		private readonly CustomVertex.PositionTextured[] vertices;
		private TGCVector3 origin;
		private TGCVector3 size;
		private TGCVector2 uvOffset;

		/// <summary>
		///     Crea una pared vacia.
		/// </summary>
		public TgcPlane()
		{
			vertices = new CustomVertex.PositionTextured[6];
			AutoAdjustUv = false;
			Enabled = true;
			BoundingBox = new TgcBoundingAxisAlignBox();
			UTile = 1;
			VTile = 1;
			AlphaBlendEnable = false;
			UVOffset = TGCVector2.Zero;

			//Shader
			Effect = TgcShaders.Instance.VariosShader;
			Technique = TgcShaders.T_POSITION_TEXTURED;
		}

		/// <summary>
		///     Crea una pared con un punto de origen, el tamaño de la pared y la orientación de la misma, especificando
		///     el tiling de la textura
		/// </summary>
		/// <param name="origin">Punto de origen de la pared</param>
		/// <param name="size">Dimensiones de la pared. Uno de los valores será ignorado, según la orientación elegida</param>
		/// <param name="orientation">Orientacion de la pared</param>
		/// <param name="texture">Textura de la pared</param>
		/// <param name="uTile">Cantidad de tile de la textura en coordenada U</param>
		/// <param name="vTile">Cantidad de tile de la textura en coordenada V</param>
		public TgcPlane(TGCVector3 origin, TGCVector3 size, Orientations orientation, TgcTexture texture, float uTile,
			float vTile)
			: this()
		{
			setTexture(texture);

			AutoAdjustUv = false;
			Origin = origin;
			Size = size;
			Orientation = orientation;
			UTile = uTile;
			VTile = vTile;

			updateValues();
		}

		/// <summary>
		///     Crea una pared con un punto de origen, el tamaño de la pared y la orientación de la misma, con ajuste automatico
		///     de coordenadas de textura
		/// </summary>
		/// <param name="origin">Punto de origen de la pared</param>
		/// <param name="size">Dimensiones de la pared. Uno de los valores será ignorado, según la orientación elegida</param>
		/// <param name="orientation">Orientacion de la pared</param>
		/// <param name="texture">Textura de la pared</param>
		public TgcPlane(TGCVector3 origin, TGCVector3 size, Orientations orientation, TgcTexture texture)
			: this()
		{
			setTexture(texture);

			AutoAdjustUv = true;
			Origin = origin;
			Size = size;
			Orientation = orientation;
			UTile = 1;
			VTile = 1;

			updateValues();
		}

		/// <summary>
		///     Origen de coordenadas de la pared.
		///     Llamar a updateValues() para aplicar cambios.
		/// </summary>
		public TGCVector3 Origin
		{
			get { return origin; }
			set { origin = value; }
		}

		/// <summary>
		///     Dimensiones de la pared.
		/// </summary>
		public TGCVector3 Size
		{
			get { return size; }
			set { size = value; }
		}

		/// <summary>
		///     Orientación de la pared.
		///     Llamar a updateValues() para aplicar cambios.
		/// </summary>
		public Orientations Orientation { get; set; }

		/// <summary>
		///     Textura de la pared
		/// </summary>
		public TgcTexture Texture { get; private set; }

		/// <summary>
		///     Shader del mesh
		/// </summary>
		public Effect Effect { get; set; }

		/// <summary>
		///     Technique que se va a utilizar en el effect.
		///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
		/// </summary>
		public string Technique { get; set; }

		/// <summary>
		///     Cantidad de tile de la textura en coordenada U.
		///     Llamar a updateValues() para aplicar cambios.
		/// </summary>
		public float UTile { get; set; }

		/// <summary>
		///     Cantidad de tile de la textura en coordenada V.
		///     Llamar a updateValues() para aplicar cambios.
		/// </summary>
		public float VTile { get; set; }

		/// <summary>
		///     Auto ajustar coordenadas UV en base a la relación de tamaño de la pared y la textura
		///     Llamar a updateValues() para aplicar cambios.
		/// </summary>
		public bool AutoAdjustUv { get; set; }

		/// <summary>
		///     Offset UV de textura
		/// </summary>
		public TGCVector2 UVOffset
		{
			get { return uvOffset; }
			set { uvOffset = value; }
		}

		/// <summary>
		///     Indica si la pared esta habilitada para ser renderizada
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		///     BoundingBox de la pared
		/// </summary>
		public TgcBoundingAxisAlignBox BoundingBox { get; }

		public TGCVector3 Position
		{
			get { return Origin; }
		}

		/// <summary>
		///     Habilita el renderizado con AlphaBlending para los modelos
		///     con textura o colores por vértice de canal Alpha.
		///     Por default está deshabilitado.
		/// </summary>
		public bool AlphaBlendEnable { get; set; }

		/// <summary>
		///     Renderizar la pared
		/// </summary>
		public void Render()
		{
			if (!Enabled)
				return;

			activateAlphaBlend();

			TexturesManager.Instance.shaderSet(Effect, "texDiffuseMap", Texture);
			TexturesManager.Instance.clear(1);
			TgcShaders.Instance.setShaderMatrixIdentity(Effect);
			D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionTextured;
			Effect.Technique = Technique;

			//Render con shader
			Effect.Begin(0);
			Effect.BeginPass(0);
			D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, vertices);
			Effect.EndPass();
			Effect.End();

			resetAlphaBlend();
		}

		/// <summary>
		///     Liberar recursos de la pared
		/// </summary>
		public void Dispose()
		{
			Texture.dispose();
		}

		/// <summary>
		///     Configurar punto minimo y maximo de la pared.
		///     Se ignora un valor de cada punto según la orientación elegida.
		///     Llamar a updateValues() para aplicar cambios.
		/// </summary>
		/// <param name="min">Min</param>
		/// <param name="max">Max</param>
		public void setExtremes(TGCVector3 min, TGCVector3 max)
		{
			Origin = min;
			Size = TGCVector3.Subtract(max, min);
		}

		/// <summary>
		///     Actualizar parámetros de la pared en base a los valores configurados
		/// </summary>
		public void updateValues()
		{
			float autoWidth;
			float autoHeight;

			//Calcular los 4 corners de la pared, segun el tipo de orientacion
			TGCVector3 bLeft, tLeft, bRight, tRight;
			if (Orientation == Orientations.XYplane)
			{
				bLeft = Origin;
				tLeft = new TGCVector3(Origin.X + Size.X, Origin.Y, Origin.Z);
				bRight = new TGCVector3(Origin.X, Origin.Y + Size.Y, Origin.Z);
				tRight = new TGCVector3(Origin.X + Size.X, Origin.Y + Size.Y, Origin.Z);

				autoWidth = Size.X / Texture.Width;
				autoHeight = Size.Y / Texture.Height;
			}
			else if (Orientation == Orientations.YZplane)
			{
				bLeft = Origin;
				tLeft = new TGCVector3(Origin.X, Origin.Y, Origin.Z + Size.Z);
				bRight = new TGCVector3(Origin.X, Origin.Y + Size.Y, Origin.Z);
				tRight = new TGCVector3(Origin.X, Origin.Y + Size.Y, Origin.Z + Size.Z);

				autoWidth = Size.Y / Texture.Width;
				autoHeight = Size.Z / Texture.Height;
			}
			else
			{
				bLeft = Origin;
				tLeft = new TGCVector3(Origin.X + Size.X, Origin.Y, Origin.Z);
				bRight = new TGCVector3(Origin.X, Origin.Y, Origin.Z + Size.Z);
				tRight = new TGCVector3(Origin.X + Size.X, Origin.Y, Origin.Z + Size.Z);

				autoWidth = Size.X / Texture.Width;
				autoHeight = Size.Z / Texture.Height;
			}

			//Auto ajustar UV
			if (AutoAdjustUv)
			{
				UTile = autoHeight;
				VTile = autoWidth;
			}
			var offsetU = UVOffset.X;
			var offsetV = UVOffset.Y;

			//Primer triangulo
			vertices[0] = new CustomVertex.PositionTextured(bLeft, offsetU + UTile, offsetV + VTile);
			vertices[1] = new CustomVertex.PositionTextured(tLeft, offsetU, offsetV + VTile);
			vertices[2] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);

			//Segundo triangulo
			vertices[3] = new CustomVertex.PositionTextured(bLeft, offsetU + UTile, offsetV + VTile);
			vertices[4] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);
			vertices[5] = new CustomVertex.PositionTextured(bRight, offsetU + UTile, offsetV);

			/*Versión con triángulos para el otro sentido
            //Primer triangulo
            vertices[0] = new CustomVertex.PositionTextured(tLeft, 0 * this.uTile, 1 * this.vTile);
            vertices[1] = new CustomVertex.PositionTextured(bLeft, 1 * this.uTile, 1 * this.vTile);
            vertices[2] = new CustomVertex.PositionTextured(bRight, 1 * this.uTile, 0 * this.vTile);

            //Segundo triangulo
            vertices[3] = new CustomVertex.PositionTextured(bRight, 1 * this.uTile, 0 * this.vTile);
            vertices[4] = new CustomVertex.PositionTextured(tRight, 0 * this.uTile, 0 * this.vTile);
            vertices[5] = new CustomVertex.PositionTextured(tLeft, 0 * this.uTile, 1 * this.vTile);
            */

			//BoundingBox
			BoundingBox.setExtremes(bLeft, tRight);
		}

		/// <summary>
		///     Configurar textura de la pared.
		/// </summary>
		public void setTexture(TgcTexture texture)
		{
			if (Texture != null)
			{
				Texture.dispose();
			}
			Texture = texture;
		}

		/// <summary>
		///     Activar AlphaBlending, si corresponde.
		/// </summary>
		protected void activateAlphaBlend()
		{
			if (AlphaBlendEnable)
			{
				D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
				D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
			}
		}

		/// <summary>
		///     Desactivar AlphaBlending.
		/// </summary>
		protected void resetAlphaBlend()
		{
			D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
			D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
		}

		/// <summary>
		///     Convierte la pared en un TgcMesh.
		/// </summary>
		/// <param name="meshName">Nombre de la malla que se va a crear</param>
		public TgcMesh toMesh(string meshName)
		{
			return TgcMesh.FromTGCPlane(meshName, this.vertices, this.Texture);
		}


		/// <summary>
		///     Crear un nuevo TGCPlane igual a este.
		/// </summary>
		/// <returns>TgcPlane clonado</returns>
		public TgcPlane clone()
		{
			var clonePlane = new TgcPlane();
			clonePlane.Origin = Origin;
			clonePlane.Size = Size;
			clonePlane.Orientation = Orientation;
			clonePlane.AutoAdjustUv = AutoAdjustUv;
			clonePlane.UTile = UTile;
			clonePlane.VTile = VTile;
			clonePlane.AlphaBlendEnable = AlphaBlendEnable;
			clonePlane.UVOffset = UVOffset;
			clonePlane.setTexture(Texture.Clone());

			updateValues();
			return clonePlane;
		}
	}
}