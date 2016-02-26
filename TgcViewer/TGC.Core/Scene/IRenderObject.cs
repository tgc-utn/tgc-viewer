namespace TGC.Core.Scene
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
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        bool AlphaBlendEnable
        {
            get;
            set;
        }
    }
}