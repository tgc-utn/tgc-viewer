using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer;

namespace TgcViewer.Utils.Interpolation
{
    /// <summary>
    /// Utilidad para interpolar linealmente un valor entre un MIN y un MAX, variando en forma de vaiven (subi y baja)
    /// a una velocidad determinada.
    /// Nunca termina
    /// </summary>
    public class InterpoladorVaiven
    {
        float min;
        /// <summary>
        /// Valor minimo
        /// </summary>
        public float Min
        {
            get { return min; }
            set { min = value; }
        }

        float max;
        /// <summary>
        /// Valor maximo
        /// </summary>
        public float Max
        {
            get { return max; }
            set { max = value; }
        }

        float speed;
        /// <summary>
        /// Velocidad de incremento en segundos
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        float current;
        /// <summary>
        /// Valor actual
        /// </summary>
        public float Current
        {
            get { return current; }
            set { current = value; }
        }

        /// <summary>
        /// Crear interpolador
        /// </summary>
        public InterpoladorVaiven()
        {
        }

        /// <summary>
        /// Cargar valores iniciales del interpolador
        /// </summary>
        public void reset()
        {
            this.current = this.min;
        }

        /// <summary>
        /// Interpolar y devolver incremento.
        /// Llamar a reset() la primera vez.
        /// <returns>Valor actual</returns>
        /// </summary>
        public float update()
        {
            float n = speed * GuiController.Instance.ElapsedTime;
            current += n;
            if (current > max)
            {
                speed *= -1;
                current = max;
            }
            else if (current < min)
            {
                speed *= -1;
                current = min;
            }

            return current;
        }

    }
}
