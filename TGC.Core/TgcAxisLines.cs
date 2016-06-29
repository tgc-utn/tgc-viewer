using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Textures;

namespace TGC.Core
{
    /// <summary>
    ///     Herramienta para visualizar los ejes cartesianos
    /// </summary>
    public class TgcAxisLines
    {
        private const float AXIS_POS_OFFSET = 40;
        private const float AXIS_POS_DISTANCE = 25;

        /// <summary>
        ///     Vertices para dibujar los ejes cartesianos
        /// </summary>
        private readonly CustomVertex.PositionColored[] lineVertices =
        {
            new CustomVertex.PositionColored(0.0f, 0.0f, 0.0f, Color.Red.ToArgb()), // red = +x Axis
            new CustomVertex.PositionColored(1.0f, 0.0f, 0.0f, Color.Red.ToArgb()),
            new CustomVertex.PositionColored(0.0f, 0.0f, 0.0f, Color.Green.ToArgb()), // green = +y Axis
            new CustomVertex.PositionColored(0.0f, 1.0f, 0.0f, Color.Green.ToArgb()),
            new CustomVertex.PositionColored(0.0f, 0.0f, 1.0f, Color.Blue.ToArgb()), // blue = +z Axis
            new CustomVertex.PositionColored(0.0f, 0.0f, 0.0f, Color.Blue.ToArgb())
        };

        private readonly VertexBuffer vertexBuffer;

        public TgcAxisLines()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), lineVertices.Length,
                D3DDevice.Instance.Device, Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            vertexBuffer.Created += onVertexBufferCreate;
            onVertexBufferCreate(vertexBuffer, null);

            Enable = true;
        }

        /// <summary>
        ///     Habilita/Deshabilita el dibujado de los ejes cartesianos
        /// </summary>
        public bool Enable { get; set; }

        private void onVertexBufferCreate(object sender, EventArgs e)
        {
            var buffer = (VertexBuffer)sender;
            buffer.SetData(lineVertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Renderizar ejes segun posicion actual de la camara
        /// </summary>
        public void render()
        {
            //Obtener World coordinate de la esquina inferior de la pantalla
            var w = D3DDevice.Instance.Device.Viewport.Width;
            var h = D3DDevice.Instance.Device.Viewport.Height;
            var sx = AXIS_POS_OFFSET;
            var sy = h - AXIS_POS_OFFSET;

            var matProj = D3DDevice.Instance.Device.Transform.Projection;
            var v = new Vector3();
            v.X = (2.0f * sx / w - 1) / matProj.M11;
            v.Y = -(2.0f * sy / h - 1) / matProj.M22;
            v.Z = 1.0f;

            //Transform the screen space into 3D space
            var m = Matrix.Invert(D3DDevice.Instance.Device.Transform.View);
            var rayDir = new Vector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33
                );
            var rayOrig = new Vector3(m.M41, m.M42, m.M43);
            var worldCoordPos = rayOrig + AXIS_POS_DISTANCE * rayDir;

            //Renderizar
            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);
            D3DDevice.Instance.Device.Material = D3DDevice.DEFAULT_MATERIAL;
            D3DDevice.Instance.Device.Transform.World = Matrix.Translation(worldCoordPos);

            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.LineList, 0, 3);

            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
        }
    }
}