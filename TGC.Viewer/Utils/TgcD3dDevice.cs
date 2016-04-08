using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Windows.Forms;
using TGC.Core.Direct3D;

namespace TGC.Viewer.Utils
{
    public class TgcD3dDevice
    {
        public TgcD3dDevice(Control panel3d)
        {
            D3DDevice.Instance.AspectRatio = (float)panel3d.Width / panel3d.Height;

            var caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);
            Console.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

            CreateFlags flags;
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags = CreateFlags.HardwareVertexProcessing;
            else
                flags = CreateFlags.SoftwareVertexProcessing;

            var d3dpp = new PresentParameters();

            d3dpp.BackBufferFormat = Format.Unknown;
            d3dpp.SwapEffect = SwapEffect.Discard;
            d3dpp.Windowed = true;
            d3dpp.EnableAutoDepthStencil = true;
            d3dpp.AutoDepthStencilFormat = DepthFormat.D24S8;
            d3dpp.PresentationInterval = PresentInterval.Immediate;

            //Antialiasing
            if (Manager.CheckDeviceMultiSampleType(Manager.Adapters.Default.Adapter, DeviceType.Hardware,
                Manager.Adapters.Default.CurrentDisplayMode.Format, true, MultiSampleType.NonMaskable))
            {
                d3dpp.MultiSample = MultiSampleType.NonMaskable;
                d3dpp.MultiSampleQuality = 0;
            }
            else
            {
                d3dpp.MultiSample = MultiSampleType.None;
            }

            //Crear Graphics Device
            Device.IsUsingEventHandlers = false;
            var d3DDevice = new Device(0, DeviceType.Hardware, panel3d, flags, d3dpp);
            d3DDevice.DeviceReset += OnResetDevice;

            D3DDevice.Instance.Device = d3DDevice;
        }

        /// <summary>
        ///     This event-handler is a good place to create and initialize any
        ///     Direct3D related objects, which may become invalid during a
        ///     device reset.
        /// </summary>
        public void OnResetDevice(object sender, EventArgs e)
        {
            GuiController.Instance.onResetDevice();
        }

        /// <summary>
        ///     Hace las operaciones de Reset del device
        /// </summary>
        internal void doResetDevice()
        {
            D3DDevice.Instance.setDefaultValues();

            //Reset Timer
            HighResolutionTimer.Instance.Reset();
        }

        internal void doClear()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, D3DDevice.Instance.ClearColor, 1.0f,
                0);
            HighResolutionTimer.Instance.Set();
        }

        internal void resetWorldTransofrm()
        {
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
        }

        /// <summary>
        ///     Liberar Device al finalizar la aplicacion
        /// </summary>
        internal void shutDown()
        {
            D3DDevice.Instance.Device.Dispose();
        }
    }
}