namespace TGC.Core.Interpolation
{
    /// <summary>
    ///     Utilidad para interpolar linealmente un valor entre un MIN y un MAX, variando en forma de vaiven (subi y baja)
    ///     a una velocidad determinada.
    ///     Nunca termina
    /// </summary>
    public class InterpoladorVaiven
    {
        /// <summary>
        ///     Valor minimo
        /// </summary>
        public float Min { get; set; }

        /// <summary>
        ///     Valor maximo
        /// </summary>
        public float Max { get; set; }

        /// <summary>
        ///     Velocidad de incremento en segundos
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        ///     Valor actual
        /// </summary>
        public float Current { get; set; }

        /// <summary>
        ///     Cargar valores iniciales del interpolador
        /// </summary>
        public void reset()
        {
            Current = Min;
        }

        /// <summary>
        ///     Interpolar y devolver incremento.
        ///     Llamar a reset() la primera vez.
        ///     <returns>Valor actual</returns>
        /// </summary>
        public float update(float elapsedTime)
        {
            var n = Speed * elapsedTime;
            Current += n;
            if (Current > Max)
            {
                Speed *= -1;
                Current = Max;
            }
            else if (Current < Min)
            {
                Speed *= -1;
                Current = Min;
            }

            return Current;
        }
    }
}