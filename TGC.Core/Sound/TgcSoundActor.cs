using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Actor de sonido de TGC.
    ///     Cualquier objeto que emite sonido o lo escucha es considerado un actor.
    /// </summary>
    public abstract class TgcSoundActor
    {
        /// <summary>
        ///     Objeto que contiene la posicion del actor
        /// </summary>
        protected ITransformObject origin;

        internal abstract void Register();

        internal abstract void Unregister();

        internal abstract void UpdatePosition();

        /// <summary>
        ///     Asigna la orientacion del actor.
        ///     Ambos vectores deben estar normalizados y deben ser ortonormales.
        ///     Debe ser llamada cada vez que cambia la orientacion del actor.
        /// </summary>
        /// <param name="front">Direccion del frente del objeto</param>
        /// <param name="top">Direccion hacia arriba del objeto</param>
        public abstract void SetOrientation(TGCVector3 front, TGCVector3 top);

        /// <summary>
        ///     Libera los recursos del actor
        /// </summary>
        public abstract void Dispose();

    }
}
