namespace TgcViewer.Utils.Interpolation
{
    /// <summary>
    ///     Utilidad para controlar el avance del tiempo hasta un tope determinado (como una cuenta regresiva)
    /// </summary>
    public class Temporizador
    {
        /// <summary>
        ///     Valor actual
        /// </summary>
        public float Current { get; set; }

        /// <summary>
        ///     Fin de la cuenta regresiva, en segundos.
        /// </summary>
        public float StopSegs { get; set; }

        /// <summary>
        ///     Cargar valores iniciales del temporizador
        /// </summary>
        public void reset()
        {
            Current = 0;
        }

        /// <summary>
        ///     Avanzar el tiempo
        /// </summary>
        /// <returns>True si llego a su fin</returns>
        public bool update()
        {
            Current += GuiController.Instance.ElapsedTime;
            if (Current > StopSegs)
            {
                Current = StopSegs;
                return true;
            }
            return false;
        }
    }
}