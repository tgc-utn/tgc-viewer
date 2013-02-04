using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;

namespace TgcViewer.Utils.Sound
{
    /// <summary>
    /// Herramienta para manipular el Device de DirectSound
    /// </summary>
    public class TgcDirectSound
    {

        Device dsDevice;
        /// <summary>
        /// Device de DirectSound
        /// </summary>
        public Device DsDevice
        {
            get { return dsDevice; }
        }

        private Listener3D listener3d;
        /// <summary>
        /// Representa el objeto central del universo 3D que escucha todos los demás sonidos.
        /// En base a su posición varía la captación de todos los demas sonidos 3D.
        /// </summary>
        public Listener3D Listener3d
        {
            get { return listener3d; }
        }

        private ITransformObject listenerTracking;
        /// <summary>
        /// Objeto al cual el Listener3D va a seguir para variar su posición en cada cuadro.
        /// Solo puede haber un objeto que está siendo seguido por el Listener3D a la vez.
        /// En caso de haber configurado un objeto a seguir, el Listener3D actualiza su posición en forma
        /// automática en cada cuadro.
        /// </summary>
        public ITransformObject ListenerTracking
        {
            get { return listenerTracking; }
            set { listenerTracking = value; }
        }

        private Microsoft.DirectX.DirectSound.Buffer primaryBuffer;

        public TgcDirectSound()
        {
            //Crear device de DirectSound
            dsDevice = new Device();
            dsDevice.SetCooperativeLevel(GuiController.Instance.MainForm, CooperativeLevel.Normal);

            //Crear Listener3D
            BufferDescription primaryBufferDesc = new BufferDescription();
            primaryBufferDesc.Control3D = true;
            primaryBufferDesc.PrimaryBuffer = true;
            primaryBuffer = new Microsoft.DirectX.DirectSound.Buffer(primaryBufferDesc, dsDevice);
            listener3d = new Listener3D(primaryBuffer);
            listener3d.Position = new Vector3(0f, 0f, 0f);
            listener3d.Orientation = new Listener3DOrientation(new Vector3(1, 0, 0), new Vector3(0, 1, 0));
        }

        /// <summary>
        /// Actualiza la posición del Listener3D en base al ListenerTracking
        /// </summary>
        internal void updateListener3d()
        {
            if (listenerTracking != null)
            {
                listener3d.Position = listenerTracking.Position;
            }
        }




    }
}
