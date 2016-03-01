using Microsoft.DirectX;

namespace TGC.Viewer.Utils.Interpolation
{
    /// <summary>
    ///     Utilidad para interpolar linealmente entre dos posiciones 2D
    /// </summary>
    public class Position2dInterpolator
    {
        private Vector2 current;
        private Vector2 dir;
        private float distanceToTravel;

        /// <summary>
        ///     Velocidad de desplazamiento en segundos
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        ///     Posicion inicial
        /// </summary>
        public Vector2 Init { get; set; }

        /// <summary>
        ///     Posicion final
        /// </summary>
        public Vector2 End { get; set; }

        /// <summary>
        ///     Cargar valores iniciales del interpolador
        /// </summary>
        public void reset()
        {
            dir = End - Init;
            distanceToTravel = dir.Length();
            dir.Normalize();
            current = Init;
        }

        /// <summary>
        ///     Actualizar estado del interpolador.
        ///     Llamar a reset() la primera vez.
        /// </summary>
        /// <returns>Nueva posicion</returns>
        public Vector2 update()
        {
            var movement = Speed * GuiController.Instance.ElapsedTime;
            distanceToTravel -= movement;
            if (distanceToTravel < 0)
            {
                distanceToTravel = 0;
                current = End;
            }
            else
            {
                current += Vector2.Scale(dir, movement);
            }
            return current;
        }

        /// <summary>
        ///     Indica si el interpolador llego al final
        /// </summary>
        public bool isEnd()
        {
            return distanceToTravel == 0;
        }
    }
}