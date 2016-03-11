namespace TGC.Core.Interpolation
{
    /// <summary>
    ///     Utilidad para interpolar un valor que arranca en INIT y va hasta END, a una velocidad determinada.
    ///     El metodo isEnd() indica si termino.
    /// </summary>
    public class InterpoladorLineal
    {
        /// <summary>
        ///     Valor inicial
        /// </summary>
        public float Init { get; set; }

        /// <summary>
        ///     Valor actual
        /// </summary>
        public float Current { get; set; }

        /// <summary>
        ///     Valor final
        /// </summary>
        public float End { get; set; }

        /// <summary>
        ///     Velocidad de incremento en segundos
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        ///     Cargar valores iniciales del interpolador
        /// </summary>
        public void reset()
        {
            Current = Init;
        }

        /// <summary>
        ///     Interpolar y devolver incremento.
        ///     Llamar a reset() la primera vez.
        /// </summary>
        /// <returns>Valor actual</returns>
        public float update(float elapsedTime)
        {
            var n = Speed * elapsedTime;
            Current += n;
            if (Speed > 0)
            {
                if (Current > End)
                {
                    Current = End;
                }
            }
            else
            {
                if (Current < End)
                {
                    Current = End;
                }
            }

            return Current;
        }

        /// <summary>
        ///     Indica si el interpolador llego al final
        /// </summary>
        public bool isEnd()
        {
            return Current == End;
        }
    }
}