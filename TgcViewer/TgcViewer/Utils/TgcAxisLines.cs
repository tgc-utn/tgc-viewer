using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils
{
    /// <summary>
    /// Herramienta para visualizar los ejes cartesianos
    /// </summary>
    public class TgcAxisLines
    {
        const float AXIS_POS_OFFSET = 40;
        const float AXIS_POS_DISTANCE = 25;

        /// <summary>
        /// Vertices para dibujar los ejes cartesianos
        /// </summary>
        CustomVertex.PositionColored[] lineVertices =
        {
            new CustomVertex.PositionColored( 0.0f, 0.0f, 0.0f,  Color.Red.ToArgb() ),   // red = +x Axis
            new CustomVertex.PositionColored( 1.0f, 0.0f, 0.0f,  Color.Red.ToArgb() ),
            new CustomVertex.PositionColored( 0.0f, 0.0f, 0.0f,  Color.Green.ToArgb() ), // green = +y Axis
            new CustomVertex.PositionColored( 0.0f, 1.0f, 0.0f,  Color.Green.ToArgb() ),
            new CustomVertex.PositionColored( 0.0f, 0.0f, 1.0f,  Color.Blue.ToArgb() ),  // blue = +z Axis
            new CustomVertex.PositionColored( 0.0f, 0.0f, 0.0f,  Color.Blue.ToArgb() )
        };

        bool enable;
        /// <summary>
        /// Habilita/Deshabilita el dibujado de los ejes cartesianos
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        VertexBuffer vertexBuffer;

        
        public TgcAxisLines(Device d3dDevice)
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), lineVertices.Length,
                d3dDevice, Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            vertexBuffer.Created += new EventHandler(this.onVertexBufferCreate);
            onVertexBufferCreate(vertexBuffer, null);
        }

        private void onVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            buffer.SetData(lineVertices, 0, LockFlags.None);
        }

        /// <summary>
        /// Renderizar ejes segun posicion actual de la camara
        /// </summary>
        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Obtener World coordinate de la esquina inferior de la pantalla
            int w = d3dDevice.Viewport.Width;
            int h = d3dDevice.Viewport.Height;
            float sx = AXIS_POS_OFFSET;
            float sy = h - AXIS_POS_OFFSET;
            
            Matrix matProj = d3dDevice.Transform.Projection;
            Vector3 v = new Vector3();
            v.X = (((2.0f * sx) / w) - 1) / matProj.M11;
            v.Y = -(((2.0f * sy) / h) - 1) / matProj.M22;
            v.Z = 1.0f;

            //Transform the screen space into 3D space
            Matrix m = Matrix.Invert(d3dDevice.Transform.View);
            Vector3 rayDir = new Vector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33
                );
            Vector3 rayOrig = new Vector3(m.M41, m.M42, m.M43);
            Vector3 worldCoordPos = rayOrig + AXIS_POS_DISTANCE * rayDir;


            //Renderizar
            texturesManager.clear(0);
            texturesManager.clear(1);
            d3dDevice.Material = TgcD3dDevice.DEFAULT_MATERIAL;
            d3dDevice.Transform.World = Matrix.Translation(worldCoordPos);

            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);
            d3dDevice.DrawPrimitives(PrimitiveType.LineList, 0, 3);

            d3dDevice.Transform.World = Matrix.Identity;
        }

    }
}
