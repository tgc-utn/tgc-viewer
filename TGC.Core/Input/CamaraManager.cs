namespace TGC.Core.Input
{
    public class CamaraManager
    {
        private CamaraManager()
        {
        }

        /// <summary>
        ///     Permite acceder a una instancia de la clase TgcShaders desde cualquier parte del codigo.
        /// </summary>
        public static CamaraManager Instance { get; } = new CamaraManager();

        /// <summary>
        ///     Cámara actual que utiliza el framework
        /// </summary>
        public TgcCamera CurrentCamera { get; set; }
    }
}