using Microsoft.DirectX.DirectSound;
using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Herramienta para manipular el Device de DirectSound
    /// </summary>
    public class TgcDirectSound
    {
        private Buffer primaryBuffer;

        /// <summary>
        ///     Device de DirectSound
        /// </summary>
        public Device DsDevice { get; set; }

        /// <summary>
        ///     Representa el objeto central del universo 3D que escucha todos los demás sonidos.
        ///     En base a su posición varía la captación de todos los demas sonidos 3D.
        /// </summary>
        public Listener3D Listener3d { get; set; }

        /// <summary>
        ///     Objeto al cual el Listener3D va a seguir para variar su posición en cada cuadro.
        ///     Solo puede haber un objeto que está siendo seguido por el Listener3D a la vez.
        ///     En caso de haber configurado un objeto a seguir, el Listener3D actualiza su posición en forma
        ///     automática en cada cuadro.
        /// </summary>
        public ITransformObject ListenerTracking { get; set; }

        /// <summary>
        ///     Actualiza la posición del Listener3D en base al ListenerTracking
        /// </summary>
        public void UpdateListener3d()
        {
            if (ListenerTracking != null)
            {
                Listener3d.Position = ListenerTracking.Position;
            }
        }

        public void InitializeD3DDevice(Control control)
        {
            //Crear device de DirectSound
            DsDevice = new Device();
            DsDevice.SetCooperativeLevel(control, CooperativeLevel.Normal);

            //Crear Listener3D
            var primaryBufferDesc = new BufferDescription();
            primaryBufferDesc.Control3D = true;
            primaryBufferDesc.PrimaryBuffer = true;
            primaryBuffer = new Buffer(primaryBufferDesc, DsDevice);
            Listener3d = new Listener3D(primaryBuffer);
            Listener3d.Position = TGCVector3.Empty;
            Listener3d.Orientation = new Listener3DOrientation(new TGCVector3(1, 0, 0), TGCVector3.Up);
        }
    }
}