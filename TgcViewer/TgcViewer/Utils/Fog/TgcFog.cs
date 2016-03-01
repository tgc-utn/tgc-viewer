using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;

namespace TGC.Viewer.Utils.Fog
{
    /// <summary>
    ///     Herramienta para manipular el efecto de Niebla provisto por Direct3D
    /// </summary>
    public class TgcFog
    {
        private Color color;

        public TgcFog()
        {
            resetValues();
        }

        /// <summary>
        ///     Distancia desde la que comienza a haber niebla
        /// </summary>
        public float StartDistance { get; set; }

        /// <summary>
        ///     Distancia hasta la que llega la niebla
        /// </summary>
        public float EndDistance { get; set; }

        /// <summary>
        ///     Densidad del efecto
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        ///     Habilita o deshabilita el efecto de niebla.
        ///     Es necesario llamar a updateValues() para que realmente se aplica el efecto.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Color de la niebla
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        ///     Configura los valores iniciales del efecto
        /// </summary>
        public void resetValues()
        {
            Enabled = false;
            StartDistance = 2.0f;
            EndDistance = 10.0f;
            color = Color.Gray;
        }

        /// <summary>
        ///     Actualiza todos los valores de la niebla.
        ///     Activa o desactiva efectivamente el efecto, según como se haya configurado.
        /// </summary>
        public void updateValues()
        {
            if (Enabled)
            {
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogEnable, true);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.RangeFogEnable, true);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogColor, color.ToArgb());
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogVertexMode, (int)FogMode.Linear);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogStart, StartDistance);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogEnd, EndDistance);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogDensity, Density);
            }
            else
            {
                D3DDevice.Instance.Device.SetRenderState(RenderStates.FogEnable, false);
            }
        }
    }
}