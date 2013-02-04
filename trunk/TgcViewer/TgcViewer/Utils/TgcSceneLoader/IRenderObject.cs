using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.TgcSceneLoader
{
    /// <summary>
    /// Interfaz generica para renderizar objetos
    /// </summary>
    public interface IRenderObject
    {
        /// <summary>
        /// Renderiza el objeto
        /// </summary>
        void render();

        /// <summary>
        /// Libera los recursos del objeto
        /// </summary>
        void dispose();

        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por v�rtice de canal Alpha.
        /// Por default est� deshabilitado.
        /// </summary>
        bool AlphaBlendEnable
        {
            get;
            set;
        }
    }
}
