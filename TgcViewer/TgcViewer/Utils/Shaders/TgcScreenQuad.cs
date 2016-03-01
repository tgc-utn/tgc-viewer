using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;

namespace TGC.Viewer.Utils.Shaders
{
    /// <summary>
    ///     Utilidad para crear y renderizar un FullScreen Quad, útil para efectos de post-procesado
    /// </summary>
    public class TgcScreenQuad
    {
        /// <summary>
        ///     Crear quad
        /// </summary>
        public TgcScreenQuad()
        {
            //Se crean 2 triangulos (o Quad) con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x = 1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] screenQuadVertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            ScreenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            ScreenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     VertexBuffer del quad
        /// </summary>
        public VertexBuffer ScreenQuadVB { get; set; }

        /// <summary>
        ///     Render de quad con shader.
        ///     Setear previamente todos los parámetros de shader y technique correspondiente.
        ///     Limpiar la pantalla segun sea necesario
        /// </summary>
        public void render(Effect effect)
        {
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, ScreenQuadVB, 0);

            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Liberar recursos
        /// </summary>
        public void dispose()
        {
            ScreenQuadVB.Dispose();
        }
    }
}