using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.Direct3D;

namespace TgcViewer.Utils.Shaders
{
    /// <summary>
    /// Utilidad para crear y renderizar un FullScreen Quad, útil para efectos de post-procesado
    /// </summary>
    public class TgcScreenQuad
    {
        VertexBuffer screenQuadVB;
        /// <summary>
        /// VertexBuffer del quad
        /// </summary>
        public VertexBuffer ScreenQuadVB
        {
            get { return screenQuadVB; }
            set { screenQuadVB = value; }
        }

        /// <summary>
        /// Crear quad
        /// </summary>
        public TgcScreenQuad()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Se crean 2 triangulos (o Quad) con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x = 1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] screenQuadVertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);
        }

        /// <summary>
        /// Render de quad con shader.
        /// Setear previamente todos los parámetros de shader y technique correspondiente.
        /// Limpiar la pantalla segun sea necesario
        /// </summary>
        public void render(Effect effect)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void dispose()
        {
            screenQuadVB.Dispose();
        }

    }
}
