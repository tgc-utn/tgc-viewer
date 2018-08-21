using System.Windows.Forms;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
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
        private PrimarySoundBuffer primaryBuffer;

        /// <summary>
        ///     Device de DirectSound
        /// </summary>
        public DirectSound DsDevice { get; set; }

        /// <summary>
        ///     Representa el objeto central del universo 3D que escucha todos los dem�s sonidos.
        ///     En base a su posici�n var�a la captaci�n de todos los demas sonidos 3D.
        /// </summary>
        public SoundListener3D Listener3d { get; set; }

        /// <summary>
        ///     Objeto al cual el Listener3D va a seguir para variar su posici�n en cada cuadro.
        ///     Solo puede haber un objeto que est� siendo seguido por el Listener3D a la vez.
        ///     En caso de haber configurado un objeto a seguir, el Listener3D actualiza su posici�n en forma
        ///     autom�tica en cada cuadro.
        /// </summary>
        public ITransformObject ListenerTracking { get; set; }

        /// <summary>
        ///     Actualiza la posici�n del Listener3D en base al ListenerTracking
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
            DsDevice = new DirectSound();
            DsDevice.SetCooperativeLevel(control.Handle, CooperativeLevel.Normal);

            //Crear Listener3D

            var primaryBufferDesc = new SoundBufferDescription();
            primaryBufferDesc.Flags = BufferFlags.Control3D | BufferFlags.PrimaryBuffer;
            primaryBuffer = new PrimarySoundBuffer(DsDevice, primaryBufferDesc);
            Listener3d = new SoundListener3D(primaryBuffer);
            Listener3d.Position = TGCVector3.Empty;
            Listener3d.Orientation = new Listener3DOrientation(new TGCVector3(1, 0, 0), TGCVector3.Up);

        }
    } 
}