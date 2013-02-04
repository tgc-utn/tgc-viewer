using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace TgcViewer.Utils.Fog
{
    /// <summary>
    /// Herramienta para manipular el efecto de Niebla provisto por Direct3D
    /// </summary>
    public class TgcFog
    {
        private float startDistance;
        /// <summary>
        /// Distancia desde la que comienza a haber niebla
        /// </summary>
        public float StartDistance
        {
            get { return startDistance; }
            set { startDistance = value; }
        }

        private float endDistance;
        /// <summary>
        /// Distancia hasta la que llega la niebla
        /// </summary>
        public float EndDistance
        {
            get { return endDistance; }
            set { endDistance = value; }
        }

        private float density;
        /// <summary>
        /// Densidad del efecto
        /// </summary>
        public float Density
        {
            get { return density; }
            set { density = value; }
        }

        private bool enabled;
        /// <summary>
        /// Habilita o deshabilita el efecto de niebla.
        /// Es necesario llamar a updateValues() para que realmente se aplica el efecto.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private Color color;
        /// <summary>
        /// Color de la niebla
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }


        public TgcFog()
        {
            resetValues();
        }

        /// <summary>
        /// Configura los valores iniciales del efecto
        /// </summary>
        public void resetValues()
        {
            enabled = false;
            startDistance = 2.0f;
            endDistance = 10.0f;
            color = Color.Gray;
        }

        /// <summary>
        /// Actualiza todos los valores de la niebla.
        /// Activa o desactiva efectivamente el efecto, según como se haya configurado.
        /// </summary>
        public void updateValues()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            if(enabled)
            {
                d3dDevice.SetRenderState(RenderStates.FogEnable, true);
                d3dDevice.SetRenderState(RenderStates.RangeFogEnable, true);
                d3dDevice.SetRenderState(RenderStates.FogColor, color.ToArgb());
                d3dDevice.SetRenderState(RenderStates.FogVertexMode, (int)FogMode.Linear);
                d3dDevice.SetRenderState(RenderStates.FogStart, startDistance);
                d3dDevice.SetRenderState(RenderStates.FogEnd, endDistance);
                d3dDevice.SetRenderState(RenderStates.FogDensity, density);
            }
            else
            {
                d3dDevice.SetRenderState(RenderStates.FogEnable, false);
            }
        }



    }
}
