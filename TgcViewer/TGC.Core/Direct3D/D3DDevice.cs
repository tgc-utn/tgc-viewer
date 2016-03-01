using Microsoft.DirectX.Direct3D;

namespace TGC.Core.Direct3D
{
    public class D3DDevice
    {
        /// <summary>
        ///     Device de DirectX 3D para crear primitivas
        /// </summary>
        public Device device;

        /// <summary> Constructor privado para poder hacer el singleton</summary>
        private D3DDevice()
        {
        }

        public static D3DDevice Instance { get; } = new D3DDevice();

        public Device Device
        {
            get { return device; }
            set { device = value; }
        }
    }
}