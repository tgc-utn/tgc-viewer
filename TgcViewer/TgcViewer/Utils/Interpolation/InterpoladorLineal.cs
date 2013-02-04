using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;

namespace TgcViewer.Utils.Interpolation
{
    /// <summary>
    /// Utilidad para interpolar un valor que arranca en INIT y va hasta END, a una velocidad determinada.
    /// El metodo isEnd() indica si termino.
    /// </summary>
    public class InterpoladorLineal
    {
        float init;
        /// <summary>
        /// Valor inicial
        /// </summary>
        public float Init
        {
            get { return init; }
            set { init = value; }
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

        float end;
        /// <summary>
        /// Valor final
        /// </summary>
        public float End
        {
            get { return end; }
            set { end = value; }
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

        /// <summary>
        /// Crear interpolador lineal
        /// </summary>
        public InterpoladorLineal()
        {
        }

        /// <summary>
        /// Cargar valores iniciales del interpolador
        /// </summary>
        public void reset()
        {
            this.current = init;
        }

        /// <summary>
        /// Interpolar y devolver incremento.
        /// Llamar a reset() la primera vez.
        /// </summary>
        /// <returns>Valor actual</returns>
        public float update()
        {
            float n = speed * GuiController.Instance.ElapsedTime;
            current += n;
            if (speed > 0)
            {
                if (current > end)
                {
                    current = end;
                }
            }
            else
            {
                if (current < end)
                {
                    current = end;
                }
            }

            return current;
        }

        /// <summary>
        /// Indica si el interpolador llego al final
        /// </summary>
        public bool isEnd()
        {
            return current == end;
        }

    }
}
